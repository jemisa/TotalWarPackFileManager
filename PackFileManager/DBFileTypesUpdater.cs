using System;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using AutoUpdater;
using Common;
using PackFileManager.Properties;

namespace PackFileManager
{
    class Util {
        static readonly string sourceForgeFormat =
            "http://sourceforge.net/projects/packfilemanager/files/{0}/download";
            // "http://downloads.sourceforge.net/project/packfilemanager/{0}?r=&ts={1}&use_mirror=master";

        public static string CreateSourceforgeUrl(string file) {
            return string.Format(sourceForgeFormat, file, DateTime.Now.Ticks);
        }
        public static string PfmDirectory {
            get {
                return Path.GetDirectoryName(Application.ExecutablePath);
            }
        }
    }
    
    /*
     * A class checking for available updates to software and schema file.
     */
	class DBFileTypesUpdater {
        static string VERSION_FILE = "xmlversion";
        
        ILatestVersionRetriever versions;
        
        public DBFileTypesUpdater(bool findBetaSchema) {
            // versions = new TwcVersionRetriever();
            versions = new SourceforgeVersionRetriever(findBetaSchema);
        }
        
        #region Query Update Neccessary
        
        /*
         * Query if a new software version is available.
         */
        public bool NeedsPfmUpdate {
            get {
                return BuildVersionComparator.Instance.Compare(versions.LatestPfmFile, CurrentPfmVersion) > 0;
            }
        }
        #endregion

        #region Current Versions
        static string CurrentPfmVersion {
            get {
                return Application.ProductVersion;
            }
        }
        public string LatestPfmVersion {
            get {
                return versions.LatestPfmFile;
            }
        }
        static string CurrentSchemaVersion {
            get {
                string currentSchemaVersion = File.Exists(VERSION_FILE) ? File.ReadAllText(VERSION_FILE).Trim() : "0";
                return currentSchemaVersion;
            }
        }
        #endregion

        #region Zip File Names
        string PackFileZipname {
            get {
                return versions.LatestPfmFile;
            }
        }
        string SchemaZipname {
            get {
                return versions.LatestSchemaFile;
            }
        }
        static Regex VERSION_RE = new Regex("schema_([0-9]*)(beta)?\\..*");
        string LatestSchemaVersion {
            get {
                string result = "";
                try {
                    result = VERSION_RE.Match(SchemaZipname).Groups[1].Value;
                } catch { }
                return result;
            }
        }
        #endregion

        /*
         * Download and unzip the schema file.
         */
        public void UpdateSchema() {

            //FIXME This is an artificial method of changing TLS version.  It requires a newer version of .NET be installed than the program uses and thus the program should be migrated to a newer version since we are trying to use newer features than the required .NET version has.
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            Updater.DownloadFile(versions.SchemaUrl, Util.PfmDirectory, SchemaZipname);
            Updater.Unzip(Path.Combine(Util.PfmDirectory, SchemaZipname));
        }
        
        /*
         * Download software, then start the AutoUpdater as an external process;
         * this is needed to be able to extract the downloaded zip containing the PFM exe
         * which is still running.
         */
        public void UpdatePfm(string openPack = null) {
            Process myProcess = Process.GetCurrentProcess();
            string currentPackPath = openPack == null ? "" : string.Format(" \"{0}\"", openPack);
            string arguments = string.Format("{0} \"{1}\" \"{2}\"{3}", myProcess.Id, versions.PfmUrl, Application.ExecutablePath, currentPackPath);
#if DEBUG
            Console.WriteLine("Updating with AutoUpdater.exe {0}", arguments);
#endif

            myProcess.CloseMainWindow();
            // re-open file if one is open already
            Process.Start("AutoUpdater.exe", arguments);
            myProcess.Close();
        }
	}
    
    /*
     * Retrieve latest versions from any source.
     */
    interface ILatestVersionRetriever {
        string LatestPfmFile { get; }
        string LatestSchemaFile { get; }
        
        string SchemaUrl { get; }
        string PfmUrl { get; }
    }
 
    /*
     * Retrieves versions from single text file on sourceforge and creates links
     * from known base path + version information.
     */
    class SourceforgeVersionRetriever : ILatestVersionRetriever {
        const string pfmTag = "packfilemanager";
        const string schemaTag = "xmlversion";
        
        public string LatestPfmFile { get; private set; }
        public string LatestSchemaFile { get; private set; }

        #region Download URLs
        public string SchemaUrl {
            get {
                string result = Util.CreateSourceforgeUrl(string.Format("Schemata/{0}", LatestSchemaFile));
                return result;
            }
        }
        public string PfmUrl {
            get {
                return Util.CreateSourceforgeUrl(string.Format("Release/{0}", LatestPfmFile)); // replace " " -> "%20"
            }
        }
        #endregion
        
        static Regex schema_file_re = new Regex("schema_([0-9]+).zip");
        static Regex beta_schema_file_re = new Regex("schema_([0-9]+)(beta)?.zip");
        static Regex pfm_file_re = new Regex("Pack File Manager (.*).zip");
        //TODO Constructors shouldn't be doing logic.
        public SourceforgeVersionRetriever(bool findBeta) {
            Console.WriteLine("looking up sf");

            //FIXME This is an artificial method of changing TLS version.  It requires a newer version of .NET be installed than the program uses and thus the program should be migrated to a newer version since we are trying to use newer features than the required .NET version has.
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            FindOnPage findSchema = new FindOnPage {
                Url = "https://sourceforge.net/projects/packfilemanager/files/Schemata/",
                ToFind = findBeta ? beta_schema_file_re : schema_file_re
            };
            FindOnPage findPfmVersion = new FindOnPage {
                Url = "https://sourceforge.net/projects/packfilemanager/files/Release/",
                ToFind = pfm_file_re
            };
            Thread[] findThreads = new Thread[] {
                new Thread(findSchema.Search),
                new Thread(findPfmVersion.Search)
            };
            foreach (Thread t in findThreads) {
                t.Start();
            }
            foreach (Thread t in findThreads) {
                t.Join();
            }
            LatestSchemaFile = findSchema.Result;
            LatestPfmFile = findPfmVersion.Result;
            if (LatestPfmFile == null || LatestSchemaFile == null) {
                throw new InvalidDataException(string.Format("Could not determine latest versions: got {0}, {1}", 
                                                             LatestPfmFile, LatestSchemaFile));
            }
#if DEBUG
            Console.WriteLine("Current versions: pfm {0}, schema {1}", LatestPfmFile, LatestSchemaFile);
#endif
        }
    }
  
    /*
     * Retrieves versions from TWC PFM thread; PFM dl link is created from
     * known SF base path + version information, schema link retrieved from
     * forum thread href.
     */
    class TwcVersionRetriever : ILatestVersionRetriever {
        public string LatestPfmFile { get; private set; }
        public string LatestSchemaFile { get; private set; }
  
        #region Download URLs
        // retrieved from twc thread link while parsing
        string schemaUrl;
        public string SchemaUrl {
            get {
                return string.Format(schemaUrl, LatestSchemaFile);
            }
            private set {
                // expects the "attachmentid=..." string for the query parameters, will create URL itself
                schemaUrl = string.Format("http://www.twcenter.net/forums/attachment.php?{0}", value);
            }
        }
        public string PfmUrl {
            get {
                return string.Format(schemaUrl, LatestPfmFile);
            }
        }
        #endregion
  
        static Regex FileTypeRegex = new Regex("<a href=\".*(attachmentid=[^\"]*)\"[^>]*>schema_([0-9]*).zip</a>.*</td>");
        static Regex SwVersionRegex = new Regex(@"Update.*Pack File Manager ([0-9]*\.[0-9]*(\.[0-9]*)?)");
        static Regex StopReadRegex = new Regex("/ attachments");

        public TwcVersionRetriever() {
            LatestPfmFile = LatestSchemaFile = "0";
            
            string url = string.Format("http://www.twcenter.net/forums/showthread.php?t=494248");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (var stream = new StreamReader(response.GetResponseStream())) {
                string line = stream.ReadLine();
                while (line != null) {
                    if (FileTypeRegex.IsMatch(line)) {
                        Match match = FileTypeRegex.Match(line);
                        string schemaVersion = match.Groups[2].Value;
                        if (BuildVersionComparator.Instance.Compare(schemaVersion, LatestSchemaFile) > 0) {
                            LatestSchemaFile = schemaVersion;
                            schemaUrl = match.Groups[1].Value;
                        }
                    } else if (SwVersionRegex.IsMatch(line)) {
                        string swVersion = SwVersionRegex.Match(line).Groups[1].Value;
                        if (BuildVersionComparator.Instance.Compare(swVersion, LatestPfmFile) > 0) {
                            LatestPfmFile = swVersion;
                        }
                    } else if (StopReadRegex.IsMatch(line)) {
                        // PFM info is in the post itself, schema info in the attachment links...
                        // no need to go on
                        break;
                    }
                    line = stream.ReadLine();
                }
            }
            if (LatestPfmFile == null || LatestSchemaFile == null) {
                throw new InvalidDataException(string.Format("Could not determine latest versions: got {0}, {1}", 
                                                             LatestPfmFile, LatestSchemaFile));
            }
#if DEBUG
            Console.WriteLine("Latest PFM: {0}, Latest Schema: {1}", LatestPfmFile, LatestSchemaFile);
#endif
        }
    }
 
    /*
     * A class looking for a regular expression on a web page.
     * The Regex needs to contain a group, the match against which will be available after parsing.
     */
    class FindOnPage {
        public Regex ToFind { get; set; }
        public string Url { get; set; }
        public string Result { get; private set; }

        public void Search() {
            // can't find stuff like this
            if (Url == null || ToFind == null)
                return;

            try
            {
                WebRequest request = WebRequest.Create(Url);
                WebResponse response = request.GetResponse();
                using(var stream = new StreamReader(response.GetResponseStream()))
                {
                    string line = stream.ReadLine();

                    while(line != null)
                    {
                        if(ToFind.IsMatch(line))
                        {
                            // found the expression: store the group,
                            // then we can stop reading data
                            Match match = ToFind.Match(line);
                            Result = match.Groups[0].Value;
                            break;
                        }
                        line = stream.ReadLine();
                    }
                }
                response.Close();
            }
            catch(WebException e)
            {
                Console.Error.WriteLine("Failed to load web page to check for regular expression with the following error: " + e.ToString());
            }
            catch { }
        }
    }
}
