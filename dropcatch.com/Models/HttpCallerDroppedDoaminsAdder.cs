using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace dropcatch.com.Models
{
    public class HttpCallerDroppedDoaminsAdder
    {
        public HttpClient _httpClient;
        public HttpClient _httpClient1;
        public readonly HttpClientHandler _httpClientHandler = new HttpClientHandler()
        {
            //CookieContainer = new CookieContainer(),
            //AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        public readonly HttpClientHandler _httpClientHandler1 = new HttpClientHandler()
        {
            UseCookies = false,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        public HttpCallerDroppedDoaminsAdder()
        {
            _httpClient = new HttpClient(_httpClientHandler);
            _httpClient1 = new HttpClient(_httpClientHandler1);

        }
        public async Task<(HtmlDocument doc, string error)> GetDoc(string url, int maxAttempts = 1)
        {
            var resp = await GetHtml(url, maxAttempts);
            if (resp.error != null) return (null, resp.error);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(resp.html);
            return (doc, null);
        }
        public async Task<(string html, string error)> GetHtml(string url, int maxAttempts = 10)
        {

            int tries = 0;
            do
            {
                try
                {
                    var response = await _httpClient.GetAsync(url);
                    string html = await response.Content.ReadAsStringAsync();
                    return (html, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    tries++;
                    if (tries == maxAttempts)
                    {
                        return (null, ex.ToString());
                    }
                    await Task.Delay(2000);
                }
            } while (true);
        }
        public async Task<(HtmlDocument doc, string cookie, string error)> GetDocToExtractFormdatForBuyingDomain(string url, string cookie, int maxAttempts = 1)
        {
            var resp = await GetHtml2(url, cookie, maxAttempts);
            if (resp.error != null) return (null, null, resp.error);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(resp.html);
            return (doc, resp.cookie, null);
        }
        public async Task<(string html, string cookie, string error)> GetHtml2(string url, string cookie, int maxAttempts = 1)
        {
            //var _httpClient2 = new HttpClient(_httpClientHandler1);
            //_httpClient2.DefaultRequestHeaders.Add("cookie", cookie);
            int tries = 0;
            do
            {
                try
                {
                    var request = new HttpRequestMessage();
                    request.Headers.Add("cookie", cookie);
                    request.Method = HttpMethod.Get;
                    request.RequestUri = new Uri(url);
                    var response = await _httpClient1.SendAsync(request);
                    string setCookie = response.Headers.GetValues("set-cookie").FirstOrDefault();
                    var html = await response.Content.ReadAsStringAsync();
                    //return (html, setCookie, null);

                    //var response = await _httpClient2.GetAsync(url);
                    //string setCookie = response.Headers.GetValues("set-cookie").FirstOrDefault();
                    //string html = await response.Content.ReadAsStringAsync();
                    return (html, setCookie, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    tries++;
                    if (tries == maxAttempts)
                    {
                        return (null, null, ex.ToString());
                    }
                    await Task.Delay(2000);
                }
            } while (true);
        }
        public async Task<(string cookie, string json, string error)> PostJson(string url, string json, int maxAttempts = 1)
        {
            int tries = 0;
            do
            {
                try
                {
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    // content.Headers.Add("x-appeagle-authentication", Token);
                    var r = await _httpClient.PostAsync(url, content);
                    string cookie = r.Headers.GetValues("set-cookie").FirstOrDefault();
                    var s = await r.Content.ReadAsStringAsync();

                    return (cookie, s, null);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    tries++;
                    if (tries == maxAttempts)
                    {
                        return (null, null, e.ToString());
                    }
                    await Task.Delay(2000);
                }
            } while (true);

        }
        public async Task<(string cookie, string json, string error)> PostJson1(string url, string json, string cookie, int maxAttempts = 1)
        {
            var request = new HttpRequestMessage();
            if (url.Contains("yay.com"))
            {
                request.Headers.Add("cookie", cookie);
            }
            if (url.Contains("youdot.io"))
            {
                request.Headers.Add("authorization", "Bearer " + cookie);
            }
            request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.163 Safari/537.36");
            int tries = 0;
            do
            {
                try
                {
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    request.Method = HttpMethod.Post;
                    request.Content = content;
                    request.RequestUri = new Uri(url);
                    var response = await _httpClient1.SendAsync(request);
                    string setCookie = response.Headers.GetValues("set-cookie").FirstOrDefault();
                    var s = await response.Content.ReadAsStringAsync();
                    return (setCookie, s, null);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    tries++;
                    if (tries == maxAttempts)
                    {
                        return (null, null, e.ToString());
                    }
                    await Task.Delay(2000);
                }
            } while (true);

        }

        public async Task<(string html, string error)> PostFormData(string url, List<KeyValuePair<string, string>> formData, int maxAttempts = 10)
        {
            var formContent = new FormUrlEncodedContent(formData);
            int tries = 0;
            do
            {
                try
                {
                    var response = await _httpClient.PostAsync(url, formContent);
                    string html = await response.Content.ReadAsStringAsync();
                    return (html, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    tries++;
                    if (tries == maxAttempts)
                    {
                        return (null, ex.ToString());
                    }
                    await Task.Delay(2000);
                }
            } while (true);
        }
        public async Task<(string html, string error)> PostFormData1(string url, List<KeyValuePair<string, string>> formData, string cookie, int maxAttempts = 1)
        {
            //var _httpClient2 = new HttpClient(_httpClientHandler1);
            //_httpClient2.DefaultRequestHeaders.Add("cookie", cookie);
            //_httpClient2.DefaultRequestHeaders.Add("User-Agent","Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.163 Safari/537.36");
            var formContent = new FormUrlEncodedContent(formData);
            int tries = 0;
            do
            {
                try
                {
                    var request = new HttpRequestMessage();
                    request.Method = HttpMethod.Post;
                    request.Headers.Add("cookie", cookie);
                    request.Content = formContent;
                    request.RequestUri = new Uri(url);
                    var response = await _httpClient1.SendAsync(request);
                    string setCookie = response.Headers.GetValues("set-cookie").FirstOrDefault();
                    var html = await response.Content.ReadAsStringAsync();
                    return (html, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    tries++;
                    if (tries == maxAttempts)
                    {
                        return (null, ex.ToString());
                    }
                    await Task.Delay(2000);
                }
            } while (true);
        }

    }
}
