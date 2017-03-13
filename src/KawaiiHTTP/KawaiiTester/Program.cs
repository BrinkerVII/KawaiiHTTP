using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KawaiiTester
{
    class Program
    {
        private static bool printHostnames = true;
        static void Main(string[] args)
        {
            if (Program.printHostnames)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("Hostnames:");
                foreach (string hostName in KawaiiHTTP.MachineData.HostNames.NameDictionary)
                {
                    Console.WriteLine(hostName);
                }
                Console.ResetColor();
            }

            KawaiiHTTP.Log.Debugging = true; // Enable logging
            KawaiiHTTP.HTTPServer testServer = new KawaiiHTTP.HTTPServer();
            testServer.HTTPHandlers.Add(new QuirkyHandler());
            Console.WriteLine("Starting HTTP Server");
            testServer.StartListening();

            /*System.Net.WebClient c = new System.Net.WebClient();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(c.DownloadString("http://127.0.0.1/"));*/
        }
    }

    class QuirkyHandler : KawaiiHTTP.IHTTPHandler
    {
        public bool HandleRequest(KawaiiHTTP.Handlers.HandlePackage package)
        {
            if (package.RequestHeader.URL.StartsWith("/~console/"))
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(package.RequestHeader.URLDecoded.Substring(10));
                foreach (KeyValuePair<string, string> field in package.RequestHeader.GetFields)
                {
                    Console.WriteLine("{0} = {1}", field.Key, field.Value);
                }

                Console.ResetColor();
                return true;
            }

            return false;
        }
    }
}
