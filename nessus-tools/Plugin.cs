using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nessus_tools
{
    /// <summary>
    /// Represents a Nessus Plugin.
    /// </summary>
    public sealed class Plugin
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public DateTime LastModified { get; set; }
        public string Output { get; set; }
        public Criticality Criticality { get; set; }
        public string Description { get; set; }
        public string Synopsis { get; set; } = string.Empty;
        public string Solution { get; set; } = string.Empty;
        public int Severity { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="type">Type</param>
        /// <param name="lastModified">Last Modified</param>
        /// <param name="output">Plugin output</param>
        /// <param name="description">Description</param>
        /// <param name="criticality">Criticality</param>
        public Plugin(string name, string type, DateTime lastModified, string output, string description, Criticality criticality)
        {
            Name = name;
            Type = type;
            Output = output;
            LastModified = lastModified;
            Criticality = criticality;
            Description = description;
        }

        /// <summary>
        /// Generates the Plugin from the ReportItem given.
        /// </summary>
        /// <param name="item">ReportItem</param>
        public Plugin(ReportItem item) : this(item.Plugin_Name, item.Plugin_Type, DateTime.Now, item.PluginOutput, item.Description, item.Criticality)
        {
            try
            {
                LastModified = DateTime.Parse(item.PluginModificationDate);
                Severity = Int32.Parse(item.Severity);
            }catch{}

            Synopsis = item.Synopsis;
            Solution = item.Solution;
        }

        /// <summary>
        /// Returns a valid HashCode for this Plugin.
        /// </summary>
        /// <returns>int</returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Type.GetHashCode() ^ Description.GetHashCode();
        }

    }

    /// <summary>
    /// IEqualityComparer implementation for Plugin objects.
    /// </summary>
    public sealed class PluginComparer : IEqualityComparer<Plugin>
    {

        public bool Equals(Plugin? x, Plugin? y)
        {
            if (x == null || y == null)
            {
                return false;
            }
            return (x.GetHashCode() == y.GetHashCode());
        }

        public int GetHashCode(Plugin obj)
        {
            return obj.GetHashCode();
        }

    }

}
