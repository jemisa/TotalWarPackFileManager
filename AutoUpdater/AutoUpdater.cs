using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using ICSharpCode.SharpZipLib.Zip;

namespace AutoUpdater {
    /*
     * Provides small executable that will download a zip file, wait for another process to finish, 
     * then extract the zip file and start a given executable.
     * 
     * It is meant to provide an automatic update of a program which can't do it for itself
     * because it can't write files it still uses while running.
     */
    public class Updater {
        public static void Main(string[] args) {
            // the id of the process to wait for termination of
            int processId;

            // the target version identifier
            // determines source URL and target zip filename
            string downloadUrl;
            
            // name of the executable to run after the other process has finished (full path)
            string startFileName;
            
            // collect eventual arguments
            List<string> arguments = new List<string> ();
            
            Console.WriteLine("Autoupdate started...");
   
            try {
                processId = int.Parse (args [0]);
                downloadUrl = args [1];
                startFileName = args [2];
                for (int i = 3; i < args.Length; i++) {
                    arguments.Add (args [i]);
                }
            } catch {
                Console.WriteLine ("usage: <pid> <downloadUrl> <executable> [<exec_parameters>]");
                return;
            }
#if DEBUG
            Console.WriteLine("startFileName: {0}", startFileName);
#endif
   
            Process proc = null;
            try {
                proc = Process.GetProcessById (processId);
                proc.EnableRaisingEvents = true;
            } catch (Exception x) {
                Console.WriteLine ("Failed to wait for process '{0}': {1}", processId, x.Message);
            }

            // download file from URL; this also gives the other process some time to shutdown
            // to avoid superfluous waiting
            string targetDir = Path.GetDirectoryName(startFileName);
            string filename = Path.GetFileName(downloadUrl).Replace("%20", " ");
            if (filename.Contains("?")) {
                filename = filename.Remove(filename.IndexOf('?'));
            }
            DownloadFile(downloadUrl, targetDir, filename);
            
            try {
                if (proc != null && !proc.HasExited) {
                    Console.WriteLine ("waiting for process to exit...");
                    proc.WaitForExit ();
                }
            } catch (Exception x) {
                Console.WriteLine ("Failed to wait for process {0}: {1}", processId, x.Message);
            }
            
            Console.WriteLine ("Installing...");

            // unzip all entries
            Unzip(filename);

            string asParameters = string.Join(" ", arguments);
            Console.WriteLine ("starting {0} {1}", startFileName, asParameters.Trim ());
            
            Console.WriteLine ("Okay, finished. Press key to restart.");
            Console.ReadKey();
            Process.Start (startFileName, asParameters.Trim ());
        }
  
        // downloads the given url to the given target directory, with the optional target filename.
        // if target filename is null, the url's filename is used 
        // (which might include encoded characters and http-query information).
        public static void DownloadFile(string url, string targetDir, string targetFile = null) {
            if (targetFile == null) {
                targetFile = Path.GetFileName(url);
            }

            //FIXME This is an artificial method of changing TLS version.  It requires a newer version of .NET be installed than the program uses and thus the program should be migrated to a newer version since we are trying to use newer features than the required .NET version has.
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (var dlStream = response.GetResponseStream()) {
                string targetPath = Path.Combine(targetDir, targetFile);
                using (var targetFileStream = File.Create(targetPath)) {
                    Console.WriteLine("Downloading from {0} to {1}", url, targetPath);
                    dlStream.CopyTo(targetFileStream);
                }
            }
        }
        
        static List<string> DontUnzip {
            get {
                // these two files are in use by the auto updater itself and can't be unzipped
                List<string> result = new List<string>();
                result.Add("AutoUpdater.exe");
                result.Add("ICSharpCode.SharpZipLib.dll");
                return result;
            }
        }
        // unzip all entries of the given file to the directory it resides in.
        public static void Unzip(string zipFile) {
            string targetDir = Path.GetDirectoryName(zipFile);
            using (var zipStream = new ZipInputStream(File.OpenRead(zipFile))) {
            ZipEntry entry = zipStream.GetNextEntry();
            int tryCount = 0;
                while (entry != null) {
                    try {
                        if (DontUnzip.Contains(entry.Name)) {
                            Console.WriteLine("Skipping {0}", entry.Name);
                            entry = zipStream.GetNextEntry();
                            continue;
                        }
                        string targetFile = Path.Combine(targetDir, entry.Name);
                        using (FileStream outStream = File.Create(targetFile)) {
                            zipStream.CopyTo(outStream);
                        }
                        // give specific notes to user
                        // (like "AutoUpdater has been updated but can't be installed automatically, unzip manually")
                        if (entry.Name.Equals ("README")) {
                            foreach(string line in File.ReadAllLines(targetFile)) {
                                Console.WriteLine(line);
                            }
                        }
                        entry = zipStream.GetNextEntry();
                    } catch {
                        if (tryCount++ < 5) {
                            Console.WriteLine("Could not unpack {0}; retrying", entry.Name);
                        } else {
                            Console.WriteLine("Giving up. It's probably best if you manually extract that file.", entry.Name);
                            tryCount = 0;
                            entry = zipStream.GetNextEntry();
                        }
                    }
                }
            }
        }
    }    
    
    // compare build numbers
    public class BuildVersionComparator : Comparer<string>
    {
        public static readonly Comparer<string> Instance = new BuildVersionComparator();
        
        public override int Compare(string v1, string v2) {
            int result = 0;
            string[] v1Split = v1.Split('.');
            string[] v2Split = v2.Split('.');
            for (int i = 0; i < Math.Min(v1Split.Length, v2Split.Length); i++) {
                int v1Version = 0, v2Version = 0;
                int.TryParse(v1Split[i], out v1Version);
                int.TryParse(v2Split[i], out v2Version);
                result = v1Version - v2Version;
                if (result != 0) {
                    return result;
                }
            }
            if (result == 0) {
                // different version lengths (eg 1.7.2 and 2.0)
                result = v1Split.Length != v2Split.Length ? 1 : 0;
                // longer one is larger (2.0.1 > 2.0)
                result *= v1Split.Length > v2Split.Length ? 1 : -1;
            }

            // result > 0: v1 is larger
            return result;
        }
    }
}
