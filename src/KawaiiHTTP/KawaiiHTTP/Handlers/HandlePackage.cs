using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KawaiiHTTP.Handlers
{
    public struct HandlePackage
    {
        public HTTPHeader RequestHeader { get; private set; }
        public HTTPHeader ResponseHeader { get; private set; }
        public ContentStream RequestContent { get; private set; }
        public ContentStream ContentStream { get; private set; }
        public HTTPServer Server { get; private set; }
        public HandlePackage(
            HTTPHeader requestHeader,
            HTTPHeader responseHeader,
            ContentStream requestContent,
            ContentStream contentStream,
            HTTPServer server)
        {
            this.RequestHeader = requestHeader;
            this.ResponseHeader = responseHeader;
            this.RequestContent = requestContent;
            this.ContentStream = contentStream;
            this.Server = server;
        }
    }
}
