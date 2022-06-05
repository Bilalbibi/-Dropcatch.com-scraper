using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using dropcatch.com.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace dropcatch.com.Websites
{
    public class catchdrop : IWebsite
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
                driver.Navigate().GoToUrl("https://catchdrop.uk/login.html");
                await Task.Delay(3000);
                driver.FindElementById("fu").SendKeys("alexanderr ");
                driver.FindElementById("fp").SendKeys("#j55oLZzhpSg");
                driver.FindElementById("loginbut").Click();
                await Task.Delay(3000);
                driver.FindElementById("domainSearchText").SendKeys(domain);
                driver.FindElementById("domainSearchButton").Click();
                await Task.Delay(3000);
                if (driver.PageSource.Contains("You have the highest bid"))
                {
                    mainForm.NormalLog($"{domain} already backordered in catchdrop.uk");
                    driver.Quit();
                    return;
                }
            }
            catch (Exception)
            {
                driver.Quit();
                mainForm.ErrorLog($"Error while connecting to catchdrop.uk for backorderring on {domain}");
               
            }

            var data_auth = "";
            var data_drop = "";
            var data_tag = "";
            var brutEndDate = "";
            var endDate = "";
            var bid = "";

            try
            {
                data_auth = driver.FindElementByXPath("//div[@data-auth]").GetAttribute("data-auth");
                data_drop = driver.FindElementByXPath("//div[@data-drop]").GetAttribute("data-drop");
                data_tag = driver.FindElementByXPath("//div[@data-tag]").GetAttribute("data-tag");
                bid = driver.FindElementByXPath("//input[@name='backorderBid']").GetAttribute("value");
                var element = driver.FindElement(By.XPath("//*[@id='domainAvailable']"));
                brutEndDate = (string)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].childNodes[2].textContent;", element);
                brutEndDate = brutEndDate.Replace("(", "").Replace(")", "").Trim();
                var daysString = brutEndDate.Split(' ');
                var addDays = double.Parse(daysString[0]);
                //endDate = DateTime.Parse(brutEndDate).ToString("dd/MM/yyyy");
                endDate = DateTime.Now.AddDays(addDays).ToString("dd/MM/yyyy");
                var evaluateBid = double.Parse(bid);
                if (evaluateBid > 50)
                {
                    mainForm.ErrorLog($"the price bid is more than $50 in catchdrop for {domain}");
                 
                    return;
                }
            }
            catch (Exception ex)
            {
                driver.Quit();
                mainForm.ErrorLog($"Eroor while getting data about {domain} from catchdrop.uk " + ex.ToString());
              
                return;
            }
            var allCookies = "";
            var cookies = driver.Manage().Cookies.AllCookies;

            foreach (var cookie in cookies)
            {
                allCookies = allCookies + cookie.Name + "=" + cookie.Value + ";";
            }
            allCookies = allCookies.Remove(allCookies.LastIndexOf(";"));

            driver.Quit();

            var formData = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("domain",domain),
                new KeyValuePair<string, string>("auth",data_auth),
                new KeyValuePair<string, string>("drop",data_drop),
                new KeyValuePair<string, string>("account","1584005218"),
                new KeyValuePair<string, string>("ror",""),
                new KeyValuePair<string, string>("tag",data_tag),
                new KeyValuePair<string, string>("bid",bid)
            };


            var backOrder = await HttpCallerDroppedDoaminsAdder.PostFormData1("https://catchdrop.uk/vis/placeAuctionBackorder.php", formData, allCookies);
            if (backOrder.error != null)
            {
                mainForm.ErrorLog($"backorder {domain} failed to catchdrop.uk => " + backOrder.error);
           
                return;
            }
            mainForm.SuccessLog($"{domain} backordered successfully to catchdrop.uk");
        }
    }
}
