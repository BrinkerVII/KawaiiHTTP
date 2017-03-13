using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KawaiiHTTP
{
    public interface IHTTPHandler
    {
        bool HandleRequest(Handlers.HandlePackage package);
    }
}
