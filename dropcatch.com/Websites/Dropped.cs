using System.Collections.Generic;
using System.Threading.Tasks;
using dropcatch.com.Models;

namespace dropcatch.com.Websites
{
    public class Dropped : IWebsite
    {
        public HttpCallerDroppedDoaminsAdder HttpCallerDroppedDoaminsAdder = new HttpCallerDroppedDoaminsAdder();
        public MainForm mainForm = new MainForm();
       
        public async Task Bid(string domain)
        {
           
            var loginFormData = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("username","alex@lab41.co"),
                new KeyValuePair<string, string>("password","3zcsKQnDkbin")
            };
            var login = await HttpCallerDroppedDoaminsAdder.PostFormData("https://dropped.uk/login", loginFormData);
            if (login.error != null)
            {
                mainForm.ErrorLog($"login failed to dropped.uk for backordering on {domain}");

                return;
            }
            var backOrderSearchFormData = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("domain",domain)
            };

            var backOrderSearch = await HttpCallerDroppedDoaminsAdder.PostFormData("https://dropped.uk/account/backorder", backOrderSearchFormData);
            if (backOrderSearch.error != null)
            {
                mainForm.ErrorLog($"search for { domain }  failed in dropped.uk : { backOrderSearch.error}");
                return;
            }
            var backOrderFormData = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("book",$"Submit backorder for {domain}")
            };
            var backOrder = await HttpCallerDroppedDoaminsAdder.PostFormData("https://dropped.uk/account/book", backOrderFormData);
            if (backOrder.error != null)
            {
                mainForm.ErrorLog($"backorder {domain} failed to dropped.uk => {backOrder.error}");
                return;
            }
            mainForm.SuccessLog($"{domain} backordered successfully to dropped.uk ");

        }
    }
}
