using System.Collections.Generic;
using System.Threading.Tasks;
using dropcatch.com.Models;

namespace dropcatch.com.Websites
{
    public class caught_co_uk : IWebsite
    {
        public HttpCallerDroppedDoaminsAdder HttpCallerDroppedDoaminsAdder = new HttpCallerDroppedDoaminsAdder();
        public MainForm mainForm = new MainForm();

        public async Task Bid(string domain)
        {
          

            var loginFormData = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("user","AlexR"),
                new KeyValuePair<string, string>("pass","2chYY5taDfDN"),
                new KeyValuePair<string, string>("sublogin","1")
            };
            var login = await HttpCallerDroppedDoaminsAdder.PostFormData("http://caught.co.uk/process.php", loginFormData);
            if (login.error != null)
            {
                mainForm.ErrorLog("login failed to caught.co.uk : " + login.error);
              
                return;
            }
            var backOrder = await HttpCallerDroppedDoaminsAdder.GetHtml($"http://caught.co.uk/order.php?order={domain}&submit=Backorder+Available+Domains");
            // backOrder response contains "Sorry could not add domains as you have exceeded your available credits." in case your available credits is less than recommended
            if (backOrder.error != null)
            {
                mainForm.ErrorLog($"backorder {domain} to caught.co.uk => " + backOrder.error);
               
                return;
            }
            mainForm.SuccessLog($"{domain} backordered successfully to caught.co.uk");
        }
    }
}
