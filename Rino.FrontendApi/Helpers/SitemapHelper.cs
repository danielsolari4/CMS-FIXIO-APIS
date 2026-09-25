using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rino.Dtos.Configuration;
using Rino.Utils.Solr;
using SolrCore = Rino.Utils.Solr.SolrCore;

namespace CMS.FrontendAPI.Helpers
{
    public static class SitemapHelper
    {
        public static string GetFrontUrl(string frontUrl)
        {
            var urlFront = frontUrl;
            if (urlFront.EndsWith("/"))
                return urlFront.Remove(urlFront.Length - 1);
            return urlFront;
        }

        public static async Task<string> GetNewsByMonth(int month, int year, SolrConfig config, string path, string frontUrl)
        {
            var monthFrom = month;
            var yearFrom = year;
            if (month == 12)
            {
                yearFrom += 1;
                monthFrom = 1;
            }
            else
                monthFrom++;

            var fq = $"PublicationDate:[{year}-{month}-01T00:00:00Z TO {yearFrom}-{monthFrom}-01T00:00:00Z]";
            var encodeUrl = HttpUtility.UrlEncode(fq);
            var news = await SolrHelper.ExecuteQuery(SolrCore.NEWS, "fl=Id,Url,PublicationDate,Nodes_en,LastModificationDate&indent=on&q=IsDeleted:false%20AND%20Status:PUBLISHED&fq=" + encodeUrl + "&rows=2147483647&sort=PublicationDate%20desc", config);
            return WriteXmlNews(news, month, year, path, frontUrl);
        }

        public static string WriteXmlNews(dynamic news, int year, int month, string path, string frontUrl)
        {
            var pathFile = path + $"sitemaps/sitemap_news_{year}_{month}.xml";

            var token = JObject.Parse(news.ToString());
            var childs = (dynamic)token.SelectToken("response");
            var tokenChilds = JObject.Parse(childs.ToString());
            var childDocs = ((JProperty)((JContainer)tokenChilds).Last).Value.ToList();
            var urlFront = frontUrl;
            if (urlFront.EndsWith("/"))
                urlFront = urlFront.Remove(urlFront.Length - 1);

            #region xml settings configuration

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };

            #endregion

            #region build xml file

            using (var writer = XmlWriter.Create(pathFile, settings))
            {
                writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

                writer.WriteAttributeString("xmlns", "news", null, "http://www.google.com/schemas/sitemap-news/0.9");

                foreach (var newItem in childDocs.ToArray())
                {
                    ParseShortXmlItem(newItem, writer, urlFront, "0.5", "always");
                }

                writer.WriteEndElement();
                writer.Flush();
            }

            #endregion

            return pathFile;
        }

        public static void PriorityFreq(string url, out string changefreq, out string priority)
        {
            var charC = url.Count(f => f == '/');
            switch (charC)
            {
                case 0:
                case 1:
                    changefreq = "hourly";
                    priority = "0.8";
                    break;
                case 2:
                    changefreq = "weekly";
                    priority = "0.7";
                    break;
                case 3:
                    changefreq = "monthly";
                    priority = "0.7";
                    break;
                case 4:
                    changefreq = "yearly";
                    priority = "0.6";
                    break;
                default:
                    changefreq = "yearly";
                    priority = "0.6";
                    break;
            }
        }

        public static void ParseShortXmlItem(JToken newItem, XmlWriter writer, string urlFront, string priority, string changefreq)
        {
            writer.WriteStartElement("url");

            var id = string.Empty;
            string url;
            var lastMod = string.Empty;


            if (newItem.SelectToken("Id") != null)
                id = ((JValue)newItem.SelectToken("Id")).Value.ToString();

            try
            {
                var nodeSlug = "seccion";
                if ((JArray)newItem.SelectToken("Nodes_en") != null)
                {
                    var node = ((JArray)newItem.SelectToken("Nodes_en")).FirstOrDefault()?.Value<string>();
                    nodeSlug = node != null ? Rino.Dtos.Components.ComponentNewDto.GenerateSlug(node) : "seccion";
                }

                if (newItem.SelectToken("Url") != null)
                {
                    url = ((JValue)newItem.SelectToken("Url")).Value.ToString();
                    if (!url.StartsWith("/"))
                        url = "/" + nodeSlug + "/" + url + "_" + id;
                    if (url.EndsWith("/") && url.Length > 1)
                        url = url.Remove(url.Length - 1);
                }
                else url = "/";

                if (string.IsNullOrEmpty(priority) && string.IsNullOrEmpty(changefreq))
                    PriorityFreq(url, out changefreq, out priority);

                if (newItem.SelectToken("LastModificationDate") != null)
                    lastMod = ((JValue)newItem.SelectToken("LastModificationDate")).Value.ToString();

                if (newItem.SelectToken("CreationModificationDate") != null)
                    lastMod = ((JValue)newItem.SelectToken("LastModificationDate")).Value.ToString();

                if (string.IsNullOrEmpty(lastMod))
                    lastMod = DateTime.Now.ToString();

                writer.WriteElementString("loc", $"{urlFront}{url}");
                writer.WriteElementString("lastmod", DateTime.Parse(lastMod).ToString("yyyy-MM-ddTHH:mm:ssssZ"));

                writer.WriteElementString("changefreq", changefreq);
                //writer.WriteElementString("priority", priority);

                writer.WriteEndElement();
                writer.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static void ParseCompleteXmlItem(JToken newItem, XmlWriter writer, string urlFront, string priority, string changefreq)
        {
            var id = string.Empty;
            var url = string.Empty;
            var lastMod = string.Empty;
            var title = string.Empty;
            var keywords = string.Empty;

            var node = ((JArray)newItem.SelectToken("Nodes_en")).FirstOrDefault()?.Value<string>();
            if (string.IsNullOrEmpty(node)) node = "seccion";

            var nodeSlug = Rino.Dtos.Components.ComponentNewDto.GenerateSlug(node);


            if (newItem.SelectToken("Id") != null)
                id = ((JValue)newItem.SelectToken("Id")).Value.ToString();

            if (newItem.SelectToken("Url") != null)
                url = ((JValue)newItem.SelectToken("Url")).Value.ToString();

            if (newItem.SelectToken("PublicationDate") != null)
                lastMod = ((JValue)newItem.SelectToken("PublicationDate")).Value.ToString();

            if (newItem.SelectToken("Title_en") != null)
                title = ((JValue)newItem.SelectToken("Title_en")).Value.ToString();

            if (newItem.SelectToken("Keywords_en") != null)
                keywords = ((JValue)newItem.SelectToken("Keywords_en")).Value.ToString();


            if (newItem.SelectToken("Url") != null)
            {
                url = ((JValue)newItem.SelectToken("Url")).Value.ToString();
                if (!url.StartsWith("/"))
                    url = "/" + nodeSlug + "/" + url + "_" + id;
                if (url.EndsWith("/") && url.Length > 1)
                    url = url.Remove(url.Length - 1);
            }
            else url = "/";

            #region Url node

            writer.WriteStartElement("url");

            writer.WriteElementString("loc", $"<![CDATA[ {urlFront}{url} ]]>");

            #region News node

            writer.WriteStartElement("news", "news", null);

            #region Node publication

            writer.WriteStartElement("news", "publication", null);

            writer.WriteElementString("news", "name", null, title);
            writer.WriteElementString("news", "language", null, "es");

            writer.WriteEndElement();

            #endregion

            writer.WriteElementString("news", "publication_date", null, DateTime.Parse(lastMod).ToString("yyyy-MM-ddTHH:mm:ssssZ"));
            writer.WriteElementString("news", "title", null, title);
            writer.WriteElementString("news", "keywords", null, keywords);

            writer.WriteEndElement();

            #endregion

            if (string.IsNullOrEmpty(priority) && string.IsNullOrEmpty(changefreq))
                PriorityFreq(url, out changefreq, out priority);

            writer.WriteElementString("changefreq", string.Empty);

            writer.WriteEndElement();

            writer.WriteElementString("priority", string.Empty);

            writer.WriteEndElement();

            #endregion

            writer.Flush();
        }


        public static string WriteGenericFile(List<ItemSitemap> items, string pathLocation, string frontUrl)
        {
            var pathFile = pathLocation + "sitemap.xml";


            #region xml settings configuration

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };

            #endregion

            #region build xml file

            using (var writer = XmlWriter.Create(pathFile, settings))
            {
                writer.WriteStartElement("sitemapindex", "http://www.sitemaps.org/schemas/sitemap/0.9");

                foreach (var newItem in items)
                {
                    SetItemSitemap(newItem, writer, frontUrl);
                }

                writer.WriteEndElement();
                writer.Flush();
            }

            #endregion

            return pathFile;
        }


        public static void SetItemSitemap(ItemSitemap item, XmlWriter writer, string frontUrl)
        {
            writer.WriteStartElement("sitemap");

            writer.WriteElementString("loc", $"{GetFrontUrl(frontUrl)}{item.Url}");

            if (!string.IsNullOrEmpty(item.Date))
                writer.WriteElementString("lastmod", DateTime.Parse(item.Date).ToString("yyyy-MM-ddTHH:mm:ssssZ"));

            writer.WriteEndElement();
            writer.Flush();
        }

        public static async Task<FilterSitemap> GetRangeDate(SolrConfig config)
        {
            var fieldEncode = "fq=" + HttpUtility.UrlEncode("PublicationDate:[* TO NOW]");
            var lastItemString = await SolrHelper.ExecuteQuery(SolrCore.NEWS, "fl=Id,Url,PublicationDate,LastModificationDate&indent=on&" + fieldEncode + "&q=IsDeleted:false%20AND%20Status:PUBLISHED&rows=1&sort=PublicationDate%20desc", config);
            var firstItemString = await SolrHelper.ExecuteQuery(SolrCore.NEWS, "fl=Id,Url,PublicationDate,LastModificationDate&indent=on&" + fieldEncode + "&q=IsDeleted:false%20AND%20Status:PUBLISHED&rows=1&sort=PublicationDate%20asc", config);
            var filter = new FilterSitemap();

            var lastItem = JsonConvert.DeserializeObject<dynamic>(lastItemString);
            var firstItem = JsonConvert.DeserializeObject<dynamic>(firstItemString);

            if (lastItem != null && lastItem.response.docs != null)
            {
                var item = lastItem.response.docs[0];
                filter.EndDate = item.PublicationDate;
            }

            if (firstItem != null && firstItem.response.docs != null)
            {
                var item = firstItem.response.docs[0];
                filter.StartDate = item.PublicationDate;
            }

            return filter;
        }

        public static List<ItemSitemap> WriteGenericSitemap(FilterSitemap dates, string pathLocation, string frontUrl)
        {
            var list = new List<ItemSitemap>
            {
                new ItemSitemap {Url = "/sitemaps/sitemap-sections.xml", Date = string.Empty}
            };

            do
            {
                list.Add(new ItemSitemap { Url = $"/sitemaps/sitemap_news_{dates.EndDate.Month}_{dates.EndDate.Year}.xml", Date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssssZ") });
                dates.EndDate = dates.EndDate.AddMonths(-1);
            } while (dates.StartDate.Month != dates.EndDate.Month || dates.StartDate.Year != dates.EndDate.Year);

            list.Add(new ItemSitemap { Url = $"/sitemaps/sitemap_news_{dates.StartDate.Month}_{dates.StartDate.Year}.xml", Date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssssZ") });

            WriteGenericFile(list, pathLocation, frontUrl);

            return list;
        }

        public static void ValidateDirectory(string pathLocation)
        {
            if (!Directory.Exists(pathLocation + "/sitemaps"))
            {
                Directory.CreateDirectory(pathLocation + "/sitemaps");
            }
        }
    }

    public class ItemSitemap
    {
        public string Url { get; set; }
        public string Date { get; set; }
    }

    public class FilterSitemap : ICloneable
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}