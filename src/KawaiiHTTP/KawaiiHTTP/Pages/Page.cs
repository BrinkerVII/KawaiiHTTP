using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KawaiiHTTP.Pages
{
    public class Page
    {
        public bool IsParsed { get; protected set; }
        public string HTMLContent { get; protected set; }

        public virtual void DumpToStream(ContentStream stream)
        {
            // byte[] pageBytes = Encoding.UTF8.GetBytes(this.HTMLContent);
            // stream.Write(pageBytes, 0, pageBytes.Length);
            this.Parse();
            stream.Write(this.HTMLContent);
        }

        public virtual void Parse()
        {
            if (this.IsParsed) { return; }
            this.IsParsed = true;
        }
    }
}
