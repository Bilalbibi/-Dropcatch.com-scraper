using System;
using System.Threading.Tasks;
using dropcatch.com.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace dropcatch.com.Websites
{
    public class Wizedrop : IWebsite
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
                driver.Navigate().GoToUrl("https://wizedrop.com/login");

                await Task.Delay(3000);
                driver.FindElementById("email").SendKeys("alex@lab41.co");
                driver.FindElementById("password").SendKeys("OtYA@Ty32*S0");
                driver.FindElementByXPath("//button[@type='submit']").Click();
            }
            catch (Exception)
            {
                driver.Quit();
                mainForm.ErrorLog($"error while connecting to wizedrop.com for backordering on {domain}");
                return;
            }

            //var amount = 49;


            driver.FindElement(By.XPath("//input[@type='text']")).SendKeys(domain);
            driver.FindElementByXPath("//input[@type='submit']").Click();
            await Task.Delay(1000);
            driver.FindElementByXPath("//input[@type='submit']").Click();
            await Task.Delay(1000);

            try
            {
                var domainAvailability = driver.FindElementById("name-success").Text;
                if (domainAvailability.Contains("Domain is not available"))
                {
                    mainForm.NormalLog($"{domain} is not available in wizedrop.com");
                    driver.Quit();
                    return;
                }
            }
            catch (Exception)
            {
                //
            }

            try
            {
                var checkIfWinningBid = driver.FindElement(By.XPath("//p[@class='clearfix your-offer-messages']//label[@class='text-success']")).GetAttribute("style");
                if (checkIfWinningBid.Equals("display: inline-block;"))
                {
                    mainForm.NormalLog($"we bided on {domain} in wizedrop.com and we have the winning bid for now");
                    driver.Quit();
                    return;
                }
            }
            catch (Exception)
            {
            }
            var checkDomainSatutsBid = driver.FindElementById("auction-status").Text.Trim();
            if (checkDomainSatutsBid.Contains("OPEN"))
            {
                driver.FindElementByXPath("//input[@name='amount']").SendKeys("49");
                driver.FindElementByXPath("//a[contains(@id,'place-bid')]").Click();
                await Task.Delay(3000);
                if (driver.PageSource.Contains("for this domain"))
                {
                    mainForm.ErrorLog($"the amount of bid on {domain} is more than $50 in wizedrop.com");
                    return;
                }

                mainForm.SuccessLog($"backorder {domain} success in wizedrop.com");
                driver.Quit();
                return;
            }
            mainForm.ErrorLog($"the biddung on {domain} is ended in wizedrop.com");
        }
    }
}
