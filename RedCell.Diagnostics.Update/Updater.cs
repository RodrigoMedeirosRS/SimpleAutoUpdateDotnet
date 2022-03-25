using Ionic.Zip;
using RedCell.Net;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using RedCell.Diagnostics.Update.Interface;

namespace RedCell.Diagnostics.Update
{
    public class Updater : IUpdater
    {
        public int DefaultCheckInterval { get; private set; }
        public int FirstCheckDelay { get; private set; }
        public static string DefaultConfigFile { get; private set; }
        public string WorkPath { get; private set; }
        private Timer _timer { get; set; }
        private bool _updating { get; set; }
        private IManifest _localConfig { get; set; }
        private IManifest _remoteConfig { get; set; }
        private FileInfo _localConfigFile { get; set; }
        public Updater() : this(new FileInfo(DefaultConfigFile))
        {
            DefaultConfigFile = "update.xml";
        }
        public Updater(FileInfo configFile, int intervalInSeconds = 900, int firstCheckDelayInSeconds = 15, string defaultConfigFile = "update.xml", string workPath = "Document")
        {
            DefaultCheckInterval = intervalInSeconds;
            FirstCheckDelay = firstCheckDelayInSeconds;
            DefaultConfigFile = defaultConfigFile;
            WorkPath = workPath;

            Log.Debug = true;

            _localConfigFile = configFile;
            Log.Write("Loading...");
            Log.Write("Loaded.");
            Log.Write("Initializing using file '{0}'.", configFile.FullName);

            if (!configFile.Exists)
            {
                Log.Write("Config file {0} does not exist, stopping.", configFile.Name);
                return;
            }

            string data = File.ReadAllText(configFile.FullName);
            Console.WriteLine("Information of data:\n" + data);
            this._localConfig = new Manifest(data);
        }
        public void StartMonitoring()
        {
            Log.Write("Starting monitoring in {0}s...", this._localConfig.CheckInterval);
            Log.Write("Please wait...");
            _timer = new Timer(Check, null, 5000, this._localConfig.CheckInterval * 1000);
            Log.Write("Already.");
        }
        public void StopMonitoring()
        {
            Log.Write("Stopping monitoring.");

            if (_timer == null)
            {
                Log.Write("Monitoring is already stopped.");
                return;
            }
            _timer.Dispose();
        }
        private void Check(Object state)
        {
            Log.Write("Checking...");

            if (_updating)
            {
                Log.Write("Updater is already updating.");
                Log.Write("Check ending.");
            }

            var remoteUri = new Uri(this._localConfig.RemoteConfigUri);
            Log.Write("Fetching local file: '{0}'.", remoteUri.AbsoluteUri);
            var http = new Fetch { Retries = 5, RetrySleep = 30000, Timeout = 30000 };
            http.Load(remoteUri.AbsoluteUri);

            if (!http.Success)
            {
                Log.Write("Fetch error: {0}", http.Response.StatusDescription);
                this._remoteConfig = null;
                return;
            }
            string data = Encoding.UTF8.GetString(http.ResponseData);
            this._remoteConfig = new Manifest(data);

            if (this._remoteConfig == null)
            {
                Log.Write("Data is not found!, stopping...");
                Log.Write("Check ending.");
                return;
            }

            if (this._localConfig.SecurityToken != this._remoteConfig.SecurityToken)
            {
                Log.Write("Security token mismatch, stopping...");
                Log.Write("Check ending.");
                return;
            }

            Log.Write("Remote config is valid.");
            Log.Write("Local version is {0}", _localConfig.Version);
            Log.Write("Remote version is {0}", _remoteConfig.Version);

            if (this._remoteConfig.Version < this._localConfig.Version)
            {
                Log.Write("Remote version is older. That's weird.");
                Log.Write("Check ending.");
                return;
            }
            if (this._remoteConfig.Version == this._localConfig.Version)
            {
                Log.Write("Versions are the same.");
                Log.Write("Check ending.");
                return;
            }
            Log.Write("Remote version is newer. Updating.");
            _updating = true;
            Update();

            _updating = false;
            Log.Write("Check ending.");
            Console.ReadKey();
        }
        private void Update()
        {
            Log.Write("Updating '{0}' files.", this._remoteConfig.Payloads.Length);
            foreach (string str in this._remoteConfig.Payloads)
            {
                Console.WriteLine("Remote Payloads: " + str);
            }

            if (Directory.Exists(WorkPath))
            {
                Log.Write("WARNING: Work directory already exists.");

                try
                {
                    Directory.Delete(WorkPath, true);
                }
                catch (IOException) // nếu xóa không thành công, thông báo và kết thúc.
                {
                    Log.Write("Cannot delete open directory '{0}'.", WorkPath);
                    return;
                }
            }
            Directory.CreateDirectory(WorkPath);

            foreach (string update in this._remoteConfig.Payloads)
            {
                Log.Write("Fetching '{0}'.", update);
                var url = this._remoteConfig.BaseUri + update;
                var file = Fetch.Get(url);

                if (file == null)
                {
                    Log.Write("Fetch failed.");
                    return;
                }

                var info = new FileInfo(Path.Combine(WorkPath, update));
                Console.WriteLine("File is saved in: {0}", info.FullName);

                Directory.CreateDirectory(info.DirectoryName);
                File.WriteAllBytes(Path.Combine(WorkPath, update), file);

                if (Regex.IsMatch(update, @"\.zip"))
                {
                    try
                    {
                        var zipfile = Path.Combine(WorkPath, update);
                        using (var zip = ZipFile.Read(zipfile))
                            zip.ExtractAll(WorkPath, ExtractExistingFileAction.Throw);
                        File.Delete(zipfile);
                    }
                    catch (Exception ex)
                    {
                        Log.Write("Unpack failed: {0}", ex.Message);
                        return;
                    }
                }
            }

            Process thisprocess = Process.GetCurrentProcess();
            string me = thisprocess.MainModule.FileName;

            Console.WriteLine("Path: " + me);
            string bak = me + ".bak";
            Log.Write("Renaming running process to '{0}'.", bak);

            if (File.Exists(bak))
                File.Delete(bak);
            File.Move(me, bak);
            File.Copy(bak, me);

            _remoteConfig.Write(Path.Combine(WorkPath, _localConfigFile.Name));

            var directory = new DirectoryInfo(WorkPath);
            var files = directory.GetFiles("*.*", SearchOption.AllDirectories);
            foreach (FileInfo file in files)
            {
                string destination = file.FullName.Replace(directory.FullName + @"\", "");
                Log.Write("installing file '{0}'.", destination);
                Directory.CreateDirectory(new FileInfo(destination).DirectoryName);
                file.CopyTo(destination, true);
            }
            Log.Write("Deleting work directory.");
            
            Directory.Delete(WorkPath, true);
            Log.Write("Spawning new process.");
            var spawn = Process.Start(me);

            Log.Write("New process ID is {0}", spawn.Id);
            Log.Write("Closing old running process {0}.", thisprocess.Id);
            Console.WriteLine("Update Success! Press a key to close program...");
            Console.ReadKey();

            thisprocess.CloseMainWindow();
            thisprocess.Close();
            thisprocess.Dispose();
        }
    }
}
