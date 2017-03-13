using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KawaiiHTTP.Pages
{
    public class BrowserPage : Page
    {
        public struct BrowserFile
        {
            public string Location { get; private set; }
            public string DisplayName { get; private set; }
            public bool IsFile { get; private set; }
            public BrowserFile(string location, string display, bool isfile)
            {
                this.Location = location;
                this.DisplayName = display;
                this.IsFile = isfile;
            }
        }
        private List<BrowserFile> files = new List<BrowserFile>();
        public BrowserPage() : base()
        {
            this.HTMLContent = KawaiiHTTP.Properties.Resources.BrowserPage;
        }
        public string Filename { get; set; }
        public string ParentDirectory { get; set; }
        public void AddFile(BrowserFile file)
        {
            this.files.Add(file);
        }
        private string BuildTable()
        {
            StringBuilder builder = new StringBuilder();
            foreach (BrowserFile file in this.files)
            {
                builder.Append(Properties.Resources.BrowserRowTemplate
                    .Replace("#isfile", file.IsFile.ToString().ToLower())
                    .Replace("#location", file.Location)
                    .Replace("#display", file.DisplayName)
                    );
                builder.Append('\n');
            }

            return builder.ToString();
        }
        public override void Parse()
        {
            if (this.IsParsed) { return; }
            this.HTMLContent = this.HTMLContent
                .Replace("#filename", this.Filename)
                .Replace("#tablespace", this.BuildTable())
                .Replace("#parentdirectory", this.ParentDirectory)
                ;
            base.Parse();
        }
    }
}
