using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dropcatch.com.Models;
using Newtonsoft.Json.Linq;

namespace dropcatch.com.Services
{
    public class DropcatchService
    {
        public HttpCaller HttpCaller = new HttpCaller();
        /// <summary>
        /// Get all dropped domains from dropcatcher website and get their basic info
        /// </summary>
        /// <returns></returns>
        public async Task<List<Domain>> GetDroppedDomains()
        {
            Reporter.Log("Start getting dropped domains");
            var dateNow = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
            var filter = "{\"searchTerm\":\"\",\"filters\":[{\"values\":[{\"Range\":{\"Min\":\"" + dateNow + "\",\"Max\":null}}],\"Name\":\"ExpirationDate\"},{\"values\":[{\"Value\":\"Dropped\"}],\"Name\":\"RecordType\"}]}";
            var json = await HttpCaller.PostJson("https://client.dropcatch.com/Filters", filter);
            var obj = JObject.Parse(json);
            var droppedDomainsCount = (string)obj.SelectToken("..totalDocuments");
            var search = "{\"searchTerm\":\"\",\"filters\":[{\"values\":[{\"Range\":{\"Min\":\"" + dateNow + "\",\"Max\":null}}],\"Name\":\"ExpirationDate\"},{\"values\":[{\"Value\":\"Dropped\"}],\"Name\":\"RecordType\"}],\"page\":1,\"size\":" + droppedDomainsCount + ",\"sorts\":[{\"field\":\"highBid\",\"direction\":\"Descending\"}]}";
            json = await HttpCaller.PostJson("https://client.dropcatch.com/Search", search);

            obj = JObject.Parse(json);
            var domains = new List<Domain>();
            var items = (JArray)obj.SelectToken("result.items");
            Reporter.Log($"dropped domains detected {items.Count}");
            for (var i = 0; i < items.Count; i++)
            {
                Reporter.Progress((i + 1), items.Count, "populating Da status of ");
                var item = items[i];
                var domainName = (string)item.SelectToken("name");
                //var registeredDate = WhoIsService.GetRegisteredDate(domainName);
                var domain = new Domain { DomainName = domainName };
                await SeoService.PopulateDaStatus(domain);
                await WhoIsService.GetRegisteredDate(domain);
                domain.DomainId = (string)item.SelectToken("id");
                domain.NumberOfBids = (string)item.SelectToken("numberOfBids");
                domain.EndDate = (DateTime)item.SelectToken("expirationDate");
                domains.Add(domain);
            }
            Reporter.Log("complete getting dropped domains");
            return domains;
        }

        /// <summary>
        /// Get all bidders (username and amount) on a specific domain
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public async Task GetDomainBids(Domain domain)
        {
            //set old bids as not new
            foreach (var domainBid in domain.Bids)
            {
                domainBid.IsNew = false;
                domainBid.IsHighestBid = false;
            }

            var json = await HttpCaller.GetHtml($"https://client.dropcatch.com/GetAuctionBidHistory?name={domain.DomainName}&Id={domain.DomainId}&PageSize={domain.NumberOfBids}");
            var obj = JObject.Parse(json);
            var domainToCheck = new DomainToCheck();
            var items = (JArray)obj.SelectToken("result.items");
            foreach (var item in items)
            {
                var userName = (string)item.SelectToken("alias");
                var amount = (decimal)item.SelectToken("amount");
                //var isHighBid = (bool)item.SelectToken("isHighBid");

                //see if we already have that bid
                var existedBid = domain.Bids.FirstOrDefault(x => x.UserName.Equals(userName) && x.Amount.Equals(amount));
                if (existedBid == null)
                    domain.Bids.Add(new Bid { UserName = userName, Amount = amount, IsNew = true, Domain = domain });
            }

            //set the max bid by user
            var bidsByUser = domain.Bids.DistinctBy(x => x.UserName);
            foreach (var bid in bidsByUser)
            {
                var maxBidByUser = domain.Bids.Where(x => x.UserName.Equals(bid.UserName)).OrderByDescending(x => x.Amount).FirstOrDefault();
                if (maxBidByUser == null) continue;
                maxBidByUser.IsHighestBid = true;
            }
        }
        /// <summary>
        /// Collect all bidders for all the domains we have
        /// </summary>
        /// <returns></returns>
        public async Task GetDomainBidsForAll(List<Domain> domains)
        {
            Reporter.Log($"Getting bids for {domains.Count} domains");
            var i = 1;
            foreach (var domain in domains)
            {
                Reporter.Progress(i, AppStates.Domains.Count, "Getting domain for ");
                try
                {
                    await GetDomainBids(domain);
                }
                catch (Exception e)
                {
                    Reporter.Error($"Failed to get Bids on domain {domain.DomainName} : {e.Message}");
                }
                i++;
            }
            Reporter.Log($"Getting bids completed");
        }
    }
}