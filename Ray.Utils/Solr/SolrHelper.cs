using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ray.Dtos.Configuration;
using Ray.Utils.Configuration;

namespace Ray.Utils.Solr
{
    public static class SolrHelper
    {
        public static async Task<dynamic> ExecuteQuery(SolrCore core, string q, SolrConfig config)
        {
            var solrCoreUrl = GetCoreUrl(core, config);

            if (!string.IsNullOrWhiteSpace(solrCoreUrl) && !string.IsNullOrWhiteSpace(q))
            {
                if (q.Contains("wt="))
                {
                    q = q.Replace("wt=json", string.Empty).Replace("wt=xml", string.Empty).Replace("wt=python", string.Empty)
                         .Replace("wt=ruby", string.Empty).Replace("wt=php", string.Empty).Replace("wt=csv", string.Empty);
                }

                string address = string.Format("{0}select?{1}&wt=json", solrCoreUrl, q.Replace("?",""));

                using (var client = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    try
                    {
                        var result = await Task.FromResult(client.DownloadString(new Uri(address)));
                        return JsonConvert.DeserializeObject(result.ToString());
                    }
                    catch (WebException)
                    { }
                    catch (System.Exception)
                    { }
                }
            }

            return await Task.FromResult(JsonConvert.DeserializeObject<dynamic>("{}"));
        }

        public static async Task<dynamic> DataImport(SolrCore core, SolrConfig config, bool isFull = false)
        {
            var solrCoreUrl = GetCoreUrl(core, config);

            if (!string.IsNullOrWhiteSpace(solrCoreUrl))
            {
                if (isFull)
                {
                    var address = string.Format("{0}dataimport?command=full-import", solrCoreUrl);

                    using (var client = new WebClient())
                    {
                        try
                        {
                            var response = await Task.FromResult((client.DownloadString(new Uri(address))));
                            if (response.Contains("<int name=\"status\">0</int>"))
                                return true;
                        }
                        catch (WebException)
                        { }
                        catch (System.Exception)
                        { }
                    }
                }
                else
                {
                    var address = string.Format("{0}dataimport?command=delta-import", solrCoreUrl);

                    using (var client = new WebClient())
                    {
                        try
                        {
                            var response = await Task.FromResult((client.DownloadString(new Uri(address))));
                            if (response.Contains("<int name=\"status\">0</int>"))
                                return true;
                        }
                        catch (WebException)
                        { }
                        catch (System.Exception)
                        { }
                    }
                }
            }

            return false;
        }

        public static async Task<dynamic> DeleteDocumentById(SolrCore core, int id, SolrConfig config)
        {
            var solrCoreUrl = GetCoreUrl(core, config);

            if (!string.IsNullOrWhiteSpace(solrCoreUrl))
            {
                var address = string.Format("{0}update?stream.body=<delete><query>Id:{1}</query></delete>&commit=true", solrCoreUrl, id);

                using (var client = new WebClient())
                {
                    try
                    {
                        var response = await Task.FromResult((client.DownloadString(new Uri(address))));
                        if (response.Contains("<int name=\"status\">0</int>"))
                            return true;
                    }
                    catch (WebException)
                    { }
                    catch (System.Exception)
                    { }
                }
            }

            return false;
        }

        public static async Task<dynamic> DeleteDocumentByQuery(SolrCore core, string q, SolrConfig config)
        {
            var solrCoreUrl = GetCoreUrl(core, config);

            if (!string.IsNullOrWhiteSpace(solrCoreUrl))
            {
                var address = string.Format("{0}update?stream.body=<delete><query>{1}</query></delete>&commit=true", solrCoreUrl, q);

                using (var client = new WebClient())
                {
                    try
                    {
                        var response = await Task.FromResult((client.DownloadString(new Uri(address))));
                        if (response.Contains("<int name=\"status\">0</int>"))
                            return true;
                    }
                    catch (WebException)
                    { }
                    catch (System.Exception)
                    { }
                }
            }

            return false;
        }

        public static async Task<dynamic> DeleteAllDocuments(SolrCore core, SolrConfig config)
        {
            var solrCoreUrl = GetCoreUrl(core,config);

            if (!string.IsNullOrWhiteSpace(solrCoreUrl))
            {
                var address = string.Format("{0}update?stream.body=<delete><query>*:*</query></delete>&commit=true", solrCoreUrl);

                using (var client = new WebClient())
                {
                    try
                    {
                        var response = await Task.FromResult((client.DownloadString(new Uri(address))));
                        if (response.Contains("<int name=\"status\">0</int>"))
                            return true;
                    }
                    catch (WebException)
                    { }
                    catch (System.Exception)
                    { }
                }
            }

            return false;
        }

        public static async Task<bool> AddLayoutInstanceByNode(SolrCore core, object obj, SolrConfig config)
        {
            var solrCoreUrl = GetCoreUrl(core, config);

            if (!string.IsNullOrWhiteSpace(solrCoreUrl))
            {
                var address = string.Format("{0}update?commit=true", solrCoreUrl);

                using (var client = new HttpClient())
                {
                    try
                    {

                        var json = JsonConvert.SerializeObject(obj, new Newtonsoft.Json.Converters.IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ" });
                        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

                        var result = await client.PostAsync(address, stringContent);

                        if (result.IsSuccessStatusCode)
                            return true;
                    }
                    catch (WebException)
                    { }
                    catch (System.Exception)
                    { }
                }
            }

            return false;
        }

        public static async Task<dynamic> GetLayoutInstanceByNodeId(int? nodeId, SolrConfig config)
        {
            var solrCoreUrl = GetCoreUrl(SolrCore.LAYOUTINSTANCEBYNODE, config);

            if (!string.IsNullOrWhiteSpace(solrCoreUrl) && nodeId != null)
            {
                string address = string.Format("{0}select?q=NodeId:{1}&wt=json", solrCoreUrl, nodeId);

                using (var client = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    try
                    {
                        var result = await Task.FromResult(client.DownloadString(new Uri(address)));
                        return JsonConvert.DeserializeObject(result.ToString());
                    }
                    catch (WebException)
                    { }
                    catch (System.Exception)
                    { }
                }
            }

            return await Task.FromResult(JsonConvert.DeserializeObject<dynamic>("{}"));
        }

        private static string GetCoreUrl(SolrCore core, SolrConfig config)
        {
            switch (core)
            {
                case SolrCore.AUTHOR: return $"{config.Url}{config.SolrCore.Author}/";
                case SolrCore.CATEGORY: return $"{config.Url}{config.SolrCore.Category}/";
                case SolrCore.GALLERY: return $"{config.Url}{config.SolrCore.Gallery}/";
                case SolrCore.KEYWORD: return $"{config.Url}{config.SolrCore.Keywords}/";
                case SolrCore.LAYOUT: return $"{config.Url}{config.SolrCore.Layout}/";
                case SolrCore.LAYOUTINSTANCE: return $"{config.Url}{config.SolrCore.LayoutInstance}/";
                case SolrCore.LAYOUTINSTANCEBYNODE: return $"{config.Url}{config.SolrCore.LayoutInstanceByNode}/";
                case SolrCore.MEDIA: return $"{config.Url}{config.SolrCore.Media}/";
                case SolrCore.NEWS: return $"{config.Url}{config.SolrCore.News}/";
                case SolrCore.PAGE: return $"{config.Url}{config.SolrCore.Pages}/";
                case SolrCore.NODE: return $"{config.Url}{config.SolrCore.Node}/";
                case SolrCore.USER: return $"{config.Url}{config.SolrCore.User}/";
                case SolrCore.PROGRAMMINGGUIDE: return $"{config.Url}{config.SolrCore.ProgrammingGuide}/";
                case SolrCore.CHANNEL: return $"{config.Url}{config.SolrCore.Channel}/";
                case SolrCore.URLREDIRECT: return $"{config.Url}{config.SolrCore.URLRedirect}/";
                case SolrCore.THEME: return $"{config.Url}{config.SolrCore.Theme}/";
                case SolrCore.WIDGET: return $"{config.Url}{config.SolrCore.Widget}/";
                case SolrCore.PRINTEDITION: return $"{config.Url}{config.SolrCore.PrintEdition}/";
                case SolrCore.SETTINGSCORE: return $"{config.Url}{config.SolrCore.SettingsCore}/";
                case SolrCore.MENU: return $"{config.Url}{config.SolrCore.Menu}/";
            }

            return string.Empty;
        }
    }

    public enum SolrCore
    {
        AUTHOR,
        CATEGORY,
        GALLERY,
        KEYWORD,
        LAYOUT,
        LAYOUTINSTANCE,
        LAYOUTINSTANCEBYNODE,
        MEDIA,
        PRINTEDITION,
        NEWS,
        PAGE,
        NODE,
        USER,
        PROGRAMMINGGUIDE,
        CHANNEL,
        URLREDIRECT,
        THEME,
        WIDGET,
        ACCESSTYPE,
        SETTINGSCORE,
        MENU
    }
}