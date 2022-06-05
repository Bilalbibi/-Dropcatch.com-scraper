using System;
using System.Collections.Generic;

namespace dropcatch.com.Models
{
    public class Domain
    {
        public string DomainName { get; set; }
        public string DomainId { get; set; }
        public string NumberOfBids { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime RegisteredDate { get; set; } = new DateTime();
        public string Pa { get; set; }
        public decimal Da { get; set; }
        public int Links { get; set; }
        public string Equity { get; set; }
        public double Tf { get; set; }
        public double Cf { get; set; }
        public decimal MaxBid { get; set; }
        public int GoogleResults { get; set; }
        public string AhrefsRank { get; set; }
        public string AhrefsRD { get; set; }
        public string AhrefsDR { get; set; }
        public string AhrefsEB { get; set; }

        public List<Bid> Bids { get; set; } = new List<Bid>();
    }
}