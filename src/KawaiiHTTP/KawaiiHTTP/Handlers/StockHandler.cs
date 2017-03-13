using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace KawaiiHTTP.Handlers
{
    public class StockHandler : IHTTPHandler
    {
        public virtual bool HandleRequest(HandlePackage package)
        {
            bool validRoot = false;
            if (Directory.Exists(package.Server.Settings.DocumentRoot))
            {
                validRoot = true;
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(package.Server.Settings.DocumentRoot);
                }
                catch (Exception ex)
                {
                    Log.e("Could not create DocumentRoot: {0}", ex.Message);
                }
            }
            if (!validRoot)
            {
                package.ContentStream.WriteLine("Server error: Invalid root path");
                package.ResponseHeader.StatusCode = 500;
                package.ResponseHeader.StatusMessage = "Internal Server Error";
                return true;
            }

            string path = package.Server.Settings.DocumentRoot;
            string url = package.RequestHeader.URL;
            string url_lower = url.ToLower();
            if (url == "/")
            {
                string index_html = path + "\\index.html";
                if (File.Exists(index_html))
                {
                    path = index_html;
                }
            }
            else
            {
                if (url.StartsWith("/"))
                {
                    path = path + StringUtil.URLToWindows(url);
                }
                else
                {
                    path = path + "\\" + StringUtil.URLToWindows(url);
                }
            }

            if (File.Exists(path))
            {
                this.PrintFile(path, package);
                return true;
            }
            else
            {
                if (Directory.Exists(path))
                {
                    string index_html = path + "\\index.html";
                    if (File.Exists(index_html))
                    {
                        package.ResponseHeader.StatusCode = 301;
                        package.ResponseHeader.StatusMessage = "Moved Permanently";
                        string furl = url;
                        if (!url.EndsWith("/")) { furl += "/"; }
                        furl += "index.html";
                        package.ResponseHeader.SetField("Location", furl);
                        return true;
                    }
                    else
                    {
                        this.PrintDirectory(path, package);
                        return true;
                    }
                }
                else
                {
                    this.Write404(package);
                    return true;
                }
            }
        }

        private void PrintDirectory(string path, HandlePackage package)
        {
            if (!package.Server.Settings.ListDirectories)
            {
                this.Write404(package);
                return;
            }

            if (!Directory.Exists(path)) { return; }
            Pages.BrowserPage page = new Pages.BrowserPage();
            page.Filename = path.Replace(package.Server.Settings.DocumentRoot, "").Replace("\\", "/");
            page.ParentDirectory = path == package.Server.Settings.DocumentRoot ? "/" : "../";

            foreach (string dir in Directory.GetDirectories(path))
            {
                string truncated = StringUtil.TruncatePath(dir);
                page.AddFile(new Pages.BrowserPage.BrowserFile(StringUtil.WindowsToURL(truncated) + "/", truncated, false));
            }

            foreach (string file in Directory.GetFiles(path))
            {
                string truncated = StringUtil.TruncatePath(file);
                page.AddFile(new Pages.BrowserPage.BrowserFile(StringUtil.WindowsToURL(truncated), truncated, true));
            }

            page.DumpToStream(package.ContentStream);
        }

        private void PrintFile(string path, HandlePackage package)
        {
            if (!File.Exists(path)) { return; }

            try
            {
                FileStream fs = File.Open(path, FileMode.Open);
                fs.CopyTo(package.ContentStream);
                try
                {
                    fs.Close();
                }
                catch (Exception ex)
                {
                    Log.e("Failed to close file {0}:\n {1}", path, ex.Message);
                }

                string ext = StringUtil.GetExtension(path).ToLower();
                package.ResponseHeader.SetField("Content-Type", MIME.GetContentType(ext));
            }
            catch (Exception ex)
            {
                Log.e("Failed to write file to HTTP {0}", ex.Message);
            }
        }

        private void Write404(HandlePackage package)
        {
            package.ResponseHeader.StatusCode = 404;
            package.ResponseHeader.StatusMessage = "Not Found";
            package.ContentStream.WriteLine("404 not found: the requested file does not exist");
        }
    }
}
