using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dropcatch.com.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace dropcatch.com.Services
{
    public class GoogleService
    {
        public ChromeDriver _driver;

        public void Init()
        {
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("headless");
            _driver = new ChromeDriver(service, options);

        }

        public void Dispose()
        {
            _driver?.Quit();
            _driver = null;
        }

        public async Task PopulateGoogleResult(List<Domain> domains)
        {
            Reporter.Log($"Start searching google for {domains.Count} domains");
            for (var i = 0; i < domains.Count; i++)
            {
                var domain = domains[i];
                Reporter.Progress((i + 1), domains.Count, "searching google ");
                try
                {
                    await Task.Run(() =>
                    {
                        Search(domain);
                    });
                }
                catch (Exception e)
                {
                    Reporter.Error($"Error searching google {domain.DomainName} : {e.Message}");
                }
            }
            Reporter.Log($"completed searching google for {domains.Count} domains");
        }

        public void Search(Domain domain)
        {
            _driver.Navigate().GoToUrl($"https://www.google.com/ncr");
            _driver.Navigate().GoToUrl($"https://www.google.com/search?q=site:{domain.DomainName}");
            int results = 0;
            try
            {
                var state = _driver.FindElementById("result-stats")?.Text;
                if (state.Contains("About"))
                {
                    var x1 = state.IndexOf("About", StringComparison.Ordinal) + "About".Length;
                    var x2 = state.IndexOf("result", x1, StringComparison.Ordinal);
                    var s = state.Substring(x1, x2 - x1).Replace(",", "").Trim();
                    results = int.Parse(s);
                }
                else
                {
                    var x2 = state.IndexOf("result", StringComparison.Ordinal);
                    var s = state.Substring(0, x2 ).Replace(",", "").Trim();
                    results = int.Parse(s);
                }
                domain.GoogleResults = results;
            }
            catch (Exception)
            {
                domain.GoogleResults = results;
            }
        }
    }
}
