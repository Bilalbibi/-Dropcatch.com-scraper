using System.Collections.Generic;
using System.Threading.Tasks;
using dropcatch.com.Models;

namespace dropcatch.com.Websites
{
    public class Dropcatcher : IWebsite
    {
        public HttpCallerDroppedDoaminsAdder HttpCallerDroppedDoaminsAdder = new HttpCallerDroppedDoaminsAdder();
        //public HttpCaller h = new HttpCaller();
        public MainForm mainForm = new MainForm();
        public async Task Bid(string domain)
        {
            var loginFormData = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("email","alex@lab41.co"),
                new KeyValuePair<string, string>("password","3zcsKQnDkbin"),
            };

            //bidSteps.Add(new BidStep)
            //var ss = await h.GetHtml("https://google.com");
            var login = await HttpCallerDroppedDoaminsAdder.PostFormData("https://www.dropcatcher.co.uk/login/do?redirect=", loginFormData);
            string cause = "";
            if (login.error != null)
            {
                cause = $"Dropcatcher : login failed to dropcatcher.co.uk ==> {login.error}";
            }
            else
            {

            }
            var BackOrderFormData = new List<KeyValuePair<string, string>>()//sundogpictures.co.uk
            {
                new KeyValuePair<string, string>("domain",domain),
            };
            var backOrder = await HttpCallerDroppedDoaminsAdder.PostFormData("https://www.dropcatcher.co.uk/dashboard/catches/insert", BackOrderFormData);
            if (backOrder.error != null)
            {
                mainForm.ErrorLog($"backorder {domain} failed in dropcatcher.co.uk =>  {backOrder.error}");
                return;
            }
            if (backOrder.html.Contains("An error occurred when adding the catching slot"))
            {
                mainForm.NormalLog($"That domain \"{domain }\" has already been booked on dropcatcher.co.uk");
                return;
            }

            mainForm.SuccessLog($"{domain} backordered successfully to dropcatcher.co.uk");

        }
    }
}
