using System;
using System.Net;
using System.Threading;
using System.Text;
using RedCell.Net.Interface;

namespace RedCell.Net
{
    public class Fetch : IFetch
    {
        public WebHeaderCollection Headers { get; private set; }
        public HttpWebResponse Response { get; private set; }
        public NetworkCredential Credential { get; set; }
        public byte[] ResponseData { get; private set; }
        public int Retries { get; set; }
        public int Timeout { get; set; }
        public int RetrySleep { get; set; }
        public bool Success { get; private set; }
        public Fetch()
        {
            Headers = new WebHeaderCollection();
            Retries = 5;
            Timeout = 6000;
        }
        public void Load(string url)
        {
            for (int retry = 0; retry < Retries; retry++)
            {
                try
                {
                    var req = HttpWebRequest.Create(url) as HttpWebRequest;
                    req.AllowAutoRedirect = true;
                    ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;
                    if (Credential != null)
                        req.Credentials = Credential;
                    req.Headers = Headers;
                    req.Timeout = Timeout;
                    Response = req.GetResponse() as HttpWebResponse;
                    switch (Response.StatusCode)
                    {
                        case HttpStatusCode.Found:
                        {
                            Console.WriteLine("Found (302), ignoring...");
                            break;
                        }
                        case HttpStatusCode.OK:
                        {
                            using (var sr = Response.GetResponseStream())
                            using (var ms = new System.IO.MemoryStream())
                            {
                                for (int b; (b = sr.ReadByte()) != -1;)
                                {
                                    ms.WriteByte((byte)b);
                                }
                                ResponseData = ms.ToArray();
                            }
                            break;
                        }
                        default:
                        {
                            Console.WriteLine(Response.StatusCode);
                            break;
                        }
                    }
                    Success = true;
                    break;
                }
                catch (WebException we)
                {
                    Console.WriteLine(":Exception " + we.Message);
                    Response = we.Response as HttpWebResponse;
                    if (we.Status == WebExceptionStatus.Timeout)
                    {
                        Thread.Sleep(RetrySleep);
                        continue;
                    }
                    break;
                }
            }
        }
        public static byte[] Get(string url)
        {
            var f = new Fetch();
            f.Load(url);
            return f.ResponseData;
        }
        public string GetString()
        {
            var encoder = string.IsNullOrEmpty(Response.ContentEncoding) ?
                Encoding.UTF8 : Encoding.GetEncoding(Response.ContentEncoding);

            if (ResponseData == null)
                return string.Empty;
            return encoder.GetString(ResponseData);
        }
    }
}
