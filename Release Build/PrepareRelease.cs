using Common;
using Filetypes;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PackFileManager;

namespace ReleaseBuild {
    class PrepareRelease {
        public static void Main(string[] args) {
            DBTypeMap.Instance.initializeFromFile("master_schema.xml");
            List<Thread> threads = new List<Thread>();
            foreach (Game game in Game.Games) {
                GameManager.LoadGameLocationFromFile(game);
                if (game.IsInstalled) {
                    SchemaOptimizer optimizer = new SchemaOptimizer() {
                        PackDirectory = game.DataDirectory,
                        SchemaFilename = game.MaxVersionFilename
                    };
                    //optimizer.FilterExistingPacks();
                    ThreadStart start = new ThreadStart(optimizer.FilterExistingPacks);
                    Thread worker = new Thread(start);
                    threads.Add(worker);
                    worker.Start();
                    // Console.WriteLine("{0} entries removed for {1}", optimizer.RemovedEntries, game.Id);
                }
            }
            threads.ForEach(t => t.Join());
        }
    }
}
