# ABOUT 

This is an upgraded version of https://github.com/Smorgan05/NessusParser. 

Since the last version is for .net framework I decided to upgrade and test.


If you need support for .net 7 feel free to use this version.


# Nessus C# Parser


C# to parse Nessus report (XML) files. 

* nessus-parser - The Nessus report parsing library. There is also some code to connect to a Nessus server and use the API to download reports. This code was developed using Visual Studio 2012 Desktop Express Edition using .Net Framework v4.5.


# Example Usage


C#

    NessusClientData_v2 scan = NessusClientData_v2.Parse("...");
    for(ReportHost host in scan.Report.ReportHosts)
    	Console.WriteLine("{0} ({1} ports, '{2}')", host, host.ReportItems.Count, host.OS);
    

