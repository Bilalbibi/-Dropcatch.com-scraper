using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using dropcatch.com.Models;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;

namespace dropcatch.com.Services
{
    public class AutoBidService
    {
        private HttpCaller _httpCaller = new HttpCaller();

        public async Task Bid(Domain domain)
        {
            var getAuctionHtml = await _httpCaller.GetHtml($"https://client.dropcatch.com/GetDomainDetail?DomainName={domain.DomainName}");
            var obj = JObject.Parse(getAuctionHtml);
            var auctionId = obj.SelectToken("..item.id");
            var amount = (decimal)obj.SelectToken("..item.nextValidBid");
            if (amount > domain.MaxBid)
                throw new Exception($@"Failed to bid because the next valid amount {amount} > max bid {domain.MaxBid}");
            var postJsonData = "{\"auctionId\": " + auctionId + $", \"amount\": " + amount + "}}}";
            var bidJson = await _httpCaller.PostJson("https://client.dropcatch.com/PlaceBid", postJsonData);
            obj = JObject.Parse(bidJson);
            var isSuccess = (bool)obj.SelectToken("result.success");
            if (!isSuccess)
                throw new Exception("Error on bid : " + bidJson);
            else
                Reporter.Log($"Bid on {domain.DomainName} succed");

        }

        public async Task Test()
        {
            var html = await _httpCaller.GetHtml("https://client.dropcatch.com/GetDomainDetail?DomainName=tapewrite.com");
            var obj = JObject.Parse(html);
            var auctionId = obj.SelectToken("..item.id");
            Console.WriteLine(auctionId);
        }

        public async Task Login()
        {
            Reporter.Log("Login to dropcatcher");
            var loginFormDoc = await _httpCaller.GetDoc("https://client.dropcatch.com/Authorize?returnUrl=https%3A%2F%2Fwww.dropcatch.com%2F");

            var returnUrl = loginFormDoc.DocumentNode.SelectSingleNode("//input[@id='ReturnUrl']").GetAttributeValue("value", "");
            var requestVerificationToken = loginFormDoc.DocumentNode.SelectSingleNode("//input[@name='__RequestVerificationToken']").GetAttributeValue("value", "");

            var formData = new List<KeyValuePair<string, string>>()
             {
                 new KeyValuePair<string, string>("ReturnUrl",returnUrl),
                 new KeyValuePair<string, string>("Username","AlexanderR"),
                 new KeyValuePair<string, string>("Password","zbzSQFaeV8cg"),
                 new KeyValuePair<string, string>("__RequestVerificationToken",requestVerificationToken),
                 new KeyValuePair<string, string>("RememberLogin","false"),

             };
            var html = await _httpCaller.PostFormData("https://secure.dropcatch.com/Login", formData);
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            var accessToken = doc.DocumentNode.SelectSingleNode("//input[@name='access_token']").GetAttributeValue("value", "");
            Reporter.Log("Logged in to dropcatcher");
            _httpCaller = new HttpCaller(accessToken);


        }
    }
}