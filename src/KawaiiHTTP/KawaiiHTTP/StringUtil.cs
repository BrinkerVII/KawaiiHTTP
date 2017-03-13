using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KawaiiHTTP
{
    public class StringUtil
    {
        public static KeyValuePair<string, string> ParseKV(string input, char delimeter = '=')
        {
            string key = input;
            string value = "";
            string[] split = input.Split(delimeter);
            if (split.Length >= 2)
            {
                key = split[0];
                value = split[1];
            }

            return new KeyValuePair<string, string>(key, value);
        }

        public static List<string> Split(string input, char delimeter, bool toLower)
        {
            List<string> list = StringUtil.Split(input, delimeter);
            StringUtil.ListToLower(list);
            return list;
        }
        public static List<string> Split(string input, char delimeter)
        {
            if (input == null) { return new List<string>(); }
            if (input == string.Empty) { return new List<string>(); }

            List<string> list = new List<string>();
            foreach (string s in input.Split(delimeter))
            {
                list.Add(s.Trim());
            }

            return list;
        }

        public static void ListToLower(List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = list[i].ToLower();
            }
        }
        public string MemoryToString(ref System.IO.MemoryStream stream)
        {
            long startPosition = stream.Position;
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            byte[] memoryBytes = new byte[stream.Length];
            stream.Read(memoryBytes, 0, (int)stream.Length);
            stream.Seek(startPosition, System.IO.SeekOrigin.Begin);

            return Encoding.UTF8.GetString(memoryBytes);
        }
        public static string URLDecode(string input)
        {
            string decodedValue = input.Replace("+", " ");
            if (decodedValue.IndexOf('%') != -1)
            {
                int idx;
                while ((idx = decodedValue.IndexOf('%')) != -1)
                {
                    if (idx + 2 <= decodedValue.Length)
                    {
                        int parsedInt = 0;
                        try
                        {
                            parsedInt = Int32.Parse(decodedValue.Substring(idx + 1, 2), System.Globalization.NumberStyles.HexNumber);
                        }
                        catch (Exception ex)
                        {
                            ex.ToString(); // w/e
                        }
                        char chr = '\0';
                        try
                        {
                            chr = Convert.ToChar(parsedInt);
                        }
                        catch (Exception ex)
                        {
                            ex.ToString(); // wow
                        }
                        decodedValue = decodedValue.Replace(decodedValue.Substring(idx, 3), chr.ToString());
                    }
                }
            }

            return decodedValue;
        }
        public static string URLToWindows(string input)
        {
            return StringUtil.URLDecode(input).Replace("/", "\\");
        }
        public static string WindowsToURL(string input)
        {
            return input.Replace("\\", "/").Replace(" ", "%20");
        }
        public static string TruncatePath(string input)
        {
            string[] split = input.Split('\\');
            return split[split.Length - 1];
        }
        public static string UrlDecode(string input)
        {
            string decodedValue = input.Replace("+", " ");
            if (decodedValue.IndexOf('%') != -1)
            {
                int idx;
                while ((idx = decodedValue.IndexOf('%')) != -1)
                {
                    if (idx + 2 <= decodedValue.Length)
                    {
                        decodedValue = decodedValue.Replace(decodedValue.Substring(idx, 3), Convert.ToChar(Int32.Parse(decodedValue.Substring(idx + 1, 2), System.Globalization.NumberStyles.HexNumber)).ToString());
                    }
                }
            }

            return decodedValue;
        }
        public static string GetExtension(string input)
        {
            string[] split = input.Split('.');
            if (split.Length >= 2)
            {
                return split[split.Length - 1];
            }
            else
            {
                return "no_extension";
            }
        }
    }
}
