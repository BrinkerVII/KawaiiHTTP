using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

namespace KawaiiHTTP.Handlers
{
    public class ProxyHandler : IHTTPHandler
    {
        public bool HandleRequest(HandlePackage package)
        {
            string host = package.RequestHeader.Host.ToLower();
            if (MachineData.HostNames.ContainsName(host))
            {
                // Prevent a giant messy loop of requests
                return false;
            }

            string completeURL = string.Format("http://{0}{1}{2}", host, package.RequestHeader.URL, package.RequestHeader.ConcatGetValues());
            WebClient webClient = new WebClient();
            try
            {
                // Download the data and dump it into the stream
                byte[] proxyData = webClient.DownloadData(completeURL);
                package.ContentStream.Write(proxyData, 0, proxyData.Length);
                package.ResponseHeader.SetField("Content-Type", MIME.GetContentType(StringUtil.GetExtension(completeURL)));
                webClient.Dispose();

                // Well, that's all folks!
                return true;
            }
            catch (Exception ex)
            {
                Log.e("Failed to do proxy magic:\n{0}", ex.Message);
                package.ContentStream.Write("Well something went wrong");
                return true;
            }
        }
    }
}
