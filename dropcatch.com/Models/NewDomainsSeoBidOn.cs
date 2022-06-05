using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dropcatch.com.Models
{
    public class NewDomainsSeoBidOn
    {
        public string SeoName { get; set; }
        public bool IsUserSeoReported { get; set; }
        public HashSet<ReportedDomains> domains { get; set; } = new HashSet<ReportedDomains>();
    }
}
