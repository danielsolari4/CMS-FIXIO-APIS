using System;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Ray.Dtos.Factories.Html;
using Ray.Dtos.Interfaces;

namespace Ray.Dtos.Components
{
    public class ComponentNewDto : ComponentBaseDto, IComponentNew
    {
        [JsonProperty(PropertyName = "NewsId")]
        public int[] NewsId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public MediaSizesPaths MediaSizesPaths { get; set; }
        public string[] AuthorName { get; set; }
        public int[] AuthorId { get; set; }
        public DateTime LastModificationDate { get; set; }
        public string PublicationDate { get; set; }
        public string NodeContentTitle { get; set; }
        public string NodeContentDescription { get; set; }
        public string NodeContentTitleTheme { get; set; }
        public string MediaUrl { get; set; }
        public string MediaDiscriminator { get; set; }
        public override string GenerateHtml(HtmlBuilder builder, string imageUrl)
        {
            var html = builder.Html;

            if (AuthorName != null && AuthorName.Any())
            {
                html = html.ReplaceByPropertyName("component.properties.AuthorName", AuthorName[0]);
                if (AuthorId != null && AuthorId.Any())
                    html = html.ReplaceByPropertyName("component.properties.AuthorLink", "/autor/" + GenerateSlug(AuthorName[0]) + "_" + AuthorId[0]);
            }
            else
            {
                html = html.ReplaceByPropertyName("component.properties.AuthorName", string.Empty);
                html = html.ReplaceByPropertyName("component.properties.AuthorLink", string.Empty);
            }


            html = html.ReplaceByPropertyName("component.properties.NodeContentTitleTheme", NodeContentTitleTheme);
            html = html.ReplaceByPropertyName("component.properties.NodeContentTitle", NodeContentTitle);
            html = html.ReplaceByPropertyName("component.properties.NodeContentDescription", NodeContentDescription);
            html = html.ReplaceByPropertyName("Id", Id.ToString());
            html = html.ReplaceByPropertyName("Url", Url);
            html = html.ReplaceByPropertyName("component.properties.MediaSizesPaths.AssetMediaSizePath1", MediaSizesPaths?.Size1Path);
            html = html.ReplaceByPropertyName("component.properties.MediaSizesPaths.AssetMediaSizePath5", MediaSizesPaths?.Size5Path);
            html = html.ReplaceByPropertyName("component.properties.Title", Title);
            html = html.ReplaceByPropertyName("component.properties.Description", Description);
            html = html.ReplaceByPropertyName("component.properties.PublicationDate", PublicationDate);
            html = html.ReplaceByPropertyName("mediaurl", MediaUrl);
            html = html.ReplaceByPropertyName("component.css", Css);
            html = html.ReplaceByPropertyName("component.properties.MediaDiscriminator", MediaDiscriminator);

            var titleSection = builder.GetParseTitleHtml(this.Badge, "component.badge");

            return html.ReplaceByHtmlName("titleHtml", titleSection);
        }

        public void Parse(AssetSolrDto asset, string imageUrl)
        {
            if (asset == null) return;

            LastModificationDate = asset.LastModificationDate ?? DateTime.Now;
            AuthorName = asset.AuthorName;
            Title = asset.Title_en;
            Description = asset.Description_en;
            Url = asset.Url;
            MediaSizesPaths = asset.MediaSizesPaths;
            PublicationDate = asset.PublicationDate.ToString(CultureInfo.InvariantCulture);
            NodeContentTitle = asset.Nodes_en?.FirstOrDefault();
            NodeContentDescription = asset.Nodes_en?.FirstOrDefault() != null ? GenerateSlug(asset.Nodes_en?.FirstOrDefault()) : "seccion";
            NodeContentTitleTheme = $"theme-{asset.Nodes_en?.FirstOrDefault()?.ToLower().Replace(" ", " ")}";
            AuthorId = asset.AuthorId;

            if (asset.Nodes_slug != null)
            {
                if (asset.Nodes_slug.Count > 0)
                {
                    if (!NodeContentTitleTheme.Contains($"theme-{FirstFromSplit(asset.Nodes_slug[0], "/")?.ToLower().Replace(" ", "-")}"))
                        NodeContentTitleTheme += $" theme-{FirstFromSplit(asset.Nodes_slug[0], "/")?.ToLower().Replace(" ", "-")}";
                }
            }
            MediaDiscriminator = asset.MediaDiscriminator;
            MediaUrl = imageUrl;
        }


        public static string FirstFromSplit(string source, string delimiter)
        {
            var i = source.IndexOf(delimiter, StringComparison.Ordinal);
            return i == -1 ? source : source.Substring(0, i);
        }

        public static string GenerateSlug(string phrase, bool limit = true)
        {
            string str = RemoveAccent(phrase).ToLower();
            // invalid chars           
            str = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            if (limit)
                str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();

            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s", "-"); // hyphens   
            return str;
        }

        public static string RemoveAccent(string txt)
        {
            byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(txt);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }
    }
}
