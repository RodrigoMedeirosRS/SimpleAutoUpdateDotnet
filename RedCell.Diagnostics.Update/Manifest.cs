using System;
using System.Xml.Linq;
using System.Linq;
using System.IO;
using RedCell.Diagnostics.Update.Interface;

namespace RedCell.Diagnostics.Update
{
    internal class Manifest : IManifest
    {
        private string _data { get; set; }
        public int Version { get; private set; }
        public int CheckInterval { get; private set; }
        public string RemoteConfigUri { get; private set; }
        public string SecurityToken { get; private set; }
        public string BaseUri { get; private set; }
        public string[] Payloads { get; private set; }
        public Manifest(string data)
        {
            Load(data);
        }
        private void Load(string data)
        {
            _data = data;
            try
            {
                var xml = XDocument.Parse(data);
                if (xml.Root.Name.LocalName != "Manifest")
                {
                    Log.Write("Root XML element {0} is not recognized, stopping.", xml.Root.Name);
                    return;
                }
                Version = int.Parse(xml.Root.Attribute("version").Value);
                CheckInterval = int.Parse(xml.Root.Element("CheckInterval").Value);
                RemoteConfigUri = xml.Root.Element("RemoteConfigUri").Value;
                SecurityToken = xml.Root.Element("SecurityToken").Value;
                BaseUri = xml.Root.Element("BaseUri").Value;
                Payloads = xml.Root.Elements("Payload").Select(x => x.Value).ToArray();
            }
            catch (Exception ex)
            {
                Console.Write("Error: {0}", ex.Message);
                return;
            }
        }
        public void Write(string path)
        {
            File.WriteAllText(path, _data);
        }
    }
}