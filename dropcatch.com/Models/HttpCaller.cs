using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace dropcatch.com.Models
{
    public class HttpCaller
    {
        private readonly HttpClient _httpClient;
        private readonly HttpClient _httpClient2;
        HttpRequestMessage request = new HttpRequestMessage();
        readonly HttpClientHandler _httpClientHandler = new HttpClientHandler()
        {
            CookieContainer = new CookieContainer(),
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        readonly HttpClientHandler _httpClientHandler2 = new HttpClientHandler()
        {
            UseCookies = false
        };
        public HttpCaller()
        {
            _httpClient = new HttpClient(_httpClientHandler) { Timeout = TimeSpan.FromMinutes(5) };
            _httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36");
            _httpClient2 = new HttpClient(_httpClientHandler2);
        }

        public HttpCaller(string bearer)
        {
            _httpClient = new HttpClient(_httpClientHandler);
            _httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("authorization", "Bearer " + bearer);
        }
        public async Task<HtmlDocument> GetDoc(string url, int maxAttempts = 1)
        {
            var html = await GetHtml(url, maxAttempts);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc;
        }
        public async Task<string> GetHtml(string url, int maxAttempts = 1)
        {
            int tries = 0;
            do
            {
                try
                {
                    string html;
                    if (url.Contains("domainlore.uk"))
                    {
                        var response = await _httpClient.GetAsync(url);
                        byte[] byteArray = await response.Content.ReadAsByteArrayAsync();
                        html = Encoding.UTF8.GetString(byteArray);
                    }
                    else
                    {
                        var response = await _httpClient.GetAsync(url);
                        html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
                    }
                    return html;
                }
                catch (WebException ex)
                {
                    var errorMessage = "";
                    try
                    {
                        errorMessage = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    }
                    catch (Exception)
                    {
                    }
                    tries++;
                    if (tries == maxAttempts)
                    {
                        throw new Exception(ex.Status + " " + ex.Message + " " + errorMessage);
                    }
                    await Task.Delay(2000);
                }
            } while (true);
        }
        public async Task<string> PostJson(string url, string json, int maxAttempts = 1)
        {
            int tries = 0;
            do
            {
                try
                {
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    // content.Headers.Add("x-appeagle-authentication", Token);
                    var r = await _httpClient.PostAsync(url, content);
                    var s = await r.Content.ReadAsStringAsync();
                    return (s);
                }
                catch (WebException ex)
                {
                    var errorMessage = "";
                    try
                    {
                        errorMessage = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    }
                    catch (Exception)
                    {
                    }
                    tries++;
                    if (tries == maxAttempts)
                    {
                        throw new Exception(ex.Status + " " + ex.Message + " " + errorMessage);
                    }
                    await Task.Delay(2000);
                }
            } while (true);

        }
        public async Task<Stream> GetStream(string url, int maxAttempts = 5)
        {
            int tries = 0;
            do
            {
                try
                {
                    var response = await _httpClient.GetAsync(url);
                    //byte[] byteArray = await response.Content.ReadAsByteArrayAsync();
                    //string html = Encoding.UTF8.GetString(byteArray);
                    return await response.Content.ReadAsStreamAsync();
                }
                catch (WebException ex)
                {
                    var errorMessage = "";
                    try
                    {
                        errorMessage = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    }
                    catch (Exception)
                    {
                    }
                    tries++;
                    if (tries == maxAttempts)
                    {
                        throw new Exception(ex.Status + " " + ex.Message + " " + errorMessage);
                    }
                    await Task.Delay(2000);
                }
            } while (true);
        }
        public async Task<string> PostFormData(string url, List<KeyValuePair<string, string>> formData, int maxAttempts = 1)
        {
            var formContent = new FormUrlEncodedContent(formData);
            int tries = 0;
            do
            {
                try
                {
                    string html;
                    if (url.Contains("dropcatch.com"))
                    {
                        var response = await _httpClient.PostAsync(url, formContent);
                        html = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        var response = await _httpClient.PostAsync(url, formContent);
                        byte[] byteArray = await response.Content.ReadAsByteArrayAsync();
                        html = Encoding.UTF8.GetString(byteArray);
                    }
                    return html;
                }
                catch (WebException ex)
                {
                    var errorMessage = "";
                    try
                    {
                        errorMessage = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    }
                    catch (Exception)
                    {
                    }
                    tries++;
                    if (tries == maxAttempts)
                    {
                        throw new Exception(ex.Status + " " + ex.Message + " " + errorMessage);
                    }
                    await Task.Delay(2000);
                }
            } while (true);
        }
        public async Task DeletFileFromWebsite(string url, List<KeyValuePair<string, string>> formData, string cookies, int maxAttempts = 1)
        {
            var formContent = new FormUrlEncodedContent(formData);
            int tries = 0;
            do
            {
                try
                {
                    request = new HttpRequestMessage();
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(url);
                    request.Content = formContent;
                    request.Headers.Add("Cookie", cookies);
                    await _httpClient2.SendAsync(request);
                    break;
                }
                catch (WebException ex)
                {
                    var errorMessage = "";
                    try
                    {
                        errorMessage = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    }
                    catch (Exception)
                    {
                    }
                    tries++;
                    if (tries == maxAttempts)
                    {
                        throw new Exception(ex.Status + " " + ex.Message + " " + errorMessage);
                    }
                    await Task.Delay(2000);
                }
            } while (true);
        }
        public async Task<Stream> GetStream(string url, string cookies, int maxAttempts = 5)
        {
            int tries = 0;
            do
            {
                try
                {
                    request = new HttpRequestMessage();
                    request.Method = HttpMethod.Get;
                    request.RequestUri = new Uri(url);
                    request.Headers.Add("Cookie", cookies);
                    var response = await _httpClient2.SendAsync(request);
                    return await response.Content.ReadAsStreamAsync();
                }
                catch (WebException ex)
                {
                    var errorMessage = "";
                    try
                    {
                        errorMessage = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    }
                    catch (Exception)
                    {
                    }
                    tries++;
                    if (tries == maxAttempts)
                    {
                        throw new Exception(ex.Status + " " + ex.Message + " " + errorMessage);
                    }
                    await Task.Delay(2000);
                }
            } while (true);
        }
    }
}
