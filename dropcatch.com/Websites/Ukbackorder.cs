using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using dropcatch.com.Models;
using OpenQA.Selenium.Chrome;

namespace dropcatch.com.Websites
{
    public class Ukbackorder : IWebsite
    {
        public HttpCallerDroppedDoaminsAdder HttpCallerDroppedDoaminsAdder = new HttpCallerDroppedDoaminsAdder();
        public MainForm mainForm = new MainForm();
        public async Task Bid(string domain)
        {

            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            var chromeOptions = new ChromeOptions();
            chromeDriverService.HideCommandPromptWindow = true;
            chromeOptions.AddArguments("headless");
            var driver = new ChromeDriver(chromeDriverService, chromeOptions);
            try
            {
                driver.Navigate().GoToUrl("https://ukbackorder.com/login.html");
                await Task.Delay(3000);
                driver.FindElementById("fu").SendKeys("Lab41");
                driver.FindElementById("fp").SendKeys("2pciULGtH0Ya");
                driver.FindElementById("loginbut").Click();
                await Task.Delay(3000);
                driver.FindElementById("domainSearchText").SendKeys(domain);
                driver.FindElementById("domainSearchButton").Click();
                await Task.Delay(3000);
                if (driver.PageSource.Contains("already backordered"))
                {
                    mainForm.NormalLog($"{domain} already backordered in ukbackorder.com");
                    driver.Quit();
                    return;
                }
            }
            catch (Exception)
            {
                driver.Quit();
                mainForm.ErrorLog("Error while connecting to ukbackorder.com");
                return;
            }
            var data_auth = "";
            var data_drop = "";
            var data_tag = "";
            var bid = "";
            try
            {
                var checkIfWeCanBachorderDomain = driver.PageSource;
                if (checkIfWeCanBachorderDomain.Contains("so cannot be backordered"))
                {
                    mainForm.ErrorLog($"This domain {domain} is not past its renewal date so cannot be backordered on ukbackorder.com");
                    return;
                }
                data_auth = driver.FindElementByXPath("//div[@data-auth]").GetAttribute("data-auth");
                data_drop = driver.FindElementByXPath("//div[@data-drop]").GetAttribute("data-drop");
                data_tag = driver.FindElementByXPath("//div[@data-tag]").GetAttribute("data-tag");
                bid = driver.FindElementByXPath("//input[@id='prepared']").GetAttribute("value");
                var evaluateBid = double.Parse(bid);
                if (evaluateBid > 50)
                {
                    mainForm.ErrorLog($"price bid is more than $50 in ukbackorder for {domain}");
                    return;
                }
            }
            catch (Exception ex)
            {
                driver.Quit();
                mainForm.ErrorLog($"Eroor while getting data about {domain} from ukbackorder.com ==> {ex.ToString()} ");
                return;
            }
            var allCookies = "";
            var cookies = driver.Manage().Cookies.AllCookies;

            foreach (var cookie in cookies)
            {
                allCookies = allCookies + cookie.Name + "=" + cookie.Value + ";";
            }
            allCookies = allCookies.Remove(allCookies.LastIndexOf(";", StringComparison.Ordinal));

            driver.Quit();

            var formData = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("domain",domain),
                new KeyValuePair<string, string>("auth",data_auth),
                new KeyValuePair<string, string>("drop",data_drop),
                new KeyValuePair<string, string>("account","1529744417"),
                new KeyValuePair<string, string>("ror",""),
                new KeyValuePair<string, string>("tag",data_tag),
                new KeyValuePair<string, string>("cost","1"),
                new KeyValuePair<string, string>("prepared",bid)
            };

            var backOrder = await HttpCallerDroppedDoaminsAdder.PostFormData1("https://ukbackorder.com/vis/backorderDomain.php", formData, allCookies);
            if (backOrder.error != null)
            {
                mainForm.ErrorLog($"backorder faild {domain} to ukbackorder.com => {backOrder.error}");
                return;
            }
            mainForm.SuccessLog($"{domain} backordered successfully to ukbackorder.com");
        }
    }
}
