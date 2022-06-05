using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using dropcatch.com.Models;
using Newtonsoft.Json.Linq;

namespace dropcatch.com.Websites
{
    public class Catchtiger : IWebsite
    {
        public HttpCallerDroppedDoaminsAdder HttpCallerDroppedDoaminsAdder = new HttpCallerDroppedDoaminsAdder();
        public MainForm mainForm = new MainForm();

        public async Task Bid(string domain)
        {


            var loginFormData = new List<KeyValuePair<string, string>>()
            {

                new KeyValuePair<string, string>("action","login"),
                new KeyValuePair<string, string>("nietinvullen",""),
                new KeyValuePair<string, string>("ref","inloggen"),
                new KeyValuePair<string, string>("login[email]","alex@lab41.co"),
                new KeyValuePair<string, string>("login[password]","iex1!1r92Vde")
            };
            var login = await HttpCallerDroppedDoaminsAdder.PostFormData("https://www.catchtiger.com/en/inloggen/", loginFormData);
            if (login.error != null)
            {
                mainForm.ErrorLog($"login failed to catchtiger.com for backordering on {domain}");
                return;
            }
            double mountBid = 0;
            var domainInfo = await HttpCallerDroppedDoaminsAdder.GetHtml($"https://www.catchtiger.com/en/?async=yes&action=domains&do=getBids&domain={domain}");
            if (domainInfo.error != null)
            {
                mainForm.ErrorLog("We couldn't get the highest Bid from catchtiger.com ===> " + domainInfo.error);

            }
            //Console.WriteLine(domainInfo.html);
            try
            {
                var Object = JObject.Parse(domainInfo.html);
                var highestBid = double.Parse((string)Object.SelectToken("highestBid") ?? "0.00");
                if (highestBid > 35)
                {
                    mainForm.ErrorLog($"the bid price is more than $35 in catchtiger for {domain}");
                    return;
                }
                mountBid = 36;
                //Console.WriteLine(highestBid);
                //Console.WriteLine(mountBid);
            }
            catch (Exception)
            {
                mountBid = 35;
            }
            //get end date biding
            var endDateBidJson = await HttpCallerDroppedDoaminsAdder.GetHtml($"https://www.catchtiger.com/en/?action=domains&do=getAll&async=yes&pagId=domeinnaam-veilingen&exclusive=0&view=basic&draw=7&columns%5B0%5D%5Bdata%5D=domain&columns%5B0%5D%5Bname%5D=&columns%5B0%5D%5Bsearchable%5D=true&columns%5B0%5D%5Borderable%5D=true&columns%5B0%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B0%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B1%5D%5Bdata%5D=length&columns%5B1%5D%5Bname%5D=&columns%5B1%5D%5Bsearchable%5D=true&columns%5B1%5D%5Borderable%5D=true&columns%5B1%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B1%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B2%5D%5Bdata%5D=score&columns%5B2%5D%5Bname%5D=&columns%5B2%5D%5Bsearchable%5D=true&columns%5B2%5D%5Borderable%5D=true&columns%5B2%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B2%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B3%5D%5Bdata%5D=secondsRemaining&columns%5B3%5D%5Bname%5D=&columns%5B3%5D%5Bsearchable%5D=false&columns%5B3%5D%5Borderable%5D=true&columns%5B3%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B3%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B4%5D%5Bdata%5D=iBids&columns%5B4%5D%5Bname%5D=&columns%5B4%5D%5Bsearchable%5D=false&columns%5B4%5D%5Borderable%5D=true&columns%5B4%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B4%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B5%5D%5Bdata%5D=highestBid&columns%5B5%5D%5Bname%5D=&columns%5B5%5D%5Bsearchable%5D=false&columns%5B5%5D%5Borderable%5D=true&columns%5B5%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B5%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B6%5D%5Bdata%5D=googleResults&columns%5B6%5D%5Bname%5D=&columns%5B6%5D%5Bsearchable%5D=true&columns%5B6%5D%5Borderable%5D=true&columns%5B6%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B6%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B7%5D%5Bdata%5D=googleAds&columns%5B7%5D%5Bname%5D=&columns%5B7%5D%5Bsearchable%5D=false&columns%5B7%5D%5Borderable%5D=true&columns%5B7%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B7%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B8%5D%5Bdata%5D=alexa&columns%5B8%5D%5Bname%5D=&columns%5B8%5D%5Bsearchable%5D=true&columns%5B8%5D%5Borderable%5D=true&columns%5B8%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B8%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B9%5D%5Bdata%5D=isWord&columns%5B9%5D%5Bname%5D=&columns%5B9%5D%5Bsearchable%5D=false&columns%5B9%5D%5Borderable%5D=true&columns%5B9%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B9%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B10%5D%5Bdata%5D=action&columns%5B10%5D%5Bname%5D=&columns%5B10%5D%5Bsearchable%5D=false&columns%5B10%5D%5Borderable%5D=false&columns%5B10%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B10%5D%5Bsearch%5D%5Bregex%5D=false&order%5B0%5D%5Bcolumn%5D=4&order%5B0%5D%5Bdir%5D=desc&start=0&length=50&search%5Bvalue%5D={domain}&search%5Bregex%5D=false");
            if (endDateBidJson.error != null)
            {
                mainForm.ErrorLog($"couldn't scrape the end date bid of {domain} in catchtiger.com ==> {endDateBidJson.error} ");
            }
            else
            {
                var dateEnd = "";
                try
                {
                    var Object = JObject.Parse(endDateBidJson.html);
                    foreach (var item in Object["data"])
                    {
                        var date = (string)item.SelectToken("dateEnd");
                        dateEnd = DateTime.Parse(date).ToString("dd/MM/yyyy");
                    }
                }
                catch (Exception)
                {
                    mainForm.ErrorLog($"we couldn't get the end date bid of {domain} in catchtiger.com because the json data is malformed");
                }
            }
            var backOrderFormData = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("action","domains"),
                new KeyValuePair<string, string>("do","placeBid"),
                new KeyValuePair<string, string>("domain",domain),
                new KeyValuePair<string, string>("bid",""+mountBid),
                new KeyValuePair<string, string>("bidType","static")
            };
            var backOrder = await HttpCallerDroppedDoaminsAdder.PostFormData("https://www.catchtiger.com/en/?async=yes&action=domains&do=placeBid", backOrderFormData);
            if (backOrder.error != null)
            {
                mainForm.ErrorLog($"bidding on {domain} failed in catchtiger.com => " + backOrder.error);

                return;
            }
            if (backOrder.html != "1")
            {
                mainForm.ErrorLog($"{domain} not dropped yet in catchtiger.com or we already we make auction on it");
                return;
            }
            mainForm.SuccessLog($"{domain} backordered successfully to catchtiger.com ");


        }
    }
}
