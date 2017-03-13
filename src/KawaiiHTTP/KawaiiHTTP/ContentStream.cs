using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KawaiiHTTP
{
    public class ContentStream : System.IO.MemoryStream
    {
        public ContentStream() : base()
        {

        }
        public void WriteLine(string s) {
            this.Write(s + "\r\n");
        }
        public void Write(string s) {
            byte[] stringBytes = Encoding.UTF8.GetBytes(s);
            this.Write(stringBytes, 0, stringBytes.Length);
        }
    }
}
