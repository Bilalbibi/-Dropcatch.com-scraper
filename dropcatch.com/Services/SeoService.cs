using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using dropcatch.com.Models;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Chrome;

namespace dropcatch.com.Services
{
    public static class SeoService
    {
        public static HttpCaller HttpCaller = new HttpCaller();
        public static ChromeDriver _driver;

        public static void Init()
        {
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("headless");
            _driver = new ChromeDriver(service, options);

        }
        public static async Task PopulateTfAndCfData()
        {
            var tpl = new TransformBlock<Domain, (double tf, double cf, Domain domain)>(async x => await GetTfAndCfData(x).ConfigureAwait(false),
               new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 40 });

            foreach (var domain in AppStates.Domains)
                tpl.Post(domain.Value);

            foreach (var domainKeyValue in AppStates.Domains)
            {
                var (tf, cf, domain) = await tpl.ReceiveAsync().ConfigureAwait(false);
                domain.Tf = tf;
                domain.Cf = cf;
            }
        }

        private static async Task<(double tf, double cf, Domain domain)> GetTfAndCfData(Domain domain)
        {
            do
            {
                var html = await HttpCaller.GetHtml($"https://seo-rank.my-addr.com/api3/F1EF5461AEE11BE918A459EA5204150F/{domain.DomainName}");
                if (!html.Contains("status"))
                {
                    await Task.Delay(1000 * 60 * 2);
                    continue;
                }
                var obj = JObject.Parse(html);
                var status = (string)obj.SelectToken("status");
                if (status != "Found")
                    return (0, 0, domain);

                var tf = (double)obj.SelectToken("tf");
                var cf = (double)obj.SelectToken("cf");
                return (tf, cf, domain);
            } while (true);
        }
        public static async Task PopulateDaStatus(Domain domain)
        {
            var json = await HttpCaller.GetHtml($"https://seo-rank.my-addr.com/api2/+moz/F1EF5461AEE11BE918A459EA5204150F/{domain.DomainName}");
            var obj = JObject.Parse(json);
            domain.Da = (decimal)obj.SelectToken("da");
            domain.Pa = (string)obj.SelectToken("pa");
            domain.Links = (int)obj.SelectToken("links");
            domain.Equity = (string)obj.SelectToken("equity");
        }
        public static async Task<(string currentPrice, string error)> GetCurrentPrice(string domain)
        {
            var html = await HttpCaller.GetHtml($"https://client.dropcatch.com/GetDomainDetail?DomainName={domain}");
            var obj = JObject.Parse(html);
            var currentPrice = (string)obj.SelectToken("..item.highBid");
            return (currentPrice, null);
        }

        public static async Task<List<Domain>> AhrefData( List<Domain> domains)
        {
            Reporter.Log("Getting Ahref data");
            var urls = domains.Select(x => x.DomainName).ToList();
            File.WriteAllLines("Domains.txt", urls);
            _driver.Navigate().GoToUrl("https://seo-rank.my-addr.com/login_short.php?logru=https%3A%2F%2Fseo-rank.my-addr.com%2F");
            _driver.FindElementByName("username").SendKeys("Lab41");
            _driver.FindElementByName("password").SendKeys("t53sV8Mo2MJe");
            _driver.FindElementByXPath("//input[@type='image']").Click();
            await Task.Delay(2000);
            _driver.FindElementById("b_ahrefs").Click();
            string fileName = "Domains.txt";
            FileInfo f = new FileInfo(fileName);
            string fullname = f.FullName;
            _driver.FindElementByXPath("//input[@name='file1']").SendKeys(fullname);
            _driver.FindElementByXPath("//input[@type='image']").Click();
            File.Delete("Domains.txt");
            await Task.Delay(10000);
            do
            {
                _driver.Navigate().Refresh();
                var status = _driver.FindElementByXPath("//table[@class='price_table']").Text;
                if (!status.Contains("finished"))
                {
                    var processedDomains =int.Parse( _driver.FindElementByXPath("//table[@class='price_table']/tbody/tr[2]/td[4]").Text);
                    Reporter.Progress(processedDomains, urls.Count, "Doamins processed for Ahref data");
                    await Task.Delay(2000);
                    continue;
                }
                break;
            } while (true);

            var fileId = _driver.FindElementByXPath("//table[@class='price_table']//tr[2]/td[1]").Text;
            var cookiesList = _driver.Manage().Cookies.AllCookies.ToList();
            var allCookie = new StringBuilder();
            foreach (var cookie in cookiesList)
            {
                allCookie.Append(cookie.Name + "=" + cookie.Value + ";");
            }
            var Cookies = allCookie.ToString().Remove(allCookie.ToString().Length - 1);
            _driver.Quit();
            var stream = await HttpCaller.GetStream($"https://seo-rank.my-addr.com/download_results.php?i={fileId}", Cookies);
            using (var fileStream = File.Create("Ahref.csv"))
                await stream.CopyToAsync(fileStream);

            var lines = File.ReadAllLines("Ahref.csv").ToList();
            lines.RemoveAt(0);
            foreach (var line in lines)
            {
                var datas = line.Split(',');
                if (datas[2].Contains("found"))
                {
                    var domain = domains.Find(x => x.DomainName == datas[0].Replace("\"", ""));
                    domain.AhrefsRank = datas[3].Replace("\"", ""); 
                    domain.AhrefsDR = datas[4].Replace("\"", ""); 
                    domain.AhrefsEB = datas[5].Replace("\"", ""); 
                    domain.AhrefsRD = datas[6].Replace("\"", ""); 
                }
            }
            File.Delete("Ahref.csv");
            var fromData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("do","delete_file"),
                new KeyValuePair<string, string>("item_id",fileId)
            };
            await HttpCaller.DeletFileFromWebsite("https://seo-rank.my-addr.com/dashboard.php?p=5256", fromData, Cookies);
            return domains;
        }
    }
}
