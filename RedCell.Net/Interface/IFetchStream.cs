using System;
using System.IO;
using System.Net;
using System.Text;

namespace RedCell.Net.Interface
{
    public interface IFetchStream
    {
        HttpWebResponse Response { get; }
        byte[] ResponseData { get; }
        void Load(string url);
        string GetString();
    }
}