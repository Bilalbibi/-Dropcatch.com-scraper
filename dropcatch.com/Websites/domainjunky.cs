using System.Collections.Generic;
using System.Threading.Tasks;
using dropcatch.com.Models;

namespace dropcatch.com.Websites
{
    public class domainjunky : IWebsite
    {
        public HttpCallerDroppedDoaminsAdder HttpCallerDroppedDoaminsAdder = new HttpCallerDroppedDoaminsAdder();
        public MainForm mainForm = new MainForm();
        public async Task Bid(string domain)
        {
           

            var getloginFormData = await HttpCallerDroppedDoaminsAdder.GetDoc("https://www.domainjunky.co.uk/");
            if (getloginFormData.error != null)
            {
                mainForm.ErrorLog($"connecting failed to domainjunky.co.uk => { getloginFormData.error}");
                return;
            }
            var form_build_id = getloginFormData.doc.DocumentNode.SelectSingleNode("//input[@name='form_build_id']").GetAttributeValue("value", "").Trim();
            //Console.WriteLine(form_build_id);
            var loginFormData = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("name","Alex Read"),
                new KeyValuePair<string, string>("pass","cqVmpPIM78%x"),
                new KeyValuePair<string, string>("form_build_id",form_build_id),
                new KeyValuePair<string, string>("form_id","user_login_block"),
                new KeyValuePair<string, string>("opp","Log in")
            };

            var login = await HttpCallerDroppedDoaminsAdder.PostFormData("https://www.domainjunky.co.uk/node?destination=node", loginFormData);
            if (login.error != null)
            {
                mainForm.ErrorLog($"login failed to domainjunky.co.uk => {login.error}");
                return;
            }
            var bookPage = await HttpCallerDroppedDoaminsAdder.GetDoc("https://www.domainjunky.co.uk/book");
            if (bookPage.error != null)
            {
                mainForm.ErrorLog($"couldn't access to Book Drop Catch page => {login.error}");
                return;
            }

            form_build_id = bookPage.doc.DocumentNode.SelectSingleNode("//input[@name='form_build_id']").GetAttributeValue("value", "").Trim();
            var form_token = bookPage.doc.DocumentNode.SelectSingleNode("//input[@name='form_token']").GetAttributeValue("value", "").Trim();

            var BackOrderFormData = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("catchlist",domain),
                new KeyValuePair<string, string>("op","Request Catch"),
                new KeyValuePair<string, string>("form_build_id",form_build_id),
                new KeyValuePair<string, string>("form_token",form_token),
                new KeyValuePair<string, string>("form_id","domains_book_form")
            };
            var backOrder = await HttpCallerDroppedDoaminsAdder.PostFormData("https://www.domainjunky.co.uk/book", BackOrderFormData);
            if (backOrder.error != null)
            {
                mainForm.ErrorLog($"backorder {domain} to domainjunky.co.uk => " + backOrder.error);
                return;
            }
            mainForm.SuccessLog($"{domain} backordered successfully to domainjunky.co.uk");
        }
    }
}
