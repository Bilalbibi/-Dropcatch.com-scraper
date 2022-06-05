using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dropcatch.com.Models
{
    public class SitesDomainsBids
    {
       
        public Dictionary<string, List<string>> SitesAndDomainsBid = new Dictionary<string, List<string>>();
        public int SiteName { get; set; }
        public List<DomainInfo> Domains = new List<DomainInfo>();

    }
}
