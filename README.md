Nessus Tools
=================================

C# to parse Nessus report (XML) files. 

* nessus-tools - The Nessus report parsing library. There is also some code to connect to a Nessus server and use the API to download reports. This code was developed using Visual Studio 2012 Desktop Express Edition using .Net Framework v4.5.


Example Usage
=================================

C#

    NessusClientData_v2 scan = NessusClientData_v2.Parse("...");
    for(ReportHost host in scan.Report.ReportHosts)
    	Console.WriteLine("{0} ({1} ports, '{2}')", host, host.ReportItems.Count, host.OS);
    
Documentation Incoming...
