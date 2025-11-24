using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConBrioMusica
{
    internal class YouTubeHtmlParser
    {
        private HtmlAgilityPack.HtmlDocument _htmlDocument;

        public YouTubeHtmlParser()
        {
            _htmlDocument = new HtmlAgilityPack.HtmlDocument();
        }

        // Load HTML from a URL
        public async Task LoadHtmlFromUrlAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var html = await httpClient.GetStringAsync(url);
                _htmlDocument.LoadHtml(html);
            }
        }

        // Load HTML from a string
        public void LoadHtmlFromString(string html)
        {
            _htmlDocument.LoadHtml(html);
        }

        // Example method to extract video title
        public string GetVideoTitle()
        {
            //<div><h3 class="wZ4JdaHxSAhGy1HoNVja yGEuosa_aZeFroGMfpgu RDtM9fAcGGHRulQKs39C" title="Van Halen - Ain't Talkin' 'Bout Love (Live 1983 US Festival)">
            // YouTube's HTML structure can change, so XPath might need adjustment.
            // This is a common XPath for a video title, but verify with current YouTube HTML.
            var titleNode = _htmlDocument.DocumentNode.SelectSingleNode("//meta[@property='og:title']");
            //var titleNodeByClass = _htmlDocument.DocumentNode.SelectSingleNode("//h3[contains(@class, 'wZ4JdaHxSAhGy1HoNVja')]");
            //var titleNode = _htmlDocument.DocumentNode.SelectSingleNode($"//h3[@title=\"{targetTitle}\"]\"");

            // 2. Check if the node was found
            if (titleNode != null)
            {
                // 3. Extract the value of the 'title' attribute
                // GetAttributeValue(attributeName, defaultValueIfNotFound)
                string titleValue = titleNode.GetAttributeValue("title", "Title Not Found");

                // 4. Use the extracted value
                Console.WriteLine($"Extracted Title: {titleValue}");
            }
            else
            {
                Console.WriteLine("The specified h3 tag was not found in the document.");
            }
            return titleNode?.GetAttributeValue("content", "Title Not Found");
        }

        // Example method to extract video description
        public string GetVideoDescription()
        {
            var descriptionNode = _htmlDocument.DocumentNode.SelectSingleNode("//meta[@property='og:description']");
            return descriptionNode?.GetAttributeValue("content", "Description Not Found");
        }

        // Example method to extract embed URL
        public string GetEmbedUrl()
        {
            var embedUrlNode = _htmlDocument.DocumentNode.SelectSingleNode("//meta[@property='og:video:url']");
            return embedUrlNode?.GetAttributeValue("content", "Embed URL Not Found");
        }

        // Extracts the song name from potential microdata or page title parsing
        public string GetSongName()
        {
            // Attempt to find metadata using schema.org microdata (common on music videos)
            var songNode = _htmlDocument.DocumentNode.SelectSingleNode("//meta[@itemprop='name']");
            if (songNode != null && songNode.GetAttributeValue("content", "").Length > 0)
            {
                return songNode.GetAttributeValue("content", "");
            }

            // Fallback: Parse from the video title if schema is unavailable
            string fullTitle = GetVideoTitle();
            if (!string.IsNullOrEmpty(fullTitle) && fullTitle != "Title Not Found")
            {
                // A common format is "Artist Name - Song Name"
                var parts = fullTitle.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    return parts[1].Trim();
                }
            }

            return "Song Name Not Found";
        }

        // Extracts the artist name from potential microdata or page title parsing
        public string GetArtistName()
        {
            // Attempt to find metadata using schema.org microdata
            var artistNode = _htmlDocument.DocumentNode.SelectSingleNode("//span[@itemprop='byArtist']/a[@itemprop='url']/span[@itemprop='name']");
            if (artistNode != null)
            {
                return artistNode.InnerText.Trim();
            }

            // Fallback: Parse from the video title
            string fullTitle = GetVideoTitle();
            if (!string.IsNullOrEmpty(fullTitle) && fullTitle != "Title Not Found")
            {
                var parts = fullTitle.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    return parts[0].Trim();
                }
            }

            // Another common location is a link in the description or a meta tag for the channel/uploader
            var channelNode = _htmlDocument.DocumentNode.SelectSingleNode("//meta[@itemprop='channelTitle']");
            if (channelNode != null)
            {
                return channelNode.GetAttributeValue("content", "Artist Name Not Found");
            }

            return "Artist Name Not Found";
        }

        // Extracts the album name
        public string GetAlbumName()
        {
            // This information is usually present in the extra description metadata for official music videos
            var albumNode = _htmlDocument.DocumentNode.SelectNodes("//div[@id='watch-description-extra-info']//a")?
                                          .FirstOrDefault(node => node.InnerText.Contains("Album"))?.ParentNode;

            if (albumNode != null)
            {
                // The album name might be a sibling node or within a specific structure
                // The exact path can vary greatly. This is a best guess.
                return albumNode.InnerText.Replace("Album:", "").Trim();
            }

            // Look for schema.org data for "album"
            var albumMetaNode = _htmlDocument.DocumentNode.SelectSingleNode("//meta[@itemprop='album']/@content");
            if (albumMetaNode != null)
            {
                return albumMetaNode.GetAttributeValue("content", "Album Not Found");
            }

            return "Album Not Found";
        }

        // Extracts the release date
        public string GetReleaseDate()
        {
            // Look for schema.org data for "datePublished"
            var dateNode = _htmlDocument.DocumentNode.SelectSingleNode("//meta[@itemprop='datePublished']");
            if (dateNode != null)
            {
                return dateNode.GetAttributeValue("content", "Release Date Not Found");
            }

            // Fallback to searching the description for a date format
            string description = GetVideoDescription();
            if (!string.IsNullOrEmpty(description) && description != "Description Not Found")
            {
                // Simple regex for a common date format (e.g., YYYY-MM-DD)
                var match = Regex.Match(description, @"\d{4}-\d{2}-\d{2}");
                if (match.Success)
                {
                    return match.Value;
                }
            }

            return "Release Date Not Found";
        }
    }
}
