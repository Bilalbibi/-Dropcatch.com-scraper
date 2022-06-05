using System.Collections.Generic;
using System.Threading.Tasks;
using dropcatch.com.Models;

namespace dropcatch.com.Websites
{
    public class Autobackorder :IWebsite
    {
        public HttpCallerDroppedDoaminsAdder HttpCallerDroppedDoaminsAdder = new HttpCallerDroppedDoaminsAdder();
        public MainForm mainForm = new MainForm();

        public async Task Bid(string domain)
        {
            

            var response = await HttpCallerDroppedDoaminsAdder.GetDoc("https://autobackorder.com/");
            if (response.error != null)
            {
                mainForm.ErrorLog($"connection to autobackorder.com failed for backordering on {domain} ==> " + response.error);
               
            }
            var _csrf_frontend = response.doc.DocumentNode.SelectSingleNode("//input[@name='_csrf-frontend']").GetAttributeValue("value", "");
            var formData = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("_csrf-frontend",_csrf_frontend),
                new KeyValuePair<string, string>("LoginForm[username]","AlexR "),
                new KeyValuePair<string, string>("LoginForm[password]","8paF9L!18y22"),
                new KeyValuePair<string, string>("login-button","")
            };
            var logIn = await HttpCallerDroppedDoaminsAdder.PostFormData("https://autobackorder.com/site/login", formData);
            if (logIn.error != null)
            {
                mainForm.ErrorLog($"login to autobackorder.com failed for backordering on {domain} ==> " + logIn.error);
               
            }

            var backOrder = await HttpCallerDroppedDoaminsAdder.GetDoc($"https://autobackorder.com/domain/index?UserDomain%5Bname%5D={domain}");
            //if balance is low or empty backOrder.outer html will contains "An error occured. Please contact us"
            if (backOrder.error != null)
            {
                mainForm.ErrorLog($"backorde {domain} to autobackorder.com failed ==> " + backOrder.error);
               
            }
            mainForm.SuccessLog($"backorder {domain} success in autobackorder.com");
          
        }

    }
}
