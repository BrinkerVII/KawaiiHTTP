using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KawaiiHTTP
{
    public static class MIME
    {
        private static bool Initialized = false;
        private static Dictionary<string, string> _MIMELib = new Dictionary<string, string>();
        public static Dictionary<string, string> MIMELib
        {
            get
            {
                if (!Initialized)
                {
                    string[] mimelines = KawaiiHTTP.Properties.Resources.MIME.Replace("\r", "").ToLower().Split('\n');
                    foreach (string line in mimelines)
                    {
                        if (line.StartsWith("!")) { continue; }

                        string[] midSplit = line.Split(' ');
                        string[] leftSplit = midSplit[0].Split(';');
                        if (midSplit.Length < 2) { continue;  }
                        foreach (string extension in leftSplit)
                        {
                            if (_MIMELib.ContainsKey(extension))
                            {
                                _MIMELib.Remove(extension);
                            }

                            _MIMELib[extension] = midSplit[1];
                        }
                    }

                    Initialized= true;
                }
                return _MIMELib;
            }
            set { throw new Exception("Cannot set the MIMELib object!"); }
        }
        public static bool CanMIME(string extension)
        {
            return MIME.MIMELib.ContainsKey(extension.ToLower());
        }
        public static string GetContentType(string extension, string defaultType = "text/html")
        {
            string lextension = extension.ToLower();
            if (MIME.CanMIME(lextension))
            {
                return MIME.MIMELib[lextension];
            }
            else
            {
                return defaultType;
            }
        }
    }
}
