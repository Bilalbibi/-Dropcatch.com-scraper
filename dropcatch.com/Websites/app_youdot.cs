using System.Threading.Tasks;
using dropcatch.com.Models;
using Newtonsoft.Json.Linq;

namespace dropcatch.com.Websites
{
    public class app_youdot : IWebsite
    {
        public HttpCallerDroppedDoaminsAdder HttpCallerDroppedDoaminsAdder = new HttpCallerDroppedDoaminsAdder();
        public MainForm mainForm = new MainForm();
       
       
        public async Task Bid(string domain)
        {
            
            var logInData = "{\"email\":\"alex@lab41.co\",\"password\":\"4C&d2V@zy@by\"}";

            var logIn = await HttpCallerDroppedDoaminsAdder.PostJson("https://truffade.youdot.io/login", logInData);
            if (logIn.error != null)
            {
                mainForm.ErrorLog($"login to app.youdot.io failed for backordering on {domain} ==> {logIn.error} ");
                return;
            }
            var Object = JObject.Parse(logIn.json);
            var token = (string)Object.SelectToken("token");

            var backOrderData = "{\"domains\":[\"" + domain + "\"]}";
            var backOrder = await HttpCallerDroppedDoaminsAdder.PostJson1("https://truffade.youdot.io/backorder", backOrderData, token);
            if (backOrder.error != null)
            {
                mainForm.ErrorLog($"backordering on  {domain } is failed in app.youdot.io failed ==> " + logIn.error);
              
            }
          
        }
    }
}
