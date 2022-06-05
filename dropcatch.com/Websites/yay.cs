using System.Collections.Generic;
using System.Threading.Tasks;
using dropcatch.com.Models;

namespace dropcatch.com.Websites
{
    public class yay : IWebsite
    {
        public HttpCallerDroppedDoaminsAdder HttpCallerDroppedDoaminsAdder = new HttpCallerDroppedDoaminsAdder();
        public MainForm mainForm = new MainForm();
        public async Task Bid(string domain)
        {

            var GetLoginToken = await HttpCallerDroppedDoaminsAdder.GetDoc("https://www.yay.com/login/");
            if (GetLoginToken.error != null)
            {
                mainForm.ErrorLog($"Couldn't access to yay.com login page ==> {GetLoginToken.error}");
                return;
            }
            var token = GetLoginToken.doc.DocumentNode.SelectSingleNode("//input[@id='login_token']").GetAttributeValue("value", "").Trim();
            string formData = "{\"email\":\"alex@lab41.co\",\"password\":\"88tjpqN&$cu9\",\"token\":" + "\"" + token + "\"" + ",\"leg\":1,\"code\":\"\",\"migrating\":false}";

            var login = await HttpCallerDroppedDoaminsAdder.PostJson("https://www.yay.com/svc/account/login", formData);
            if (login.error != null)
            {
                return;
            }
            //string domain = "lescaves.co.uk";
            string jsonFormat = "{\"item_type\":54,\"item_sub_type\":1,\"item_data\":{\"domain_name\":" + "\"" + domain + "\"" + ",\"tld\":\"co.uk\",\"period\":1,\"action\":18,\"premium\":0,\"premiumPhase\":\"\"}}";

            var insertToBasket = await HttpCallerDroppedDoaminsAdder.PostJson1("https://www.yay.com/svc/cart/item", jsonFormat, login.cookie);
            if (insertToBasket.error != null)
            {
                mainForm.ErrorLog($"insert {domain} in Basket failed for yay.com ==> {insertToBasket.error}");
                return;
            }
            var GetFormDataForBuyingDomain = await HttpCallerDroppedDoaminsAdder.GetDocToExtractFormdatForBuyingDomain("https://www.yay.com/shopping-cart/", insertToBasket.cookie);
            if (GetFormDataForBuyingDomain.error != null)
            {
                mainForm.ErrorLog($"We couldn't access to the buying domain tab in yay.com => {GetFormDataForBuyingDomain.error}");
                return;
            }
            GetFormDataForBuyingDomain.doc.Save("yay.html");

            var checkout_token = GetFormDataForBuyingDomain.doc.DocumentNode.SelectSingleNode("//input[@id='p_c_token']").GetAttributeValue("value", "").Trim();
            var formDataForBackOrder = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("checkout-token",checkout_token),
                new KeyValuePair<string, string>("p_type","1"),
                new KeyValuePair<string, string>("p_existing_id","105759"),
                new KeyValuePair<string, string>("p_stripe_token","0"),
                new KeyValuePair<string, string>("p_address_id","122835"),
                new KeyValuePair<string, string>("p_address_org",""),
                new KeyValuePair<string, string>("p_address_a1","120 High Road"),
                new KeyValuePair<string, string>("p_address_a2",""),
                new KeyValuePair<string, string>("p_address_city",""),
                new KeyValuePair<string, string>("p_address_state",""),
                new KeyValuePair<string, string>("p_address_zip","N2 9ED"),
                new KeyValuePair<string, string>("p_address_cc",""),
                new KeyValuePair<string, string>("p_address_phone",""),
            };

            var backOrder = await HttpCallerDroppedDoaminsAdder.PostFormData1("https://www.yay.com/shopping-cart/", formDataForBackOrder, GetFormDataForBuyingDomain.cookie);
            if (backOrder.error != null)
            {
                mainForm.ErrorLog($"backorder faild for {domain} in yay.com => {backOrder.error}");
                return;
            }
            await Task.Delay(5000);
            var checkIfordred = await HttpCallerDroppedDoaminsAdder.GetHtml2("https://www.yay.com/service/dom/back-order", GetFormDataForBuyingDomain.cookie);
            if (checkIfordred.error != null)
            {
                mainForm.ErrorLog("couldn't check if doamin is taken or not on Yay.com" + checkIfordred.error);
            }
            if (!checkIfordred.html.Contains(domain))
            {
                //var sndAlert = await EmailNote($"{domain.DomainName} is taken by a competitor on yay.com");
                //return;
                mainForm.ErrorLog($"{domain} is taken by a competitor in yay.com");
                return;
            }

            mainForm.SuccessLog($"{domain} backordered successfully to yay.com");
        }
    }
}
