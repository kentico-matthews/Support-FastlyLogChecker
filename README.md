# Support-FastlyLogChecker
A tool for finding erroneous requests for a specific project in the log files from Fastly

Follow Honza Sedo's steps for downloading fastly log files from Atlassian, and put them all in a folder.
Run this tool, selecting that directory and choosing a project ID, and it will report any non-200 responses for that project, along with how many times any search terms you add occur in requests to that project.
