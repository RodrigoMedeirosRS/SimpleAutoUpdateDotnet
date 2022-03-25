using System.Net;

namespace RedCell.Net.Interface
{
    public interface IFetch
    {
        WebHeaderCollection Headers { get; }
        HttpWebResponse Response { get; }
        NetworkCredential Credential { get; set; }
        byte[] ResponseData { get; }
        int Retries { get; set; }
        int Timeout { get; set; }
        int RetrySleep { get; set; }
        bool Success { get; }
        void Load(string url);
        string GetString();
    }
}