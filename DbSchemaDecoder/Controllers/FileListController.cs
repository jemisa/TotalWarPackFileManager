using Common;
using DbSchemaDecoder.Models;
using Filetypes.Codecs;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DbSchemaDecoder.Controllers
{

    public class DataBaseFile
    {
        public string TableType { get; set; }
        public PackedFile DbFile { get; set; }
    }
    class FileListController
    {
        public List<DataBaseFile> FileListModels { get; set; } = new List<DataBaseFile>();
        public event EventHandler<DataBaseFile> OnFileSelectedEvent;

        public FileListController()
        {
            Load(@"C:\Program Files (x86)\Steam\steamapps\common\Total War WARHAMMER II");
           /* FileListModels.Add(new TestItem() { FileName = "Db_01", ValidState = true });
            FileListModels.Add(new TestItem() { FileName = "Db_02", ValidState = false });
            FileListModels.Add(new TestItem() { FileName = "Db_03", ValidState = true });*/
        }

        private ICommand _command;
        public ICommand Command
        {
            get
            {
                return _command ?? (_command = new RelayCommand<DataBaseFile>(CommandWithAParameter));
            }
        }

        private void CommandWithAParameter(DataBaseFile state)
        {
            //var str = state as string;
            if (OnFileSelectedEvent != null)
                OnFileSelectedEvent.Invoke(this, state);
        }

        public void Load(string gameDir)
        {
            PackLoadSequence allFiles = new PackLoadSequence
            {
                IncludePacksContaining = delegate (string s) { return true; }
            };

            List<string> packPaths = allFiles.GetPacksLoadedFrom(gameDir);
            packPaths.Reverse();
            PackFileCodec codec = new PackFileCodec();

            foreach (string path in packPaths)
            {
                PackFile pack = codec.Open(path);
                foreach (var f in pack.Files)
                {
                    var isDb = IsDb(f);
                    if (isDb.HasValue)
                    {
                        FileListModels.Add(new DataBaseFile()
                        {
                            TableType = isDb.Value.Item1,
                            DbFile = isDb.Value.Item2
                        });
                    }
                }
            }
        }

        (string, PackedFile)? IsDb(PackedFile file)
        {
            bool hasParent = file.Parent != null;
            if (hasParent)
            {
                bool hasParentParnet = file.Parent.Parent != null;
                if (hasParentParnet)
                {
                    if (file.Parent.Parent.Name == "db")
                    {
                        return (file.Parent.Name, file as PackedFile);
                    }
                }
            }
            return null;
        }
    }
}
