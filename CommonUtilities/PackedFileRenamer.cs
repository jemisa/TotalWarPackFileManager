using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace CommonUtilities {
    public class PackedFileRenamer {
        string prefix;
        public PackedFileRenamer(string pr) {
            prefix = pr;
        }
        public void Rename(IEnumerable<PackedFile> files) {
            foreach (PackedFile file in files.Where(f => !f.Name.StartsWith(prefix))) {
                file.Name = string.Format("{0}{1}", prefix, file.Name);
            }
        }
    }
}
