using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dropcatch.com.Models
{
    public interface IWebsite
    {
        Task Bid(string domain);
    }
}
