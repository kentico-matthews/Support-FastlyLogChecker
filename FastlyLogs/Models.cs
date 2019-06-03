using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FastlyLogs
{
    public class NotOK
    {
        public JToken RequestData;

        public string StatusCode;

        public string QueryString;

        public string Url;

        public NotOK(JToken request)
        {
            RequestData = request;
            StatusCode = (string)request["status"];
            QueryString = (string)request["query"];
            Url = (string)request["url"];
        }
    }
    public class SearchTerm
    {
        public string Text;

        public int UrlOccurances;

        public int QueryStringOccurances;

        public SearchTerm(string text)
        {
            Text = text;
            UrlOccurances = 0;
            QueryStringOccurances = 0;
        }
    }
}
