# SimpleAutoUpdate for NetCore 3.1
* Check a remote site via HTTP for a newer version.
* If a newer version is available, download it as a ZIP.
* Ensure successful download before overwriting anything.
* Make it easy to add to any application as a component.
* Allow updating of the application itself.
* Do not require a bootstrapper, or multi-step process.
* Resist tampering.
* Accommodate some simple logging.
* Single XML file configuration.
* Work on multiplatforms (Linux and Windows).
* Provide a simple, functional and reliable auto update system to NetCore 3 venrsios.

# How it Works
<img src="https://github.com/RodrigoMedeirosRS/SimpleAutoUpdate/blob/master/Readme%20Resources/Update.png" />
The Updater class does all the heavy lifting. It starts by loading an XML manifest that supplies all of the information it needs to do its work. By default it will look for a file called update.xml in the application's path. The manifests are represented by the Manifest class.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<!-- Increment the version for each update. -->
<Manifest version="3">
    <!-- Your application will check for updates every (seconds) -->
    <CheckInterval>900</CheckInterval>

    <!-- The URI to the remote manifest -->
    <RemoteConfigUri>https://remote.update.net/myapp/update.xml</RemoteConfigUri>

    <!-- This token must be the same at both ends to avoid tampering -->
    <SecurityToken>D68EF3A7-E787-4CC4-B020-878BA649B4CD</SecurityToken>
    <!-- All payload files are assumed to have this URI prefix. -->
    <BaseUri>https://remote.update.net/myapp/</BaseUri>

    <!-- One or more files containing updates. -->
    <Payload>myapp.zip</Payload>
</Manifest>
```

The format for the local and remote manifests is the same. At present, payloads must be ZIP files and their directory structure should be relative to the application's root. i.e. foo\bar.exe will be put in the application's foo directory.

The updater creates a System.Threading.Timer that ticks at the interval set by the manifest. When this occurs, a new thread is created that executes the Check method. Meanwhile, the application continues to run without interruption in the foreground.

Check fetches the remote manifest, checks the SecurityToken for tampering, and compares the version of the remote manifest to that of the local one. If the remote version is newer, the Update method is executed.

Update creates a work directory, downloads each of the payloads specified in the remote manifest, and unzips each of them. It also copies the remote manifest to the working directory, because it will become the new local manifest.

<b>Now for a cool trick</b>. How do we solve the chicken and egg problem? One of the payloads might contain a replacement for your application's executable itself, but it can't be overwritten while it is running. This is enforced by the operating system. <b>However</b>, Windows does (reason unknown) allow a running executable to be renamed! First we rename the application to [application].exe.bak and then we copy that file back to [application].exe. The file lock has been moved to the backup file, so if a payload contains a replacement, it will overwrite [application].exe. If it is not being replaced, no harm done.

To update the application we copy everything in the work directory to the application directory, and then delete the work directory.

Finally, the application is spawned as a new Process, and the current process is closed.

#Using the Code

Add a reference to RedCell.Diagnostics.Update.dll to your project.

Add to your application's startup code:

```c#
var updater = new RedCell.Diagnostics.Update.Updater();
updater.StartMonitoring();
```
Create an XML manifest and place it both in your application's directory and on the remote server.

You're done.

#But what's going on?

I have included a simple facilty for debugging, or if you wish to add a user interface to let the user know what is happening.
using RedCell.Diagnostics.Update;

```c#
// Log activity to the console.
Log.Console = true;

// Log activity to the System.Diagnostics.Debug facilty.
Log.Debug = true;

// Prefix messages to the above.
Log.Prefix = "[Update] "; // This is the default.

// Send activity messages to the UI.
Log.Event += (sender, e) => GuiMessageBox.Show(e.Message);
```

#Know issues at this time.
* Although the executable can be updated, any other open files can't.
* Multi-Threading applications can't be updated.
* Files aren't checked for integrity i.e. by comparing hashes.
* Files aren't overwritten until they are successfully unpacked, but there still isn't a rollback or backup mechanism.
* This isn't intended for applications that reside in Program Files. These directories are write-protected and writing requires UAC. More on that topic later.

#Notes

This project is derivated from Yvan Rodrigues original RedCell updater.
