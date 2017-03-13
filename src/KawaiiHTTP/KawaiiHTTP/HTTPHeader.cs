using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KawaiiHTTP
{
    public partial class HTTPHeader
    {
        private Dictionary<string, string> httpFields;
        public Dictionary<string, string> GetFields { get; private set; }
        public HTTP_Method Method { get; set; } = HTTP_Method.UNSUPPORTED;
        public string URL { get; private set; }
        public double Version { get; set; } = 1.1;
        public int StatusCode { get; set; } = 200;
        public string StatusMessage { get; set; } = "OK";

        public string URLDecoded
        {
            get { return StringUtil.URLDecode(this.URL); }
        }
        public List<string> Accept
        {
            get { return StringUtil.Split(this.GetField("Accept"), ','); }
        }
        public List<string> AcceptCharset
        {
            get { return StringUtil.Split(this.GetField("Accept-Charset"), ','); }
        }
        public List<string> AcceptEncoding
        {
            get { return StringUtil.Split(this.GetField("Accept-Encoding"), ','); }
        }
        public List<string> AcceptLanguage
        {
            get { return StringUtil.Split(this.GetField("Accept-Language"), ','); }
        }
        public string AcceptDatetime
        {
            get { return this.GetField("Accept-Datetime"); }
        }
        public string Authorization
        {
            get { return this.GetField("Authorization"); }
        }
        public List<string> CacheControl
        {
            get { return StringUtil.Split(this.GetField("Cache-Control"), ','); }
        }
        public List<string> Connection
        {
            get { return StringUtil.Split(this.GetField("Connection"), ',', true); }
        }
        public List<string> Cookie
        {
            get { return StringUtil.Split(this.GetField("Cookie"), ';'); }
        }
        public int ContentLength
        {
            get
            {
                int v = 0;
                try
                {
                    v = int.Parse(this.GetField("Content-Length"));
                }
                catch (Exception ex)
                {
                    Log.d("Could not parse content length: {0}", ex.Message);
                }

                return v;
            }
        }
        public string ContentMD5
        {
            get { return this.GetField("Content-MD5"); }
        }
        public string ContentType
        {
            get { return this.GetField("Content-Type"); }
        }
        public string Date
        {
            get { return this.GetField("Date"); }
        }
        public KeyValuePair<string, string> Expect
        {
            get { return StringUtil.ParseKV(this.GetField("Expect"), '-'); }
        }
        public string Host
        {
            get { return this.GetField("Host").Trim(); }
        }
        public string Origin
        {
            get { return this.GetField("Origin"); }
        }
        public List<string> Pragma
        {
            get { return StringUtil.Split(this.GetField("Pragma"), ','); }
        }
        public string ProxyAuthorization
        {
            get { return this.GetField("Proxy-Authoriation"); }
        }
        public KeyValuePair<string, string> Range
        {
            get { return StringUtil.ParseKV(this.GetField("Range")); }
        }
        public string Referer
        {
            get { return this.GetField("Referer"); }
        }
        public List<string> TE
        {
            get { return StringUtil.Split(this.GetField("TE"), ','); }
        }
        public string UserAgent
        {
            get { return this.GetField("User-Agent"); }
        }
        public List<string> Upgrade
        {
            get { return StringUtil.Split(this.GetField("Upgrade"), ','); }
        }
        public List<string> Via
        {
            get { return StringUtil.Split(this.GetField("Via"), ','); }
        }
        public string Warning
        {
            get { return this.GetField("Warning"); }
        }
        public bool DNT
        {
            get { return this.GetField("DNT") == "1"; }
        }
        public string GetField(string key)
        {
            string pkey = key;
            if (!this.httpFields.ContainsKey(key))
            {
                pkey = key.ToLower();
                if (!this.httpFields.ContainsKey(pkey))
                {
                    return "";
                }

                return "";
            }
            else
            {
                return this.httpFields[pkey];
            }
        }
        public void SetField(string key, string value)
        {
            if (this.httpFields.ContainsKey(key))
            {
                this.httpFields.Remove(key);
                Log.d("Overridden HTTP field {0}: {1}", key, value);
            }

            this.httpFields[key] = value;
        }
        public void AppendField(string key, string value, string glue)
        {
            string ovalue = "";

            if (this.httpFields.ContainsKey(key))
            {
                ovalue = this.httpFields[key].TrimEnd();
                this.httpFields.Remove(key);
            }

            if (!ovalue.EndsWith(glue))
            {
                ovalue += glue;
            }

            this.httpFields[key] = ovalue + value;
        }
        private void BuildFields(ref StringBuilder fieldBuilder)
        {
            foreach (KeyValuePair<string, string> field in this.httpFields)
            {
                fieldBuilder.Append(field.Key);
                fieldBuilder.Append(": ");
                fieldBuilder.Append(field.Value);
                fieldBuilder.Append("\r\n");
            }
        }
        public string ConcatGetValues()
        {
            if (this.GetFields.Count <= 0) { return ""; }
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, string> v in this.GetFields)
            {
                builder.Append(string.Format("&{0}={1}", v.Key, v.Value));
            }

            return string.Format("?{0}", builder.ToString(1, builder.Length - 1));
        }
        public override string ToString()
        {
            string request = string.Format("{0} {1} HTTP/{2}", this.Method, this.URL, this.Version);
            StringBuilder fieldBuilder = new StringBuilder();
            this.BuildFields(ref fieldBuilder);

            return string.Format("{0}\n{1}", request, fieldBuilder.ToString());
        }
        public void DumpFields(System.IO.BufferedStream stream)
        {
            StringBuilder fieldBuilder = new StringBuilder();
            this.BuildFields(ref fieldBuilder);

            byte[] fieldBytes = Encoding.UTF8.GetBytes(fieldBuilder.ToString());
            stream.Write(fieldBytes, 0, fieldBytes.Length);
        }

        public Dictionary<string, string> HTTPFields
        {
            get { return this.httpFields; }
        }
        public HTTPHeader()
        {
            this.httpFields = new Dictionary<string, string>();
            this.URL = "";
        }
        public HTTPHeader(string url, Dictionary<string, string> fields, Dictionary<string, string> gFields)
        {
            this.httpFields = fields;
            this.URL = url;
            this.GetFields = gFields;
        }
        public HTTPHeader(HTTP_Method method, string url, Dictionary<string, string> fields, Dictionary<string, string> gFields)
        {
            this.httpFields = fields;
            this.Method = method;
            this.URL = url;
            this.GetFields = gFields;
        }
    }
}
