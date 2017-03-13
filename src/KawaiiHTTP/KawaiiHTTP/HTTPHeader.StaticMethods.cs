using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KawaiiHTTP
{
    public partial class HTTPHeader
    {
        private static Dictionary<string, string> ParseGetFields(string inputURL)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>();

            string[] exploded_url = inputURL.Split('?');
            if (exploded_url.Length >= 2)
            {
                foreach (string value in exploded_url[exploded_url.Length - 1].Split('&'))
                {
                    KeyValuePair<string, string> parsedValue = StringUtil.ParseKV(StringUtil.URLDecode(value));
                    if (fields.ContainsKey(parsedValue.Key))
                    {
                        fields.Remove(parsedValue.Key);
                    }

                    fields.Add(parsedValue.Key, parsedValue.Value);
                }
            }

            return fields;
        }
        private static string ExtractURL(string inputURL)
        {
            return inputURL.Split('?')[0];
        }
        public static HTTPHeader ParseRequest(string requestString)
        {
            HTTP_Method method = HTTP_Method.UNSUPPORTED;
            string url = "/";
            double version = 0;
            Dictionary<string, string> httpFields = new Dictionary<string, string>();
            Dictionary<string, string> getFields = new Dictionary<string, string>();

            string[] lines = requestString.Split('\n'); // Take the HTTP request and split it into the seperate lines
            string[] requestParts = lines[0].Split(' '); // Take line 0 (the request string) and split it into its components
            int partCounter = 0;
            for (int i = 0; i < requestParts.Count(); i++)
            {
                string part = requestParts[i];
                if (part == string.Empty) { continue; } // No place for empty strings

                switch (partCounter)
                {
                    case 0: // Request method
                        try
                        {
                            // Some top kek method selecting
                            method = (HTTP_Method)System.Enum.Parse(typeof(HTTP_Method), part.ToUpper());
                        }
                        catch (Exception ex)
                        {
                            Log.d("Got an unsupported HTTP Request method: {0}", ex.Message);
                        }

                        break;
                    case 1: // Request URL
                        url = HTTPHeader.ExtractURL(part);
                        if (url != part)
                        {
                            getFields = HTTPHeader.ParseGetFields(part);
                        }
                        break;
                    case 2: // Request HTTP Version
                        string part_lower = part.ToLower();
                        if (part_lower.StartsWith("http/") && part.Length >= 5)
                        {
                            try
                            {
                                version = double.Parse(part_lower.Substring(5));
                            }
                            catch (Exception ex)
                            {
                                Log.d("Could not parse HTTP Version: {0}", ex.Message);
                            }
                        }

                        break;
                    default:
                        // Well that was awkward
                        break;
                }

                partCounter++;
            }

            if (lines.Length > 1)
            { // We've got ourselves some fields, lets parse those <3
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (line == string.Empty || line.Length < 3) { continue; }
                    string key = ""; // Fields aren't that long, so whatever
                    int keyPosition = 0;
                    for (int c = 0; c < line.Length; c++) // C++, hue hue hue
                    {
                        string chr = line.Substring(c, 1);
                        if (chr == ":") { keyPosition = c; break; }
                    }

                    key = line.Substring(0, keyPosition);
                    if (httpFields.ContainsKey(key))
                    {
                        Log.d("HTTP header contains duplicate field: {0}", key);
                    }
                    else
                    {
                        httpFields[key] = line.Substring(keyPosition + 1);
                    }
                }
            }

            HTTPHeader newHeader = new HTTPHeader(method, url, httpFields, getFields);
            newHeader.Version = version;

            return newHeader;
        }

        public static HTTPHeader GenerateResponse()
        {
            HTTPHeader header = new HTTPHeader();
            header.SetField("Content-Type", "text/html");
            header.Version = 1.1;
            return header;
        }
    }
}
