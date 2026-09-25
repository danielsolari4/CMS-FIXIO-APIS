using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Rino.Utils.Helpers
{
    public static class FormUpload
    {
        private static readonly Encoding encoding = Encoding.UTF8;

        public static HttpWebResponse MultipartFormDataPost(string postUrl, Dictionary<string, object> postParameters, string token)
        {
            string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
            string contentType = "multipart/form-data; boundary=" + formDataBoundary;

            byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);

            return PostForm(postUrl, contentType, formData, token);
        }

        public static byte[] DownloadData(string imageUrl)
        {
            byte[] data = null;

            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                using (var wc = new WebClient())
                {
                    byte[] bytes;

                    if (FormUpload.RemoteFileExists(imageUrl))
                    {
                        bytes = wc.DownloadData(imageUrl);

                        var ms = new MemoryStream(bytes);
                        data = new byte[ms.Length];
                        ms.Read(data, 0, data.Length);
                        ms.Close();
                    }
                }
            }

            return data;
        }

        private static HttpWebResponse PostForm(string postUrl, string contentType, byte[] formData, string token)
        {
            HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;

            if (request == null)
            {
                throw new NullReferenceException("request is not a http request");
            }

            // Set up the request properties.
            request.Method = "POST";
            request.ContentType = contentType;
            request.CookieContainer = new CookieContainer();
            request.ContentLength = formData.Length;
            request.Headers.Add("Authorization", token);

            // Send the form data to the request.
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Close();
            }

            return request.GetResponse() as HttpWebResponse;
        }

        private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
        {
            Stream formDataStream = new MemoryStream();
            bool needsCLRF = false;

            foreach (var param in postParameters)
            {
                // Thanks to feedback from commenters, add a CRLF to allow multiple parameters to be added.
                // Skip it on the first parameter, add it to subsequent parameters.
                if (needsCLRF)
                    formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

                needsCLRF = true;

                if (param.Value is FileParameter)
                {
                    FileParameter fileToUpload = (FileParameter)param.Value;

                    // Add just the first part of this param, since we will write the file data directly to the Stream
                    string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3}\r\n\r\n",
                        boundary,
                        param.Key,
                        fileToUpload.FileName ?? param.Key,
                        fileToUpload.ContentType ?? "application/octet-stream");

                    formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                    // Write the file data directly to the Stream, rather than serializing it to a string.
                    formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
                }
                else
                {
                    string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                        boundary,
                        param.Key,
                        param.Value);
                    formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
                }
            }

            // Add the end of the request.  Start with a newline
            string footer = "\r\n--" + boundary + "--\r\n";
            formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

            // Dump the Stream into a byte[]
            formDataStream.Position = 0;
            byte[] formData = new byte[formDataStream.Length];
            formDataStream.Read(formData, 0, formData.Length);
            formDataStream.Close();

            return formData;
        }

        public class FileParameter
        {
            public byte[] File { get; set; }
            public string FileName { get; set; }
            public string ContentType { get; set; }
            public FileParameter(byte[] file) : this(file, null) { }
            public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
            public FileParameter(byte[] file, string filename, string contenttype)
            {
                File = file;
                FileName = filename;
                ContentType = contenttype;
            }
        }

        private const string YoutubeLinkRegex = "(?:.+?)?(?:\\/v\\/|watch\\/|\\?v=|\\&v=|youtu\\.be\\/|\\/v=|^youtu\\.be\\/)([a-zA-Z0-9_-]{11})+";
        private static Regex ytRegexExtractId = new Regex(YoutubeLinkRegex, RegexOptions.Compiled);
        public static string ExtractYoutubeVideoIdFromUri(Uri uri)
        {
            try
            {
                //extract the id
                var regRes = ytRegexExtractId.Match(uri.ToString());
                if (regRes.Success)
                {
                    return regRes.Groups[1].Value;
                }
            }
            catch { }

            return null;
        }

        private const string DailyMotionLinkRegex = @"/^(?:https?:\/\/)?(?:www\.)?(?:youtu\.be\/|youtube\.com\/(?:embed\/|v\/|watch\?v=|watch\?.+&v=))((\w|-){11})(?:\S+)?$|^(?:https?:\/\/)?(?:www\.)?dailymotion.com\/(video|hub)+(\/([^_]+))?[^#]*(‪#‎video‬=([^_&]+))?$|^(?:https?:\/\/)?(?:www\.)?vimeo.com\/([0-9]+)$/";
        private static readonly Regex dmRegexExtractId = new Regex(DailyMotionLinkRegex, RegexOptions.Compiled);

        public static string ExtractDailyMotionVideoIdFromUri(Uri uri)
        {
            try
            {
                //extract the id
                var regRes = dmRegexExtractId.Match(uri.ToString());
                if (regRes.Success)
                {
                    return regRes.Groups[5].Value;
                }
                else
                {
                    return uri.AbsolutePath.Replace("/", string.Empty);
                }
            }
            catch { }

            return null;
        }

        public static string ExtractKalturaEntryIdFromUri(Uri uri)
        {
            try
            {
                if (uri.Segments[9] != null)
                    return uri.Segments[9].Replace("/", string.Empty);
            }
            catch { }

            return null;
        }

        public static string ExtractKalturaPartnerIdFromUri(Uri uri)
        {
            try
            {
                if (uri.Segments[5] != null)
                    return uri.Segments[5].Replace("/", string.Empty);
            }
            catch { }

            return null;
        }

        public static bool RemoteFileExists(string url)
        {
            try
            {
                bool exists = false;
                var request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "HEAD";
                var response = request.GetResponse() as HttpWebResponse;
                exists = response.StatusCode == HttpStatusCode.OK;
                response.Close();
                return exists;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }
    }
}
