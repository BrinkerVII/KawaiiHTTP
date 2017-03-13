using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KawaiiHTTP.Settings
{
    public class SettingsObject
    {
        public string Server {
            get { return "Kawaii"; }
        }
        public bool ChargenOnEmptyStream {
            get { return false; }
        }
        public int ChargenLength {
            get { return 1024 * 3; }
        }
        public bool ListDirectories {
            get { return true; }
        }
        public bool AutoProxy {
            get { return true; }
        }
        private string documentRoot = "$current$\\DocumentRoot";
        public string DocumentRoot {
            get { return this.documentRoot.Replace("$current$", System.IO.Directory.GetCurrentDirectory()); }
        }
    }
}
