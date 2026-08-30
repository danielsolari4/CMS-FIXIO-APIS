using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ray.Dtos.Configuration;
using Ray.Utils.Logging;

namespace Ray.Utils.Cache
{
    public static class FrontendCacheClient
    {
        private static readonly HttpClient Client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

        public static async Task<CacheInvalidationResult> Invalidate(
            IEnumerable<string> paths,
            AppSettings appSettings,
            string reason = null,
            CacheInvalidationKind kind = CacheInvalidationKind.News)
        {
            var result = new CacheInvalidationResult { Reason = reason };
            var normalized = NormalizePaths(paths);
            result.Paths = normalized;

            if (normalized.Count == 0)
                return result;

            var instances = GetFrontendInstances(appSettings);
            if (instances.Count == 0)
            {
                CMSLogger.Error("[cache-invalidation] no frontend url configured.");
                return result;
            }

            var token = GetToken(appSettings);
            var mode = string.IsNullOrWhiteSpace(token) ? CacheInvalidationMode.LegacyNext : CacheInvalidationMode.Batch;
            result.Mode = mode.ToString();

            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < instances.Count; i++)
            {
                var instance = mode == CacheInvalidationMode.Batch
                    ? await PostBatch(instances[i], normalized, purgeCdn: i == 0, token, reason, appSettings)
                    : await SendLegacy(instances[i], normalized, reason, kind, appSettings);

                result.Instances.Add(instance);
            }

            stopwatch.Stop();
            result.ElapsedMs = stopwatch.ElapsedMilliseconds;
            return result;
        }

        private static async Task<CacheInvalidationInstanceResult> PostBatch(
            string baseUrl,
            IList<string> paths,
            bool purgeCdn,
            string token,
            string reason,
            AppSettings appSettings)
        {
            var endpointPath = GetSetting("FrontEnd__CacheInvalidation__Endpoint");
            if (string.IsNullOrWhiteSpace(endpointPath))
                endpointPath = appSettings?.CacheInvalidation?.Endpoint;
            if (string.IsNullOrWhiteSpace(endpointPath))
                endpointPath = "/api/cache/invalidate";

            var payload = JsonConvert.SerializeObject(new
            {
                paths,
                purgeCdn,
                reason = reason ?? string.Empty
            });

            var url = baseUrl.TrimEnd('/') + endpointPath;
            var result = await SendWithRetries(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                };
                request.Headers.Add("Authorization", "Bearer " + token);
                return request;
            }, baseUrl, appSettings);

            result.PurgedCdn = purgeCdn;
            return result;
        }

        private static async Task<CacheInvalidationInstanceResult> SendLegacy(
            string baseUrl,
            IList<string> paths,
            string reason,
            CacheInvalidationKind kind,
            AppSettings appSettings)
        {
            var instanceResult = new CacheInvalidationInstanceResult { Url = baseUrl, PurgedCdn = true };
            var root = baseUrl.TrimEnd('/');
            var secret = GetSecret(appSettings);
            if (string.IsNullOrWhiteSpace(secret))
                secret = "1";

            var errors = new List<string>();
            foreach (var path in paths)
            {
                var url = $"{root}/api/revalidate?secret={Uri.EscapeDataString(secret)}&nocache={DateTime.UtcNow.Ticks}&path={Uri.EscapeDataString(path)}";
                var single = await SendWithRetries(() => new HttpRequestMessage(HttpMethod.Get, url), baseUrl, appSettings);
                instanceResult.Attempts += single.Attempts;

                if (!single.Success)
                {
                    instanceResult.StatusCode = single.StatusCode;
                    errors.Add($"{path}: {single.StatusCode} {single.Error}");
                }
            }

            instanceResult.Success = errors.Count == 0;
            if (errors.Count > 0)
                instanceResult.Error = string.Join(" | ", errors);

            return instanceResult;
        }

        private static async Task<CacheInvalidationInstanceResult> SendWithRetries(Func<HttpRequestMessage> requestFactory, string baseUrl, AppSettings appSettings)
        {
            var result = new CacheInvalidationInstanceResult { Url = baseUrl };
            var maxAttempts = Math.Max(1, GetMaxAttempts(appSettings));

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                result.Attempts = attempt;
                try
                {
                    using var request = requestFactory();
                    var response = await Client.SendAsync(request);
                    result.StatusCode = (int)response.StatusCode;
                    result.Success = response.IsSuccessStatusCode;
                    result.Error = response.IsSuccessStatusCode ? null : await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode || !IsRetryable(result.StatusCode))
                        return result;
                }
                catch (System.Exception ex)
                {
                    result.Success = false;
                    result.Error = ex.Message;
                }

                if (attempt < maxAttempts)
                    await Task.Delay((int)(1000 * Math.Pow(3, attempt - 1)));
            }

            CMSLogger.Error($"[cache-invalidation] failed against {baseUrl}: {result.StatusCode} {result.Error}");
            return result;
        }

        internal static List<string> NormalizePaths(IEnumerable<string> paths)
        {
            var normalized = new List<string>();
            if (paths == null)
                return normalized;

            foreach (var path in paths)
            {
                if (string.IsNullOrWhiteSpace(path))
                    continue;

                var value = path.Trim();
                if (HasPlaceholderSegment(value))
                    continue;

                if (!value.StartsWith("/"))
                    value = "/" + value;

                while (value.Contains("//"))
                    value = value.Replace("//", "/");

                if (value.Length > 1)
                    value = value.TrimEnd('/');

                if (!normalized.Contains(value, StringComparer.Ordinal))
                    normalized.Add(value);
            }

            return normalized;
        }

        private static List<string> GetFrontendInstances(AppSettings appSettings)
        {
            var instances = new List<string>();

            if (appSettings?.CacheInvalidation?.Instances != null && appSettings.CacheInvalidation.Instances.Count > 0)
                instances.AddRange(appSettings.CacheInvalidation.Instances.Where(x => !string.IsNullOrWhiteSpace(x)));

            if (instances.Count == 0 && !string.IsNullOrWhiteSpace(appSettings?.CacheInvalidation?.Url))
                instances.Add(appSettings.CacheInvalidation.Url.Trim());

            var envInstances = GetSetting("FrontEnd__CacheInvalidation__Instances");
            if (instances.Count == 0 && !string.IsNullOrWhiteSpace(envInstances))
                instances.AddRange(envInstances.Split('|'));

            if (instances.Count == 0 && !string.IsNullOrWhiteSpace(GetSetting("FrontEnd__Url")))
                instances.Add(GetSetting("FrontEnd__Url").Trim());

            if (instances.Count == 0 && !string.IsNullOrWhiteSpace(appSettings?.Content?.FrontendUrl))
                instances.Add(appSettings.Content.FrontendUrl.Trim());

            return instances
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string GetToken(AppSettings appSettings)
        {
            return GetSetting("FrontEnd__CacheInvalidation__Token")
                ?? appSettings?.CacheInvalidation?.Token;
        }

        private static string GetSecret(AppSettings appSettings)
        {
            var secret = GetSetting("FrontEnd__SecretCacheKey")
                ?? appSettings?.CacheInvalidation?.SecretCacheKey;
            return string.IsNullOrWhiteSpace(secret) ? "1" : secret;
        }

        private static int GetMaxAttempts(AppSettings appSettings)
        {
            if (appSettings?.CacheInvalidation?.MaxAttempts.HasValue == true)
                return appSettings.CacheInvalidation.MaxAttempts.Value;

            var raw = GetSetting("FrontEnd__CacheInvalidation__MaxAttempts");
            return int.TryParse(raw, out var parsed) ? parsed : 3;
        }

        private static bool HasPlaceholderSegment(string path)
        {
            return path.Split('/').Any(segment =>
                segment.Equals("undefined", StringComparison.OrdinalIgnoreCase) ||
                segment.Equals("null", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsRetryable(int statusCode)
        {
            return statusCode == 0 || statusCode == 207 || statusCode == 408 || statusCode == 429 || statusCode >= 500;
        }

        private static string GetSetting(string key)
        {
            return Environment.GetEnvironmentVariable(key);
        }
    }

    public enum CacheInvalidationMode
    {
        Batch,
        LegacyNext
    }

    public enum CacheInvalidationKind
    {
        News,
        Layout
    }

    public class CacheInvalidationResult
    {
        public CacheInvalidationResult()
        {
            Paths = new List<string>();
            Instances = new List<CacheInvalidationInstanceResult>();
        }

        public string Reason { get; set; }
        public string Mode { get; set; }
        public IList<string> Paths { get; set; }
        public IList<CacheInvalidationInstanceResult> Instances { get; set; }
        public long ElapsedMs { get; set; }
        public bool Success => Instances.Count > 0 && Instances.All(x => x.Success);
    }

    public class CacheInvalidationInstanceResult
    {
        public string Url { get; set; }
        public bool Success { get; set; }
        public bool PurgedCdn { get; set; }
        public int StatusCode { get; set; }
        public int Attempts { get; set; }
        public string Error { get; set; }
    }
}
