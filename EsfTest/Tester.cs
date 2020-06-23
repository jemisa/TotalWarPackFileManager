using EsfLibrary;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace EsfTest {
    public class Tester {
        public static string FILENAME="testfiles.txt";
        public static TextWriter logWriter;        
        public static TextWriter addressLogWriter;
        public static void Main(string[] args) {
            runTests();
            //testFiles();
        }
        
        public static void runTests() {
            new CodecTest().run();
        }
        public static void testFiles() {
            foreach (string file in File.ReadAllLines(FILENAME, Encoding.Default)) {
                if (file.StartsWith("#") || string.IsNullOrEmpty(file)) {
                    continue;
                }
                //testOld (file);
                testNew (file);
                //Console.ReadKey();
            }
        }
        
        static void testNew (string filename) {
            using (FileStream 
                   logStream = File.Create(filename + "_log.txt"), 
                   addressStream = File.Create(filename + "_address.txt")) {
                logWriter = new StreamWriter(logStream);
                addressLogWriter = new StreamWriter(addressStream);
                EsfFile file = null;
                DateTime start = DateTime.Now;
                try {
                    Console.WriteLine("reading {0}", filename);
                    using (FileStream stream = File.OpenRead(filename)) {
                        EsfCodec codec = EsfCodecUtil.GetCodec(stream);
                        TicToc timer = new TicToc();
                        codec.NodeReadStarting += timer.Tic;
                        codec.NodeReadFinished += timer.Toc;
                        // codec.NodeReadFinished += OutputNodeEnd;
                        file = EsfCodecUtil.LoadEsfFile(filename);
                        forceDecode(file.RootNode);
                        //file = new EsfFile(stream, codec);
                        
                        timer.DumpAll();
                    }
                    Console.WriteLine("{0} read in {1} seconds", file, (DateTime.Now.Ticks - start.Ticks) / 10000000);
                    Console.WriteLine("Reading finished, saving now");
                } catch (Exception e) {
                    Console.WriteLine("Read failed: {0}, {1}", filename, e);
                }
                try {
                    string saveFile = filename + "_save";
                    if (file != null) {
                        EsfCodecUtil.WriteEsfFile(saveFile, file);
                    }
                    //File.Delete(saveFile);
                } catch (Exception e) {
                    Console.WriteLine("Write {0} failed: {1}", filename, e);
                }
                logWriter.Flush();
                addressLogWriter.Flush();
            }
        }
        static void forceDecode(EsfNode node) {
            if (node is ParentNode) {
                (node as ParentNode).AllNodes.ForEach(n => forceDecode(n));
            }
        }
        
        static void OutputNodeEnd(EsfNode node, long position) {
            if (logWriter != null) {
                logWriter.WriteLine("{0} / {1:x}", node, node.TypeCode);
                addressLogWriter.WriteLine("{2:x}: {0} / {1:x}", node, node.TypeCode, position);
                logWriter.Flush();
                addressLogWriter.Flush();
            }
        }

/*        static void testOld(string filename) {
            EsfFile file = new EsfFile (filename);
            foreach (IEsfNode rootNode in file.RootNodes) {
                rootNode.ParseDeep ();
            }
        }
  */      
//        static void parseNode(IEsfNode node) {
//            node.Parse ();
//            node.
//        }
    }
    
    public class TicToc {
        Dictionary<byte, long> codeToTime = new Dictionary<byte, long>();
        Dictionary<byte, int> codeToCount = new Dictionary<byte, int>();
        
        private long startTime;
        public void Tic(byte typeCode, long readerPosition) {
            startTime = DateTime.Now.Ticks;
        }
        public void Toc(EsfNode node, long readerPosition) {
            long endTime = DateTime.Now.Ticks;
            long total;
            int count;
            if (codeToTime.TryGetValue((byte)node.TypeCode, out total)) {
                count = codeToCount[(byte)node.TypeCode] + 1;
            } else {
                total = 0;
                count = 1;
            }
            total += endTime - startTime;
            codeToTime[(byte) node.TypeCode] = total;
            codeToCount[(byte) node.TypeCode] = count;
        }
        public void DumpAll() {
            Dictionary<long, byte> otherWay = new Dictionary<long, byte>();
            foreach(byte code in codeToTime.Keys) {
                otherWay.Add(codeToTime[code], code);
                Console.WriteLine("{0:x}: {1}", code, codeToTime[code]);
            }
            List<long> sorted = new List<long>(otherWay.Keys);
            sorted.Sort();
            sorted.ForEach(i => Console.WriteLine("{1:x} ({2}): {0}", i, otherWay[i], codeToCount[otherWay[i]]));
        }
    }
}
