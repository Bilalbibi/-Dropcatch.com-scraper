using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dropcatch.com.Models
{
    public class DomainToCheck
    {
        public string DomainName { get; set; }
        public string EndDate { get; set; }
        public int Pa { get; set; }
        public int Da { get; set; }
        public int Links { get; set; }
        public int Equity { get; set; }
        public string IsSeoDomain { get; set; }
        public int UserSeoBidedNbr { get; set; }
        public string IsUserSeoBided { get; set; }
        public string HighestBid { get; set; }
        public double Tf { get; set; }
        public double Cf { get; set; }
        public string TFDividedByCF { get; set; }
    }
}
