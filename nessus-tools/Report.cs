﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Collections;

namespace nessus_tools
{
    /// <summary>
    /// Represents a NESSUS Report. See 
    /// http://static.tenable.com/documentation/nessus_v2_file_format.pdf for
    /// more information.
    /// </summary>
    public sealed class Report
    {

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("ReportHost")]
        public ReportHosts ReportHosts { get; set; }

        public Report()
        {
            Name = string.Empty;
            ReportHosts = new ReportHosts();
        }

        /// <summary>
        /// Parses a file at the given path for a NESSUS Report object.
        /// </summary>
        /// <param name="path">Path to the file to parse. (*.nessus)</param>
        /// <returns>Report; null if an error occurred.</returns>
        public static Report? Parse(string path)
        {
            Report? report = null;
            XmlSerializer serializer = new XmlSerializer(typeof(NessusClientData_v2));
            TextReader? reader = null;
            try
            {
                reader = new StreamReader(File.OpenRead(path));
                NessusClientData_v2 data = (NessusClientData_v2)serializer.Deserialize(reader)!;
                report = data.Report;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: Could not parse (NESSUS?) file {0}: {1} {2}", path, e.GetType(), e.Message);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }

            return report;
        }

    }

    /// <summary>
    /// Criticality is normally used in reference to the Risk_Factor member/element of ReportItem.
    /// </summary>
    public enum Criticality
    {
        Critical = 0x00,
        High = 0x01,
        Medium = 0x02,
        Low = 0x03,
        None = 0x04
    }

    /// <summary>
    /// Represents a ReportHost element.
    /// </summary>
    public sealed class ReportHost
    {
        /// <summary>
        /// Name of the ReportHost
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// Host properties containing what information NESSUS could derive for the host.
        /// </summary>
        [XmlElement("HostProperties")]
        public HostProperties HostProperties { get; set; }

        /// <summary>
        /// ReportItem for the ReportHost.
        /// </summary>
        [XmlElement("ReportItem")]
        public List<ReportItem> ReportItems { get; set; }

        /// <summary>
        /// Attemps to return the overall criticality of this host.
        /// </summary>
        public Criticality Criticality
        {
            get
            {
                int criticals = 0;
                int highs = 0;
                int mediums = 0;
                int lows = 0;
                int none = 0;

                foreach (ReportItem item in ReportItems)
                {
                    string risk = item.RiskFactor.ToLower();
                    switch (risk)
                    {
                        case "critical":
                            criticals++;
                            break;
                        case "high":
                            highs++;
                            break;
                        case "medium":
                            mediums++;
                            break;
                        case "low":
                            lows++;
                            break;
                        default:
                            none++;
                            break;
                         
                    }
                }

                if (criticals > 0)
                    return Criticality.Critical;
                else if (highs > 0)
                    return Criticality.High;
                else if (mediums > 0)
                    return Criticality.Medium;
                else if (lows > none)
                    return Criticality.Low;

                return Criticality.None;
            }
        }

        /// <summary>
        /// Attempts to return the OS detected by NESSUS.
        /// </summary>
        public string OS
        {
            get
            {
                foreach (Tag t in HostProperties.Tags)
                {
                    if (t.Name.Equals("operating-system"))
                        return t.Value;

                }

                return "(unknown)";
            }
        }

        /// <summary>
        /// Attempts to return the Netbios Name of this host (if detected by NESSUS).
        /// </summary>
        public string NetbiosName
        {
            get
            {
                foreach (Tag t in HostProperties.Tags)
                {
                    if (t.Name.Equals("netbios-name"))
                        return t.Value;

                }

                return "(unknown)";
            }
        }

        /// <summary>
        /// Returns when the scan started for this ReportHost.
        /// </summary>
        public DateTime ScanStarted
        {
            get
            {
                DateTime time = DateTime.Now;
                foreach (Tag t in HostProperties.Tags)
                {
                    if (t.Name.Equals("HOST_START"))
                    {
                        time = DateTime.Parse(t.Value);
                    }
                }

                return time;
            }
        }

        /// <summary>
        /// Returns the time the scan was completed for this ReportHost.
        /// </summary>
        public DateTime ScanEnded
        {
            get
            {
                DateTime time = DateTime.Now;
                foreach (Tag t in HostProperties.Tags)
                {
                    if (t.Name.Equals("HOST_END"))
                    {
                        time = DateTime.Parse(t.Value);
                    }
                }

                return time;
            }
        }

        /// <summary>
        /// MAC Address (if detected)
        /// </summary>
        public string MacAddress
        {
            get
            {
                string mac = "(unknown)";
                foreach (Tag t in HostProperties.Tags)
                {
                    if (t.Name.Equals("mac-address"))
                    {
                        mac = t.Value;
                    }
                }

                return mac;
            }
        }

        /// <summary>
        /// Fully Qualified Domain Name (if detected)
        /// </summary>
        public string FQDN
        {
            get
            {
                string fqdn = string.Empty;
                foreach (Tag t in HostProperties.Tags)
                {
                    if (t.Name.Equals("host-fqdn"))
                        fqdn = t.Value;

                }

                return fqdn;
            }
        }

        /// <summary>
        /// IP Address
        /// </summary>
        public string IpAddress
        {
            get
            {
                string ip = Name;
                foreach (Tag t in HostProperties.Tags)
                {
                    if (t.Name.Equals("host-ip"))
                        ip = t.Value;

                }

                return ip;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ReportHost()
        {
            Name = string.Empty;
            HostProperties = new HostProperties();
            ReportItems = new List<ReportItem>();
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() & IpAddress.GetHashCode();
        }

    }

    public sealed class ReportHostComparer : IEqualityComparer<ReportHost>
    {

        public bool Equals(ReportHost? x, ReportHost? y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            return (x.GetHashCode() == y.GetHashCode());
        }

        public int GetHashCode(ReportHost obj)
        {
            return obj.GetHashCode();
        }
    }

    /// <summary>
    /// Simple collection of ReportHost objects.
    /// </summary>
    public sealed class ReportHosts : CollectionBase
    {
        public int Add(ReportHost h)
        {
            return List.Add(h);
        }

        public bool Contains(ReportHost h)
        {
            //TODO
            return false;
        }

        public ReportHost? this[int index]
        {
            get { return (ReportHost?)List[index]; }
            set { List[index] = value; }
        }
    }

    /// <summary>
    /// HostProperties element. A collection of Tag elements really.
    /// </summary>
    public sealed class HostProperties
    {

        [XmlElement("tag")]
        public List<Tag> Tags { get; set; }

        public HostProperties()
        {
            Tags = new List<Tag>();
        }

        public HostProperties(XElement element) : this()
        {
            foreach (XElement child in element.Descendants("tag"))
            {
                Tags.Add(new Tag(child));
            }
        }
    }

    /// <summary>
    /// Tag element within a HostProperties element.
    /// </summary>
    public sealed class Tag
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlText()]
        public string Value { get; set; }

        public Tag()
        {
            Name = string.Empty;
            Value = string.Empty;
        }

        public Tag(XElement element) : this()
        {
            Name = element.Attribute("name")!.Value;
            Value = element.Value;
        }
    }

    /// <summary>
    /// Represents a ReportItem within a ReportHost.
    /// </summary>
    public sealed class ReportItem
    {
        /// <summary>
        /// Plugin Id
        /// </summary>
        [XmlAttribute("pluginID")]
        public string PluginId { get; set; } = string.Empty;
        
        /// <summary>
        /// Plugin Name
        /// </summary>
        [XmlAttribute("pluginName")]
        public string PluginName { get; set; } = string.Empty;
        
        /// <summary>
        /// Plugin Name
        /// </summary>
        [XmlAttribute("pluginFamily")]
        public string PluginFamily { get; set; } = string.Empty;
        
        /// <summary>
        /// Port
        /// </summary>
        [XmlAttribute("port")]
       public int Port { get; set; }

        /// <summary>
        /// NESSUS attempt to determine the service on the Port.
        /// </summary>
        [XmlAttribute("svc_name")]
        public string ServiceName { get; set; }

        /// <summary>
        /// Protocol for this Port
        /// </summary>
        [XmlAttribute("protocol")]
        public string Protocol { get; set; }

        /// <summary>
        /// Severity of the Port being open.
        /// </summary>
        [XmlAttribute("severity")]
        public string Severity { get; set; }

        [XmlElement("age_of_vuln")] public string AgeOfVulnerability { get; set; } = String.Empty;
        [XmlElement("cpe")] public string Cpe { get; set; } = String.Empty;
        
        [XmlElement("cve")] public List<string> CVEs { get; set; } = new List<string>();
        
        [XmlElement("cvss3_base_score")] public float? CVSS3BaseScore { get; set; }
        [XmlElement("cvss3_temporal_score")] public float? CVSS3TemporalScore { get; set; }
        [XmlElement("cvssV3_impactScore")] public float? CVSS3ImpactScore { get; set; }
        [XmlElement("cvss3_temporal_vector")] public string? CVSS3TemporalVector { get; set; } 
        [XmlElement("cvss3_vector")] public string? CVSS3Vector { get; set; }
        
        [XmlElement("cvss_base_score")] public float? CVSSBaseScore { get; set; }
        [XmlElement("cvss_temporal_score")] public float? CVSSTemporalScore { get; set; }
        [XmlElement("cvss_temporal_vector")] public string? CVSSTemporalVector { get; set; } 
        [XmlElement("cvss_score_source")] public string? CVSSScoreSource { get; set; } 
        [XmlElement("cvss_vector")] public string? CVSSVector { get; set; }
        
        [XmlElement("exploit_available")] public bool? ExploitAvailable { get; set; }
        [XmlElement("exploit_code_maturity")] public string? ExploitCodeMaturity { get; set; }
        [XmlElement("exploitability_ease")] public string? ExploitabilityEasy { get; set; }
        [XmlElement("exploited_by_nessus")] public bool? ExploitedByNessus { get; set; }
        
        [XmlElement("patch_publication_date")] public string? PatchPublicationDate { get; set; }
        [XmlElement("threat_intensity_last_28")] public string? ThreatIntensityLast28 { get; set; }
        [XmlElement("threat_recency")] public string? ThreatRecency { get; set; }
        [XmlElement("threat_sources_last_28")] public string? ThreatSourcesLast28 { get; set; }
        
        [XmlElement("vpr_score")] public float? VPRScore { get; set; }
        [XmlElement("vuln_publication_date")] public string? VulnerabilityPublicationDate { get; set; }
        [XmlElement("xref")] public List<string> Xref { get; set; } = new List<string>();
        [XmlElement("iava")] public List<string> IAVA { get; set; } = new List<string>();
        [XmlElement("msft")] public List<string> Msft { get; set; } = new List<string>();
        [XmlElement("mskb")] public List<string> Mskb { get; set; } = new List<string>();
        [XmlElement("see_also")] public string? SeeAlso { get; set; }
        [XmlElement("stig_severity")] public string? StigSeverity { get; set; }
        
        
        
        /// <summary>
        /// Description of the ReportItem
        /// </summary>
        [XmlElement("description")]
        public string Description { get; set; }

        /// <summary>
        /// Filename of the plugin ran for this ReportItem.
        /// </summary>
        [XmlElement("fname")]
        public string FName { get; set; } = string.Empty;

        /// <summary>
        /// Plugin Modification Date
        /// </summary>
        [XmlElement("plugin_modification_date")]
        public string PluginModificationDate { get; set; } = string.Empty;

        /// <summary>
        /// Name of the plugin used to generate this ReportItem.
        /// </summary>
        [XmlElement("plugin_name")]
        public string Plugin_Name { get; set; } = string.Empty;

        /// <summary>
        /// Type of plugin.
        /// </summary>
        [XmlElement("plugin_type")]
        public string Plugin_Type { get; set; } = string.Empty;

        /// <summary>
        /// Risk Factor
        /// </summary>
        [XmlElement("risk_factor")]
        public string RiskFactor { get; set; } = string.Empty;

        /// <summary>
        /// Solution
        /// </summary>
        [XmlElement("solution")]
        public string Solution { get; set; } = string.Empty;

        /// <summary>
        /// Synopsis
        /// </summary>
        [XmlElement("synopsis")]
        public string Synopsis { get; set; } = string.Empty;

        /// <summary>
        /// Actual plugin output.
        /// </summary>
        [XmlElement("plugin_output")]
        public string PluginOutput { get; set; } = string.Empty;

        /// <summary>
        /// Criticality of this ReportItem. 
        /// </summary>
        public Criticality Criticality
        {
            get
            {
                string riskLower = RiskFactor.ToLower();
                switch (riskLower)
                {
                    case "critical":
                        return Criticality.Critical;
                    case "high":
                        return Criticality.High;
                    case "medium":
                        return Criticality.Medium;
                    case "low":
                        return Criticality.Low;

                }

                return Criticality.None;
            }
        }

        public ReportItem()
        {
            Port = 0;
            ServiceName = string.Empty;
            Protocol = "TCP";
            Severity = "none";
            Description = string.Empty;
        }

        public ReportItem(XElement element) : this()
        {
            Port = int.Parse(element.Attribute("port")!.Value);
            ServiceName = element.Attribute("svc_name")!.Value;
            Protocol = element.Attribute("protocol")!.Value;
            Severity = element.Attribute("severity")!.Value;
            //TODO More child elements to parse here.
        }

    }

}
