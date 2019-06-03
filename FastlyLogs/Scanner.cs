using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;

namespace FastlyLogs
{
    public class Scanner
    {
        public string ProjectGuid;

        public string ScanDirectory;

        public int ScannedLines;

        public int ScannedFiles;

        public int TotalFiles;

        public int ProjectTotalRequests;

        public List<NotOK> NotOKs;

        public List<SearchTerm> SearchTerms;

        public Scanner()
        {
            NotOKs = new List<NotOK>();
            SearchTerms = new List<SearchTerm>();
            ScannedLines = 0;
            ScannedFiles = 0;
            ProjectTotalRequests = 0;
        }



        public bool IsRightProject(JToken request)
        {
            return ((string)request["url"]).Contains(ProjectGuid);
        }

        public void CheckIfOK(JToken request)
        {
            if (!((string)request["status"] == "200"))
            {
                NotOKs.Add(new NotOK(request));
            }
        }

        public void CheckSearchTerms(JToken request)
        {
            foreach (SearchTerm term in SearchTerms)
            {
                if (((string)request["url"]).Contains(term.Text))
                {
                    term.UrlOccurances++;
                }
                if (((string)request["query"]).Contains(term.Text))
                {
                    term.QueryStringOccurances++;
                }
            }
        }

        public void Scan(Func<Task<int>> UpdateProgress)
        {
            var files = Directory.GetFiles(ScanDirectory);
            TotalFiles = files.Count();
            foreach (string filename in files)
            {
                ScannedFiles++;
                UpdateProgress();
                using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader streamReader = new StreamReader(fileStream))
                    {
                        while (!streamReader.EndOfStream)
                        {
                            string line = streamReader.ReadLine();
                            ScannedLines++;
                            JToken request = JToken.Parse(line);
                            if (IsRightProject(request))
                            {
                                ProjectTotalRequests++;
                                CheckIfOK(request);
                                CheckSearchTerms(request);
                            }
                        }
                    }
                }
            }
        }
    }
}
