using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dropcatch.com.Models
{
    public class Bid
    {
        public string UserName { get; set; }
        public decimal Amount { get; set; }

        public bool IsNew { get; set; }
        public bool IsHighestBid { get; set; }
        public Domain Domain { get; set; }
    }
}
