using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.NetworkInformation;
using System.Net;

namespace KawaiiHTTP.MachineData
{
    public static class HostNames
    {
        private static List<string> names = new List<string>() {
            "127.0.0.1",
            "localhost",
            Dns.GetHostName().ToLower()
        };
        private static bool Initialized = false;
        public static List<string> NameDictionary
        {
            get
            {
                if (!HostNames.Initialized)
                {
                    foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        foreach (UnicastIPAddressInformation ipAddress in networkInterface.GetIPProperties().UnicastAddresses)
                        {
                            HostNames.Append(ipAddress.Address.ToString());
                        }
                    }

                    HostNames.Initialized = true;
                }

                return HostNames.names;
            }
        }
        public static bool ContainsName(string toCheck)
        {
            return HostNames.NameDictionary.Contains(toCheck.ToLower());
        }
        public static void Append(string name)
        {
            string lName = name.ToLower();
            if (!HostNames.names.Contains(lName))
            {
                HostNames.names.Add(lName);
            }
        }
        public static void Remove(string name)
        {
            string lName = name.ToLower();
            if (lName != Dns.GetHostName().ToLower())
            {
                HostNames.names.Remove(lName);
            }
        }
    }
}
