using Google.Apis.Services;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ConBrioMusica
{
    public class YouTubeSearchResult
    {
        public string Title { get; set; }
        public string Url { get; set; }
        // Add more properties as needed, e.g., ThumbnailUrl, ChannelName, Views, etc.
    }

    internal class YouTubeSearchHtmlParser
    {
        private readonly HttpClient _httpClient;

        public YouTubeSearchHtmlParser()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<YouTubeSearchResult>> ParseSearchResults(string query)
        {
            var results = new List<YouTubeSearchResult>();

            //string url = $"https://www.youtube.com/results?search_query={searchQuery}";
            string url = $"https://duckduckgo.com/?origin=funnel_home_google&t=h_&q=youtube+{query}&ia=web";
            try
            {
                var firstResult = await GetFirstSeleniumResult(query);

                if ((firstResult != null) && (firstResult.Url.Length < 100))
                {
                    results.Add(new YouTubeSearchResult
                    {
                        Title = firstResult.Title,
                        Url = firstResult.Url
                    });
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP errors (e.g., network issues, server errors)
                System.Console.WriteLine($"HTTP Request Error: {ex.Message}");
            }
            catch (System.Exception ex)
            {
                // Handle other potential errors during parsing
                System.Console.WriteLine($"Error parsing YouTube search results: {ex.Message}");
            }

            return results;
        }

        private static string GetPageSourceWithSelenium(string url)
        {
            IWebDriver driver = null;
            try
            {
                // Set up Chrome options (optional: run headless for background execution)
                ChromeOptions options = new ChromeOptions();
                //options.AddArguments("--headless"); // Uncomment to run in the background without UI

                // Initialize the ChromeDriver
                driver = new ChromeDriver(options);
                driver.Navigate().GoToUrl(url);

                // Wait for a few seconds for the JavaScript content to load
                Thread.Sleep(3000); // Adjust the sleep time if needed

                // Get the fully rendered page source
                return driver.PageSource;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred with Selenium: {ex.Message}");
                return null;
            }
            finally
            {
                // Close the browser
                if (driver != null)
                {
                    driver.Quit();
                }
            }
        }

        private static string ParseHtmlForFirstResult(string html)
        {
            //HtmlDocument htmlDoc = new HtmlDocument();
            //htmlDoc.LoadHtml(html);
            string filePath = @"C:\Users\gocav\source\repos\ConBrioMusica\html_parse.txt";
            File.WriteAllText(filePath, html);

            // The exact XPath or CSS selector can change over time as websites update.
            // A common selector for video links on YouTube search results is a link with id 'thumbnail'
            //var firstResultNode = htmlDoc.DocumentNode.SelectSingleNode("//a[@id='href=\"www.youtube.com/watch']");
            //href="https://www.youtube.com/watch?v=
            //var firstResultingNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='href=\"www.youtube.com/watch?v=']");
            string fs = "www.youtube.com/watch?v=";
            string es = "\"";
            int pFrom = html.IndexOf(fs) + fs.Length;
            String partOne = html.Substring(pFrom, html.Length - pFrom);

            int pTo = partOne.IndexOf(es);

           // String partTwo = partOne.Substring(0, pTo);
            //pTo = partTwo.IndexOf(es);
            String result = partOne.Substring(0, pTo);

            if (result.Length > 0)
            {
                // Extract the 'href' attribute which contains the video URL
                return "watch?v=" + result;
            }
            else
            {
                Console.WriteLine("Could not find the first result node using the current XPath/CSS selector.");
                return string.Empty;
            }
        }

        public static async Task GetVideoDetails(string videoId, string apiKey)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                ApplicationName = "ConBrioMusica"
            });

            var listRequest = youtubeService.Videos.List("snippet,statistics");
            listRequest.Id = videoId;
            var response = await listRequest.ExecuteAsync();

            foreach (var video in response.Items)
            {
                Console.WriteLine($"Title: {video.Snippet.Title}");
                Console.WriteLine($"Views: {video.Statistics.ViewCount}");
                Console.WriteLine($"Likes: {video.Statistics.LikeCount}");
            }
        }

        public static async Task GetVideoSearch(string videoId, string apiKey)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                ApplicationName = "ConBrioMusica"
            });
            //Await YouTube.GoogleSearch.GetResultAsync(txtSearch.Text)

            //foreach (var video in response.Items)
            //{
            //    Console.WriteLine($"Title: {video.Snippet.Title}");
            //    Console.WriteLine($"Views: {video.Statistics.ViewCount}");
            //    Console.WriteLine($"Likes: {video.Statistics.LikeCount}");
            //}
        }

        public static async Task<SearchResult> GetFirstDuckDuckGoResult(string query)
        {
            // DuckDuckGo offers a specific URL for HTML-only results, which is easier to parse
            var encodedQuery = WebUtility.UrlEncode("youtube+1985+Bo+Durnham");// query);
            //var url = $"https://duckduckgo.com/html/?q={encodedQuery}";
            //string url = $"https://duckduckgo.com/?origin=funnel_home_google&t=h_&q={encodedQuery}&ia=web";
            string url = $"https://duckduckgo.com/?origin=funnel_home_google&t=h_&q=youtube+1985+Bo+Durnham&ia=web";
            var encodedUrl = WebUtility.UrlEncode(url);// query);


            var httpClient = new HttpClient();
            // Set a User-Agent header to avoid potential blocking
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");

            var html = await httpClient.GetStringAsync(url);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            string filePath = @"C:\Users\gocav\source\repos\ConBrioMusica\html_duck.txt";
            File.WriteAllText(filePath, html);

            // Find the first search result node
            // DuckDuckGo uses divs with class 'results' and links inside 'a' tags
            //var firstResultNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='links']/div[contains(@class, 'results_links_deep')]/h2/a");
            //var firstResultNode = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'result')]//a[contains(@class, 'result__a')]");
            var firstResultNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='contextMenu-www.youtube.com/watch?v=']");

            if (firstResultNode != null)
            {
                var title = firstResultNode.InnerText;
                var relativeUrl = firstResultNode.GetAttributeValue("href", string.Empty);

                // DuckDuckGo uses relative paths for the html version
                var baseUri = new Uri("https://duckduckgo.com");
                var absoluteUri = new Uri(baseUri, relativeUrl).AbsoluteUri;

                return new SearchResult { Title = title, Url = absoluteUri };
            }

            return null;
        }

        public static async Task<SearchResult> GetFirstSeleniumResult(string query)
        {
            string url = $"https://duckduckgo.com/?origin=funnel_home_google&t=h_&q=youtube+{query}&ia=web";

            //var html = await GetPageSourceWithSelenium(url);
            string html = GetPageSourceWithSelenium(url);

            if (!string.IsNullOrEmpty(html))
            {
                // Use HtmlAgilityPack to parse the source and find the first link
                string firstResultLink = ParseHtmlForFirstResult(html);

                Console.WriteLine($"The first YouTube search result link is: {firstResultLink}");
                var title = query.Replace('+', ' ');

                // DuckDuckGo uses relative paths for the html version
                var baseUri = new Uri("https://youtube.com");
                var absoluteUri = new Uri(baseUri, firstResultLink).AbsoluteUri;

                return new SearchResult { Title = title, Url = absoluteUri };
            }

            return null;
        }

    }

    public class SearchResult
    {
        public string Title { get; set; }
        public string Url { get; set; }
    }
}
