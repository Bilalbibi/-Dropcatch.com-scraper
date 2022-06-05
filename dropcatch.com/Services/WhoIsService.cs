using dropcatch.com.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dropcatch.com.Services
{
    public static class WhoIsService
    {
        static private HttpCaller _httpCaller = new HttpCaller();
        public static async Task GetRegisteredDate(Domain domian)
        {
            var doc = await _httpCaller.GetDoc($"https://who.is/whois/{domian.DomainName}");

            try
            {
                var regDate = doc.DocumentNode.SelectSingleNode("//div[text()='Registered On']/following-sibling::div")
                    ?.InnerText.Trim() ?? "";
                if (regDate!="")
                {
                    domian.RegisteredDate = DateTime.Parse(regDate); 
                }
            }
            catch (Exception)
            {
            }


        }
    }
}
