using System.IO;
using System.Net;
using System.Threading.Tasks;
using Ray.Utils.Logging;

namespace Ray.Utils.Helpers
{
    public class HttpHelper
    {
        public static async Task<string> GetPageAsync(string url, string contentType)
        {
            System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = contentType;
                request.Method = WebRequestMethods.Http.Get;
                request.Timeout = 20000;
                request.Proxy = null;
                WebResponse response = await request.GetResponseAsync();
                return ReadStreamFromResponse(response);
            }
            catch (System.Exception ex)
            {
                CMSLogger.Info("Weather exception" + ex.Message);
                return null;
            }
        }


        private static string ReadStreamFromResponse(WebResponse response)
        {
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader sr = new StreamReader(responseStream))
            {
                //Need to return this response 
                string strContent = sr.ReadToEnd();
                return strContent;
            }
        }
    }
}
