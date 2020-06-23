using System;
using System.IO;
using System.Collections.Generic;
using Common;

namespace PackFileTest {
    /*
     * Base class for tests run against packed files.
     */
    public abstract class PackedFileTest : IComparable {
        public delegate PackedFileTest TestFactory();

        public SortedSet<string> generalErrors = new SortedSet<string> ();
        public SortedSet<string> allTestedFiles = new SortedSet<string>();
        
        public abstract bool CanTest(PackedFile file);

        public bool Verbose { get; set; }
        
        public virtual int TestCount {
            get {
                return allTestedFiles.Count;
            }
        }
        
        public virtual List<string> FailedTests {
            get {
                List<string> list = new List<string>();
                if (generalErrors.Count > 0) {
                    list.Add("General errors:");
                    list.AddRange(generalErrors);
                }
                return list;
            }
        }
        
        public abstract void TestFile(PackedFile file);
        
        public abstract void PrintResults();
        
        public int CompareTo(object o) {
            int result = GetType().GetHashCode() - o.GetType().GetHashCode();
            return result;
        }
        
        public static void PrintList(string label, ICollection<string> list) {
            if (list.Count != 0) {
                Console.WriteLine ("{0}: {1}", label, list.Count);
                foreach (string toPrint in list) {
                    Console.WriteLine (toPrint);
                }
            }
        }
        
        // run db tests for all files in the given directory
        public static void TestAllPacks(ICollection<TestFactory> testFactories, string dir, bool verbose) {
            List<string> fails = new List<string>();
            foreach (string file in Directory.EnumerateFiles(dir, "*.pack")) {
                string dirName = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(file)));
                string fileName = Path.GetFileName(file);
                string message = string.Format("starting test for {0} - {1}", dirName, fileName);
                Console.WriteLine(message);

                List<PackedFileTest> tests = new List<PackedFileTest>();
                foreach (TestFactory createTest in testFactories) {
                    tests.Add(createTest());
                }
                TestAllFiles(tests, file, verbose);
                List<string> failedTests = new List<string>();
                foreach (PackedFileTest test in tests) {
                    if (test.FailedTests.Count > 0) {
                        failedTests.Add(test.ToString());
                        failedTests.AddRange(test.FailedTests);
                        failedTests.Add("");
                        // output results
                        // test.PrintResults();
                    }
                }
                if (failedTests.Count > 0) {
                    //Console.WriteLine(string.Format("{0} - {1}",  dirName, fileName));
                    Console.WriteLine("Dir: {0}\nTests Run:{1}", dir, tests.Count);

                    if (verbose) {
                        foreach (string test in failedTests) {
                            Console.WriteLine(test);
                        }
                    }
                } else {
                    Console.WriteLine("All tests successful");
                }
                Console.Out.Flush();

                if (failedTests.Count > 0) {
                    fails.Add(string.Format("{0} - {1}", dirName, fileName));
                    fails.AddRange(failedTests);
                }
            }
            Console.WriteLine("******************** All test runs finished");
            fails.ForEach(f => Console.WriteLine(f));
        }
        
        // tests all files in this test's pack
        public static void TestAllFiles(ICollection<PackedFileTest> tests, string packFilePath, bool verbose) {
            PackFile packFile = new PackFileCodec().Open(packFilePath);
            foreach (PackedFile packed in packFile.Files) {
                if (verbose) {
                    Console.WriteLine("Testing {0}", packed.FullPath);
                }
                foreach (PackedFileTest test in tests) {
                    try {
                        if (test.CanTest(packed)) {
                            test.TestFile(packed);
                        }
                    } catch (Exception x) {
                        using (var outstream = File.Create(string.Format("failed_{0}.packed", packed.Name))) {
                            using (var datastream = new MemoryStream(packed.Data)) {
                                datastream.CopyTo(outstream);
                            }
                        }
                        test.generalErrors.Add(string.Format("reading {0}: {1}", packed.FullPath, x.Message));
                    }
                }
            }
        }
    }
}

