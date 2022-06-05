using System.Collections.Generic;
using dropcatch.com.Models;

namespace dropcatch.com.Services
{
    public static class AppStates
    {
        public static Dictionary<string, Domain> Domains = new Dictionary<string, Domain>();
        public static Dictionary<string, Domain> DomainsToBid = new Dictionary<string, Domain>();
        public static List<Domain> EndedDateDomains = new List<Domain>();
        public static List<string> Users = new List<string>();
        public static string NewBidsExportPath;
        public static string AllDomainsExportPath;
        public static bool ExportedFirstScan;
    }
}