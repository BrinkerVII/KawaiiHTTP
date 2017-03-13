using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KawaiiHTTP.Handlers
{
    public class CoreHandler : StockHandler
    {
        private string coreString = "/~kawaiicore/";
        public override bool HandleRequest(HandlePackage package)
        {
            string url_lower = package.RequestHeader.URL.ToLower();
            if (!url_lower.StartsWith(this.coreString)) { return false; }

            string[] url_exploded = url_lower.Substring(1).Split('/');
            if (url_exploded.Length < 3) { return false; }

            switch (url_exploded[1])
            {
                case "resource":
                    return this.HandleResource(url_exploded[2], package);
                default:
                    return false;
            }
        }
        private bool HandleResource(string resourceName, HandlePackage package)
        {
            switch (resourceName)
            {
                case "corecss.css":
                    package.ResponseHeader.SetField("Content-Type", MIME.GetContentType("css", "text/css"));
                    package.ContentStream.Write(Properties.Resources.CoreCSS);
                    return true;
                case "ubuntu-r.ttf":
                    package.ResponseHeader.SetField("Content-Type", MIME.GetContentType("ttf", "x/application-font"));
                    package.ContentStream.Write(Properties.Resources.Ubuntu_R, 0, Properties.Resources.Ubuntu_R.Length);
                    return true;
                case "folder.png":
                    package.ResponseHeader.SetField("Content-Type", MIME.GetContentType("png", "image/png"));
                    Properties.Resources.appbar_folder.Save(package.ContentStream, System.Drawing.Imaging.ImageFormat.Png);
                    return true;
                case "page.png":
                    package.ResponseHeader.SetField("Content-Type", MIME.GetContentType("png", "image/png"));
                    Properties.Resources.appbar_page_bold.Save(package.ContentStream, System.Drawing.Imaging.ImageFormat.Png);
                    return true;
                default:
                    return false;
            }
        }
        public bool IsCoreRequest(HTTPHeader header)
        {
            return header.URL.ToLower().StartsWith(this.coreString);
        }
    }
}
