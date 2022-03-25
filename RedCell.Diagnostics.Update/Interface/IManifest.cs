using System;
using System.Xml.Linq;
using System.Linq;
using System.IO;

namespace RedCell.Diagnostics.Update.Interface
{
    public interface IManifest
    {
        int Version { get; }
        int CheckInterval { get; }
        string RemoteConfigUri { get; }
        string SecurityToken { get; }
        string BaseUri { get; }
        string[] Payloads { get; }
        void Write(string path);
    }
}