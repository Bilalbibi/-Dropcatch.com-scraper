using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using dropcatch.com.Models;
using Newtonsoft.Json.Linq;

namespace dropcatch.com.Services
{
    public static class DomainloreService
    {
        public static HttpCaller HttpCaller = new HttpCaller();
        public static async Task<List<DroppedDoamins>> GetDroppedDomainsFromDomainlore()
        {
            var droppedDomainsFromDomainlore = new List<DroppedDoamins>();

            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("email","alex@lab41.co"),
                new KeyValuePair<string, string>("password","2Dl!XCGSa$8!"),
                new KeyValuePair<string, string>("return_to",""),
                new KeyValuePair<string, string>("submit","Log in")
            };
            await HttpCaller.PostFormData("https://domainlore.uk/member/login", formData);

            var stream = await HttpCaller.GetStream($"https://domainlore.uk/droplist/txt/7");
            using (var fileStream = File.Create("domains.txt"))
                await stream.CopyToAsync(fileStream);

            var doaminsNbr = File.ReadAllLines("domains.txt").ToList().Count;
            var counter = 1;
            var scrapedDomains = 0;
            do
            {
                var json = await HttpCaller.GetHtml($"https://domainlore.uk/droplist/json/7/{counter}");
                var jObject = JObject.Parse(json);
                var array = (JArray)jObject.SelectToken("droplist");

                foreach (var droppedDomain in jObject["droplist"])
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    var obj = JObject.Parse(((string)droppedDomain.SelectToken("dt"))?.Replace("\"", "\""));
                    var domainName = (string)droppedDomain.SelectToken("d");
                    var domainType = (string)droppedDomain.SelectToken("c");
                    int freq = domainName.Count(x => x == '.');
                    var isDigitPresent = domainName.Any(c => char.IsDigit(c));
                    var sersearch = 0;

                    var domain = new DroppedDoamins();
                    var regDate = new DateTime(int.Parse((string)droppedDomain.SelectToken("rd") ?? string.Empty), 1, 1);
                    var year2005 = new DateTime(int.Parse("2005"), 1, 1);
                    var secondeDateToCompare = new DateTime(int.Parse("2015"), 1, 1);
                    try
                    {
                        sersearch = (int)obj.SelectToken("localexact");
                    }
                    catch (Exception)
                    {
                        //
                    }
                    if (domainType.Equals("0"))
                    {
                        if (regDate < year2005 && sersearch > 5000)
                        {
                            domain.Name = domainName;
                            domain.Type = "5";
                            domain.Priority = "1";
                            domain.Source = "DomainLore.uk: (Old domain) Pre 2005 + Local Search > 5 000";
                        }
                        if (regDate > year2005 && regDate < secondeDateToCompare && sersearch > 10000)
                        {
                            domain.Name = domainName;
                            domain.Type = "5";
                            domain.Priority = "1";
                            domain.Source = "DomainLore.uk: (Mid Age Domains) After 2005 But Before 2015 + Local Search > 10,000";
                        }
                    }

                    if (domainType.Equals("1"))
                    {
                        if (domainName.EndsWith(".co.uk"))
                        {
                            var lengthDoamin = (string)obj.SelectToken("phrase");
                            var year2015 = new DateTime(int.Parse("2015"), 1, 1);
                            if (regDate < year2005)
                            {
                                domain.Name = domainName;
                                var da = await GetDaStatus(domainName);
                                domain.Da = da;
                                domain.Priority = "1";
                                domain.Source = "DomainLore.uk: Dictionary Domain, Pre 2005";
                                if (lengthDoamin.Length == 3)
                                    domain.Type = "4";
                                if (lengthDoamin.Length == 2)
                                    domain.Type = "3";
                                if (lengthDoamin.Length == 1)
                                    domain.Type = "3";
                            }
                            else if (regDate > year2005 && regDate < year2015 && lengthDoamin.Length == 2)
                            {
                                domain.Name = domainName;
                                var da = await GetDaStatus(domainName);
                                domain.Da = da;
                                domain.Priority = "2";
                                domain.Type = "3";
                                domain.Source = "DomainLore.uk: Dictionary Domain, Register date between 2005 and 2015";
                                if (da > 30)
                                {
                                    domain.Priority = "1";
                                    domain.Source = "DomainLore.uk: Dictionary Domain, Register date between 2005 and 2015,  SEO Domain (Moz > 30)";
                                }

                            }
                            else
                            {
                                if (regDate < year2015 && !isDigitPresent && !domainName.Contains("-"))
                                {
                                    domain.Name = domainName;
                                    var da = await GetDaStatus(domainName);
                                    domain.Da = da;
                                    domain.Priority = "2";
                                    domain.Source = "DomainLore.uk: Dictionary Domain, Pre 2015, domain don't contains Hyphens";
                                    if (lengthDoamin.Length == 3)
                                        domain.Type = "4";
                                    if (lengthDoamin.Length == 2)
                                        domain.Type = "3";
                                    if (lengthDoamin.Length == 1)
                                        domain.Type = "3";
                                }
                            }
                        }
                    }
                    if (domainType.Equals("-1"))
                    {
                        var cpc = 0.00;
                        var year2010 = new DateTime(int.Parse("2010"), 1, 1);
                        try
                        {
                            cpc = (double)obj.SelectToken("competition");
                        }
                        catch (Exception)
                        {
                            //
                        }
                        if (regDate < year2010 && sersearch > 10000 && cpc > 0.50)
                        {
                            if (!domainName.Contains(".org"))
                            {

                                domain.Name = domainName;
                                domain.Type = "5";
                                domain.Priority = "2";
                                domain.Source = "DomainLore.uk: Pre 2010 + Local Search > 10,000 + CPC > 0.50";
                            }
                            else
                            {
                                var da = await GetDaStatus(domainName);
                                domain.Da = da;
                                domain.Name = domainName;
                                domain.Type = "5";
                                domain.Priority = "2";
                                domain.Source = "DomainLore.uk: Pre 2010 + Local Search > 10,000 + CPC > 0.50 and SEO Domain (Moz > 30)";
                                if (da > 30)
                                {
                                    domain.Priority = "2";
                                    droppedDomainsFromDomainlore.Add(domain);
                                }

                            }
                        }
                    }
                    if (domain.Name != null)
                    {
                        if (!domain.Name.Contains(".org"))
                        {
                            droppedDomainsFromDomainlore.Add(domain);
                        }
                    }
                }
                scrapedDomains = scrapedDomains + array.Count;
                if (scrapedDomains == doaminsNbr)
                {
                    break;
                }
                counter++;
            } while (true);
            File.Delete("domains.txt");
            return droppedDomainsFromDomainlore;
        }
        public static async Task<decimal> GetDaStatus(string domainName)
        {
            var json = await HttpCaller.GetHtml($"https://seo-rank.my-addr.com/api2/+moz/F1EF5461AEE11BE918A459EA5204150F/{domainName}");
            var obj = JObject.Parse(json);
            var da = (decimal)obj.SelectToken("da");
            return da;
        }
    }
}