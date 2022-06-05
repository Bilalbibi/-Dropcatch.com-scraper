using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dropcatch.com.Models;

namespace dropcatch.com.Services
{
    public static class UkdroplistsService
    {
        public static HttpCaller HttpCaller = new HttpCaller();
        public static async Task<List<DroppedDoamins>> GetDroppedDomainsFromUkdroplists()
        {
            var date = DateTime.Now;
            var ukdroplists = new List<DroppedDoamins>();
            var doc = await HttpCaller.GetDoc($@"https://ukdroplists.com/getDailyDroppingContents.php?ttype=nstats&searchtype=domain&dropdate=all&order=desc&start=0&amount=1000&field=mozDa&extra=&searchoption=begins&searchterm=&extrasearch=");
            var droppedDomains = doc.DocumentNode.SelectSingleNode("//script").InnerText.Trim();
            var x = droppedDomains.IndexOf("of", StringComparison.Ordinal) + 3;
            var xx = droppedDomains.IndexOf("domains", StringComparison.Ordinal);
            droppedDomains = droppedDomains.Substring(x, xx - x).Replace(",", "");

            var lenDomains = await GetLenDomains(droppedDomains);
            var moazDomains = await GetMoazDomains(droppedDomains);
            var majisticDomains = await GetMajisticDomains(droppedDomains);
            var cpcAndSearchDomains = await GetCpcAndSearchDomains(droppedDomains);

            if (moazDomains != null)
                ukdroplists.AddRange(moazDomains);

            if (majisticDomains != null)
                ukdroplists.AddRange(majisticDomains);

            if (cpcAndSearchDomains != null)
                ukdroplists.AddRange(cpcAndSearchDomains);

            if (lenDomains != null)
                ukdroplists.AddRange(lenDomains);

            return ukdroplists;
        }
        public static async Task<List<DroppedDoamins>> GetLenDomains(string droppedDomains)
        {
            var url = $@"https://ukdroplists.com/getDailyDroppingContents.php?ttype=nstats&searchtype=domain&dropdate=all&order=asc&start=0&amount={droppedDomains}&field=domainLength&extra=&searchoption=begins&searchterm=&extrasearch=";
            var doc = await HttpCaller.GetDoc(url);
            var basedLenDomains = new List<DroppedDoamins>();
            var trs = doc.DocumentNode.SelectNodes("//tr");
            var counter = 0;
            foreach (var tr in trs)
            {
                var domainName = tr.SelectSingleNode("./td[@class='sticky-col first-col']").InnerText.Trim();
                var len = int.Parse(tr.SelectSingleNode("./td[@data-col='len']").InnerText.Trim());
                if (len > 3 && counter == 0)
                    return null;
                if (len > 3 && counter > 0)
                    return basedLenDomains;
                if (domainName.EndsWith(".co.uk"))
                {
                    var regDateText = tr.SelectSingleNode("./td[@data-col='reg']").InnerText.Trim();
                    var regDate = new DateTime();

                    if (regDateText != "0")
                        regDate = new DateTime(int.Parse(regDateText), 1, 1);
                    var isDigitPresent = domainName.Any(char.IsDigit);
                    if (len == 3 && !isDigitPresent && !domainName.Contains("-"))
                    {
                        var domain = SaveLLLDoamins(regDateText, regDate);
                        domain.Name = domainName;
                        basedLenDomains.Add(domain);
                    }
                    if (len == 2)
                    {
                        var domain = SaveLLDoamins(regDateText, regDate);
                        domain.Name = domainName;
                        basedLenDomains.Add(domain);
                    }
                    if (len == 1)
                    {
                        var domain = SaveLDoamins(regDateText, regDate);
                        domain.Name = domainName;
                        basedLenDomains.Add(domain);
                    }
                }
                counter++;
            }

            return basedLenDomains;
        }

        private static DroppedDoamins SaveLDoamins(string regDateText, DateTime regDate)
        {
            var domain = new DroppedDoamins();
            domain.Type = "3";
            domain.Priority = "2";
            domain.Source = "UKDropLists.com: Dictionary Domain  ";

            var dateToCompare = new DateTime(int.Parse("2005"), 1, 1);

            if (regDateText != "0" && regDate < dateToCompare)
            {
                domain.Priority = "1";
            }

            return domain;
        }

        private static DroppedDoamins SaveLLDoamins(string regDateText, DateTime regDate)
        {
            var domain = new DroppedDoamins
            {
                Type = "3",
                Priority = "2",
                Source = "UKDropLists.com: Dictionary Domain (Numbers in Domain)"
            };

            var dateToCompare = new DateTime(int.Parse("2005"), 1, 1);

            if (regDateText != "0" && regDate < dateToCompare)
            {
                domain.Priority = "1";
            }

            return domain;
        }

        private static DroppedDoamins SaveLLLDoamins(string regDateText, DateTime regDate)
        {
            var domain = new DroppedDoamins
            {
                Type = "4",
                Priority = "2",
                Source = "UKDropLists.com: Dictionary Domain (Domain No Numbers or Hyphens!)"
            };

            var dateToCompare = new DateTime(int.Parse("2005"), 1, 1);

            if (regDateText != "0" && regDate < dateToCompare)
            {
                domain.Priority = "1";
            }

            return domain;
        }

        public static async Task<List<DroppedDoamins>> GetMajisticDomains(string droppedDomains)
        {
            var url = $@"https://ukdroplists.com/getDailyDroppingContents.php?ttype=nstats&searchtype=domain&dropdate=all&order=desc&start=0&amount={droppedDomains}&field=majTrustFlow&extra=&searchoption=begins&searchterm=&extrasearch=";
            var doc = await HttpCaller.GetDoc(url);
            var basedMajisticDomains = new List<DroppedDoamins>();
            var trs = doc.DocumentNode.SelectNodes("//tr");
            int count = 0;
            foreach (var tr in trs)
            {
                var domain = new DroppedDoamins();
                var domainName = tr.SelectSingleNode("./td[@class='sticky-col first-col']").InnerText.Trim();
                var majestic = int.Parse(tr.SelectSingleNode("./td[@data-col='mjtf']").InnerText.Trim());
                if (majestic < 30 && count == 0)
                {
                    return null;
                }
                if (majestic < 30 && count > 0)
                {
                    return basedMajisticDomains;
                }
                decimal da = 0;
                var daText = tr.SelectSingleNode("./td[@data-col='mzda']")?.InnerText.Trim();
                if (!string.IsNullOrEmpty(daText))
                {
                    da = decimal.Parse(daText);
                    domain.Da = da;
                }

                decimal pa = 0;
                var paText = tr.SelectSingleNode("./td[@data-col='mzpa']")?.InnerText.Trim();
                if (!string.IsNullOrEmpty(paText))
                {
                    pa = decimal.Parse(daText);
                }
                int freq = domainName.Count(x => x == '.');
                if (domainName.Contains(".co.uk") || (domainName.Contains(".uk") && freq.Equals(1)))
                {
                    if (da > pa && majestic >= 30)
                    {

                        domain.Name = domainName;
                        domain.Priority = "3";
                        domain.Type = "1";
                        domain.Source = "UKDropLists.com: MJTF > 30";
                        basedMajisticDomains.Add(domain);
                        count++;
                    }
                }
            }

            return basedMajisticDomains;
        }

        public static async Task<List<DroppedDoamins>> GetMoazDomains(string droppedDomains)
        {
            var url = $@"https://ukdroplists.com/getDailyDroppingContents.php?ttype=nstats&searchtype=domain&dropdate=all&order=desc&start=0&amount={droppedDomains}&field=mozDa&extra=&searchoption=begins&searchterm=&extrasearch=";
            var doc = await HttpCaller.GetDoc(url);
            var basedMoazDomains = new List<DroppedDoamins>();
            var trs = doc.DocumentNode.SelectNodes("//tr");
            var count = 0;
            foreach (var tr in trs)
            {
                var domainName = tr.SelectSingleNode("./td[@class='sticky-col first-col']").InnerText.Trim();
                var da = decimal.Parse(tr.SelectSingleNode("./td[@data-col='mzda']").InnerText.Trim());
                var pa = int.Parse(tr.SelectSingleNode("./td[@data-col='mzpa']").InnerText.Trim());
                if (da < 35 && count == 0)
                {
                    return null;
                }
                if (da < 35 && count > 0)
                {
                    return basedMoazDomains;
                }
                int freq = domainName.Count(x => x == '.');
                if (domainName.Contains(".co.uk") || (domainName.Contains(".uk") && freq.Equals(1)))
                {
                    if (da >= 50 && da > pa)
                    {
                        var domains = new DroppedDoamins();
                        domains.Name = domainName;
                        domains.Da = da;
                        domains.Priority = "1";
                        domains.Type = "1";
                        domains.Source = "UKDropLists.com: SEO Domain (Moz > 50)";
                        basedMoazDomains.Add(domains);
                        count++;
                    }

                    if (da < 50 && da >= 35 && da > pa)
                    {
                        var domains = new DroppedDoamins();
                        domains.Da = da;
                        domains.Name = domainName;
                        domains.Priority = "2";
                        domains.Type = "1";
                        domains.Source = "UKDropLists.com: SEO Domain (Moz > 35 , Moz < 50)";
                        basedMoazDomains.Add(domains);
                        count++;
                    }
                }
            }

            return basedMoazDomains;
        }

        public static async Task<List<DroppedDoamins>> GetCpcAndSearchDomains(string droppedDomains)
        {
            var url = $@"https://ukdroplists.com/getDailyDroppingContents.php?ttype=nstats&searchtype=domain&dropdate=all&order=desc&start=0&amount={droppedDomains}&field=cpc&extra=&searchoption=begins&searchterm=&extrasearch=";
            var doc = await HttpCaller.GetDoc(url);
            var basedcpcAndSearchDomain = new List<DroppedDoamins>();
            var trs = doc.DocumentNode.SelectNodes("//tr");
            var count = 0;
            foreach (var tr in trs)
            {
                var domainName = tr.SelectSingleNode("./td[@class='sticky-col first-col']").InnerText.Trim();
                var daT = tr.SelectSingleNode("./td[@data-col='mzda']")?.InnerText.Trim() ?? "0";
                decimal da = 0;
                var cpc = double.Parse(tr.SelectSingleNode("./td[@data-col='cpc']").InnerText.Trim());
                if (cpc < 20 && count == 0)
                {
                    return null;
                }
                if (cpc < 20 && count > 0)
                {
                    return basedcpcAndSearchDomain;
                }
                var search = int.Parse(tr.SelectSingleNode("./td[@data-col='srch']").InnerText.Trim());
                var regDateText = tr.SelectSingleNode("./td[@data-col='reg']").InnerText.Trim();
                var regDate = new DateTime();
                var dateToCompare = new DateTime(int.Parse("2005"), 1, 1);
                if (regDateText != "0")
                {
                    regDate = new DateTime(int.Parse(regDateText), 1, 1);
                }
                int freq = domainName.Count(x => x == '.');
                if (!string.IsNullOrEmpty(daT))
                {
                    da = decimal.Parse(tr.SelectSingleNode("./td[@data-col='mzda']")?.InnerText.Trim() ?? "0");
                }
                if (domainName.Contains(".co.uk") || (domainName.Contains(".uk") && freq.Equals(1)))
                {
                    var domain = new DroppedDoamins();
                    domain.Da = da;
                    if (cpc >= 20 && search >= 500)
                    {
                        domain.Name = domainName;
                        domain.Priority = "2";
                        domain.Type = "1";
                        domain.Source = "UKDropLists.com: cpc > 20 and search > 500";
                    }

                    if (regDateText != "0" && regDate < dateToCompare)
                    {
                        domain.Priority = "1";
                    }

                    basedcpcAndSearchDomain.Add(domain);
                    count++;

                }
            }

            return basedcpcAndSearchDomain;
        }
    }
}