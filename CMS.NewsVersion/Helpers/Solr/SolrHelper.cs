using Newtonsoft.Json;
using System.Net;
using System.Text;


namespace CMS.NewsVersion.Helpers.Solr
{
    public static class SolrHelper
    {
        public static async Task<dynamic> ExecuteQuery(SolrSettings settings, string q)
        {
            var solrCoreUrl = settings.Url;

            if (!string.IsNullOrWhiteSpace(solrCoreUrl) && !string.IsNullOrWhiteSpace(q))
            {
                if (q.Contains("wt="))
                {
                    q = q.Replace("wt=json", string.Empty).Replace("wt=xml", string.Empty).Replace("wt=python", string.Empty)
                         .Replace("wt=ruby", string.Empty).Replace("wt=php", string.Empty).Replace("wt=csv", string.Empty);
                }

                string address = string.Format("{0}select{1}&wt=json", solrCoreUrl, q);

                using (var client = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    try
                    {
                        return await Task.FromResult(JsonConvert.DeserializeObject<dynamic>(client.DownloadString(new Uri(address))));
                    }
                    catch (WebException)
                    { }
                    catch (System.Exception)
                    { }
                }
            }

            return await Task.FromResult(JsonConvert.DeserializeObject<dynamic>("{}"));
        }

        public static async Task<dynamic> DataImport(SolrSettings settings, bool isFull = false)
        {
            var solrCoreUrl = settings.Url;

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

        public static async Task<dynamic> DeleteDocumentById(SolrSettings settings, int id)
        {
            var solrCoreUrl = settings.Url; 

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

        public static async Task<dynamic> DeleteDocumentByQuery(SolrSettings settings, string q)
        {
            var solrCoreUrl = settings.Url;

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

        public static async Task<dynamic> DeleteAllDocuments(SolrSettings settings)
        {
            var solrCoreUrl = settings.Url;

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

        public static async Task<bool> AddLayoutInstanceByNode(SolrSettings settings, object obj)
        {
            var solrCoreUrl = settings.Url;

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

      

      
    }

    public class SolrSettings
    {
        public string Url { get; set; }
    }
}