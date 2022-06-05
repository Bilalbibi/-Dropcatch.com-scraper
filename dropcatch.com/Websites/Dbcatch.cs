using System.Collections.Generic;
using System.Threading.Tasks;
using dropcatch.com.Models;

namespace dropcatch.com.Websites
{
    public class Dbcatch : IWebsite
    {
        public HttpCallerDroppedDoaminsAdder HttpCallerDroppedDoaminsAdder = new HttpCallerDroppedDoaminsAdder();
        public MainForm mainForm = new MainForm();
        public async Task Bid(string domain)
        {
            var formData = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("username","Lab41"),
                new KeyValuePair<string, string>("password","af8f9fff17c1"),
                new KeyValuePair<string, string>("submit","Login"),
                new KeyValuePair<string, string>("doLogin","1")
            };
            var logIn = await HttpCallerDroppedDoaminsAdder.PostFormData("https://www.dbcatch.co.uk/login.php", formData);
            if (logIn.error != null)
            {
                return;
            }
            formData = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("domains",domain),
                new KeyValuePair<string, string>("doupdate","Check Domain Status")
            };
            var getDropDAte = await HttpCallerDroppedDoaminsAdder.PostFormData("https://www.dbcatch.co.uk/book_hosted.php", formData);

            if (getDropDAte.error != null)
            {
                return;
            }
            if (getDropDAte.html.Contains("Not Suspended"))
            {
               //
            }
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(getDropDAte.html);// hibu.co.uk exemple domain
            var dropDate = doc.DocumentNode.SelectSingleNode($"//td[text()='{domain}']/following-sibling::td[1]").InnerText.Trim();

            formData = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("booknames[]",domain+","+dropDate),
                new KeyValuePair<string, string>("bookdomains","Add Domains")
            };
            var bachOrderToBookname_hosted = await HttpCallerDroppedDoaminsAdder.PostFormData("https://www.dbcatch.co.uk/bookname_hosted.php", formData);
            if (bachOrderToBookname_hosted.error != null)
            {
                
            }
            formData = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("booknames[]",domain+","+dropDate),
                new KeyValuePair<string, string>("priority[]",domain+",30"),
                new KeyValuePair<string, string>("bookdomains","Book Domains")
            };
            var bachOrderToBookname = await HttpCallerDroppedDoaminsAdder.PostFormData("https://www.dbcatch.co.uk/bookname.php", formData);
            if (bachOrderToBookname.error != null)
            {
                mainForm.ErrorLog($"backorder {domain} to https://www.dbcatch.co.uk/book.php failed ==> {bachOrderToBookname.error} ");
                return;
            }

            mainForm.SuccessLog($"backordering {domain} to dbcatch.co.uk/bookname_hosted success");
         
        }
    }
}
