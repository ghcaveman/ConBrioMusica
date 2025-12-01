using HtmlAgilityPack;
using OpenQA.Selenium.BiDi.Script;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using TagLib;

namespace ConBrioMusica
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            //set some defaults
            destinationFolderTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            PopulateGridbox();
            string clipboardText = Clipboard.GetText();
            //https://www.youtube.com/watch?v=mk48xRzuNvA&pp=ygUbc2l0dGluZyBpbiB0aGUgaGFsbCBvZiBmYW1l

            List<string> list = new List<string> { "http://www.youtube.com", "https://www.youtube.com" };
            foreach (string word in list)
            {
                if (clipboardText.StartsWith(word))
                {
                    UrlTextBox.Text = clipboardText;
                    break;
                }
            }
        }

        private async void GoButton_Click(object sender, EventArgs e)
        {
            GoButton.Enabled = false;
            try
            {
                var parser = new YouTubeHtmlParser();
                await parser.LoadHtmlFromUrlAsync(UrlTextBox.Text);

                string title = parser.GetVideoTitle();
                int index = title.IndexOf('(');
                if (index > 0)
                {
                    title = title.Substring(0, index).Trim();
                }
                string artist = parser.GetArtistName();
                index = artist.IndexOf(',');
                if (index > 0)
                {
                    artist = artist.Substring(0, index).Trim();
                }
                string song = parser.GetSongName();
                index = song.IndexOf('(');
                if (index > 0)
                {
                    song = song.Substring(0, index).Trim();
                }
                index = song.IndexOf(',');
                if (index > 0)
                {
                    song = song.Substring(index + 1, song.Length - (index + 1)).Trim();
                }
                song = song.Replace(" - ", "-");
                string album = parser.GetAlbumName();
                string release_date = parser.GetReleaseDate();
                string description = parser.GetVideoDescription();
                string embedUrl = parser.GetEmbedUrl();

                string dir = destinationFolderTextBox.Text;
                string ext = extensionLabel.Text;
                string updated_filename = song + "-" + artist;
                updated_filename = updated_filename.Replace(' ', '_');
                string full_path = dir + "\\" + updated_filename + ext;

                //set the defaults to mp3, and change if video is selected
                string format = extensionLabel.Text;
                format = format.Replace(".", ""); //remove the dot
                string programPath = @"C:\ffmpeg\bin\yt-dlp.exe"; // Replace with your program's path
                string arguments = " --extract-audio --audio-format " + format + " --embed-thumbnail " + UrlTextBox.Text; // Replace with your arguments                                                                                                // Get the path to the My Documents folder

                arguments += " -o \"" + full_path + "\"";

                using (Process processWithInfo = new Process())
                {
                    processWithInfo.StartInfo.FileName = programPath;
                    processWithInfo.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(programPath);
                    processWithInfo.StartInfo.Arguments = arguments;
                    processWithInfo.StartInfo.UseShellExecute = false; // Required for stream redirection
                    processWithInfo.StartInfo.RedirectStandardOutput = true;
                    processWithInfo.StartInfo.RedirectStandardError = true;
                    processWithInfo.StartInfo.CreateNoWindow = true; // Optional: hide the console window

                    processWithInfo.Start();

                    // Read output and error streams
                    string output = processWithInfo.StandardOutput.ReadToEnd();
                    string error = processWithInfo.StandardError.ReadToEnd();

                    processWithInfo.WaitForExit();

                    Console.WriteLine("Output:");
                    Console.WriteLine(output);
                    Console.WriteLine("Error:");
                    Console.WriteLine(error);
                    Console.WriteLine($"External program (with info) exited with code: {processWithInfo.ExitCode}");
                }
                string url = UrlTextBox.Text;
                index = url.IndexOf('&');
                if (index > 0)
                {
                    url = url.Substring(0, index).Trim();
                }
                ModifyFile(full_path, title, url);

                System.Windows.MessageBox.Show("Operation completed!");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An error occurred: {ex.Message}");
            }
            finally
            {
                // Re-enable the button after the operation completes or an error occurs
                GoButton.Enabled = true;
            }
        }

        private void videoRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (videoRadioButton.Checked)
            {
                extensionLabel.Text = ".mp4";
                destinationFolderTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            }
            else
            {
                extensionLabel.Text = ".mp3";
                destinationFolderTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            }
        }

        private void UrlTextBox_TextChanged(object sender, EventArgs e)
        {
            string clipboardText = Clipboard.GetText();
        }

        private void PopulateGridbox()
        {
            dataGridView1.Rows.Clear();
            string directoryPath = destinationFolderTextBox.Text;
            string extensionFilter = "*" + extensionLabel.Text; // Example filter for mp3 files
            string processFile = string.Empty;
            UInt32 fileCount = 0U;
            try
            {
                // You can also filter files by a search pattern
                string[] filteredFiles = Directory.GetFiles(directoryPath, extensionFilter); //556
                foreach (string filePath in filteredFiles)
                {
                    processFile = filePath;
                    // Create a TagLib.File object from the file path
                    TagLib.File tagFile = TagLib.File.Create(filePath);
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                    string title = fileNameWithoutExtension.Replace("_", " ");
                    title = title.Replace("-", " - ");
                    string artistName2 = tagFile.Tag.FirstAlbumArtist;
                    string artistName = tagFile.Tag.FirstArtist;
                    string artistName3 = tagFile.Tag.FirstComposer;
                    string songName = tagFile.Tag.Title;
                    string album = tagFile.Tag.Album;
                    uint year = tagFile.Tag.Year;
                    string comment = tagFile.Tag.Comment; //let's store the youtube link in here for now
                    int index = dataGridView1.Rows.Add();
                    dataGridView1.Rows[index].Cells[0].Value = fileNameWithoutExtension;
                    dataGridView1.Rows[index].Cells[1].Value = title;
                    dataGridView1.Rows[index].Cells[2].Value = artistName;
                    dataGridView1.Rows[index].Cells[3].Value = songName;
                    dataGridView1.Rows[index].Cells[4].Value = album;
                    dataGridView1.Rows[index].Cells[5].Value = year.ToString();
                    fileCount++;
                }
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine($"Error: Directory not found at {directoryPath}");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Error: Access to {directoryPath} is denied.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred processing {processFile}: {ex.Message}");
            }
        }

        private async void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewButtonCell btn = (DataGridViewButtonCell)dataGridView1.Rows[e.RowIndex].Cells["Fix"];
            btn.ReadOnly = true;
            // Check if the clicked cell is in your button column
            if (e.ColumnIndex == dataGridView1.Columns["Fix"].Index && e.RowIndex >= 0)
            {
                //save our selected row for reference
                selectedRowLabel.Text = e.RowIndex.ToString();
                //https://www.youtube.com/results?search_query=1985+bo+burnham
                string searchTerm = "";
                if (dataGridView1.Rows[e.RowIndex].Cells["Artist"].Value != null)
                {
                    searchTerm += dataGridView1.Rows[e.RowIndex].Cells["Artist"].Value.ToString() + " ";
                }
                if (dataGridView1.Rows[e.RowIndex].Cells["Song"].Value != null)
                {
                    searchTerm += dataGridView1.Rows[e.RowIndex].Cells["Song"].Value.ToString() + " ";
                }
                if (dataGridView1.Rows[e.RowIndex].Cells["Title"].Value != null)
                {
                    searchTerm += WebUtility.UrlDecode(dataGridView1.Rows[e.RowIndex].Cells["Title"].Value.ToString()) + " ";
                }
                searchTerm = searchTerm.Replace("&#39;", "");
                searchTerm = searchTerm.Replace("_", " ");
                searchTerm = searchTerm.Replace("-", " ");
                List<string> wordList = searchTerm.Split(' ', (char)StringSplitOptions.RemoveEmptyEntries).ToList();
                List<string> distinctList = wordList.Distinct().ToList();
                searchTerm = "";
                foreach (string word in distinctList)
                {
                    if (word.Trim().Length > 0)
                    {
                        searchTerm += word.Trim() + "+";
                    }
                }
                //strip off the trailing "+"
                if (searchTerm.Length > 1)
                {
                    searchTerm = searchTerm.Remove(searchTerm.Length - 1);
                }
                // attempt to find the song origin on youtube/google
                try
                {
                    var parser = new YouTubeSearchHtmlParser();
                    var searchResults = await parser.ParseSearchResults(searchTerm);
                    if (searchResults != null && searchResults.Count > 0)
                    {
                        string title = searchResults[0].Title;
                        string url = searchResults[0].Url;
                        string filename = destinationFolderTextBox.Text + "\\" + dataGridView1.Rows[e.RowIndex].Cells["Filename"].Value.ToString() + extensionLabel.Text;
                        ModifyFile(filename, title, url);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"An error occurred: {ex.Message}");
                }
                finally
                {
                    // Re-enable the button after the operation completes or an error occurs
                    btn = (DataGridViewButtonCell)dataGridView1.Rows[e.RowIndex].Cells["Fix"];
                    btn.ReadOnly = false;
                }
            }
        }

        string ReplaceOffensiveTerms(string input)
        {
            string output = input;
            // Define a list of offensive terms and their replacements
            var replacements = new Dictionary<string, string>
            {
                { "official", "" },
                { "lyrics", "" },
                { "greatest hits", "" },
                { "music video", "" },
                // Add more terms as needed
            };
            // Perform the replacements
            foreach (var pair in replacements)
            {
                output = Regex.Replace(output, pair.Key, pair.Value, RegexOptions.IgnoreCase);
            }
            return output;
        }

        private async void ModifyFile(string filename, string title, string url)
        {
            if (string.IsNullOrEmpty(url) != true)
            {
                var parser = new YouTubeHtmlParser();
                await parser.LoadHtmlFromUrlAsync(url);

                string video_title = parser.GetVideoTitle();
                string artist = parser.GetArtistName();
                string song = parser.GetSongName();
                string album = parser.GetAlbumName();
                string release_date = parser.GetReleaseDate();
                string description = parser.GetVideoDescription();
                string embedUrl = parser.GetEmbedUrl();
                //now we need to attempt to fix everything up the way we like it
                int index = video_title.IndexOf('(');
                if (index > 0)
                {
                    video_title = video_title.Substring(0, index).Trim();
                }
                video_title = video_title.Replace("&#39;", "");
                index = song.IndexOf('(');
                if (index > 0)
                {
                    song = song.Substring(0, index).Trim();
                }
                index = song.IndexOf('[');
                if (index > 0)
                {
                    song = song.Substring(0, index).Trim();
                }
                description = description.Replace("&#39;", "");
                string ss = " - ";
                index = song.IndexOf(ss);
                bool unknown = IsArtistUnknown(artist);
                if (index > 0)
                {
                    if (string.IsNullOrEmpty(artist) || unknown == true)
                    {
                        artist = song.Substring(0, index).Trim();
                    }
                    song = song.Substring(index + ss.Length, song.Length - (index + ss.Length)).Trim();
                }
                song = ReplaceOffensiveTerms(song);
                artist = ReplaceOffensiveTerms(artist);

                //index = song.IndexOf("Official");
                //if (index > 0)
                //{
                //    song = song.Substring(0, index).Trim();
                //}
                song = song.Replace(artist, "");
                song = song.Replace("[", "");
                song = song.Replace("]", "");
                song = song.Replace("&#39;", "");
                song = song.Replace("&quot;", "");
                song = song.Replace("&amp;", "and");
                if (IsArtistUnknown(artist) == true)
                {
                    artist = ExtractArtistNameFromDescription(description, song);
                }
                index = artist.IndexOf(',');
                if (index > 0)
                {
                    artist = artist.Substring(0, index).Trim();
                }
                artist = artist.Replace(song, "");
                artist = artist.Replace("[", "");
                artist = artist.Replace("]", "");
                artist = artist.Replace("&#39;", "");
                artist = artist.Replace("&amp;", "and");
                //artist = artist.Replace("lyrics", "");
                //artist = artist.Replace("Lyrics", "");
                //artist = artist.Replace("greatest", "");
                //artist = artist.Replace("Greatest", "");
                //artist = artist.Replace("hits", "");
                //artist = artist.Replace("Hits", "");
                //artist = artist.Replace("music", "");
                //artist = artist.Replace("Music", "");
                //artist = artist.Replace("video", "");
                //artist = artist.Replace("Video", "");
                UInt32 year = GetYearFromReleaseDate(release_date);
                title = song + " - " + artist;
                using (ConfirmDialog confForm = new ConfirmDialog())
                {
                    confForm.Title = title;
                    confForm.Artist = artist;
                    confForm.Song = song;
                    confForm.Album = album;
                    confForm.Year = year.ToString();
                    confForm.Comment = url;
                    if (confForm.ShowDialog() == DialogResult.Yes)
                    {
                        try
                        {
                            // Create a TagLib.File object from the existing audio file
                            using (TagLib.File file = TagLib.File.Create(filename))
                            {
                                // Modify the tag properties
                                file.Tag.Title = confForm.Song;
                                file.Tag.Artists = new[] { confForm.Artist };
                                file.Tag.Performers = new[] { confForm.Artist };
                                file.Tag.Album = confForm.Album;
                                file.Tag.Year = uint.Parse(confForm.Year);
                                file.Tag.Comment = confForm.Comment;
                                title = song + " - " + artist;

                                // Save the changes back to the file
                                file.Save();
                                string dir = Path.GetDirectoryName(filename);
                                string ext = Path.GetExtension(filename);
                                string updated_filename = confForm.Song + "-" + confForm.Artist;
                                updated_filename = updated_filename.Replace(' ', '_');
                                updated_filename = updated_filename.Replace("?", "");
                                updated_filename = updated_filename.Replace("/", " ").Trim();
                                System.IO.File.Move(filename, dir + "\\" + updated_filename + ext);

                                Console.WriteLine($"Tags updated successfully for: {filename}");
                                PopulateGridbox();
                                dataGridView1.Rows[int.Parse(selectedRowLabel.Text)].Selected = true;
                                dataGridView1.FirstDisplayedScrollingRowIndex = int.Parse(selectedRowLabel.Text);
                                selectedRowLabel.Text = string.Empty;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"An error occurred: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Input cancelled.");
                    }
                }                //MessageBoxResult result = System.Windows.MessageBox.Show(
                //    "Do you want to save the following changes:" + Environment.NewLine +
                //    "artist: " + artist + Environment.NewLine +
                //    "song: " + song + Environment.NewLine +
                //    "album: " + album + Environment.NewLine +
                //    "year: " + year.ToString() + Environment.NewLine +
                //    "comment: " + url.ToString() + Environment.NewLine +
                //    "description: " + description + Environment.NewLine,
                //    title + " modify confirmation", MessageBoxButton.YesNoCancel);
                //if (result == MessageBoxResult.Yes)
                //{
                //    try
                //    {
                //        // Create a TagLib.File object from the existing audio file
                //        using (TagLib.File file = TagLib.File.Create(filename))
                //        {
                //            // Modify the tag properties
                //            file.Tag.Title = song;
                //            file.Tag.Artists = new[] { artist };
                //            file.Tag.Performers = new[] { artist };
                //            file.Tag.Album = album;
                //            file.Tag.Year = year;
                //            file.Tag.Comment = url;

                //            // Save the changes back to the file
                //            file.Save();
                //            string dir = Path.GetDirectoryName(filename);
                //            string ext = Path.GetExtension(filename);
                //            string updated_filename = song + "-" + artist;
                //            updated_filename = updated_filename.Replace(' ', '_');
                //            updated_filename = updated_filename.Replace("?", "");
                //            updated_filename = updated_filename.Replace("/", " ").Trim();
                //            System.IO.File.Move(filename, dir + "\\" + updated_filename + ext);

                //            Console.WriteLine($"Tags updated successfully for: {filename}");
                //            PopulateGridbox();
                //            dataGridView1.Rows[int.Parse(selectedRowLabel.Text)].Selected = true;
                //            dataGridView1.FirstDisplayedScrollingRowIndex = int.Parse(selectedRowLabel.Text);
                //            selectedRowLabel.Text = string.Empty;
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine($"An error occurred: {ex.Message}");
                //    }
                //}
            }
        }

        private bool IsArtistUnknown(string artist)
        {
            bool unknown = false;
            if (string.IsNullOrEmpty(artist))
            {
                return true;
            }

            // Use IndexOf with StringComparison.OrdinalIgnoreCase for case-insensitive search
            if (artist.IndexOf("unknown", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                unknown = true;
            }

            if (artist.IndexOf("not found", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                unknown = true;
            }

            return unknown;
        }

        private string ExtractArtistNameFromDescription(string description, string song)
        {
            //Provided to YouTube by Virgin Music Group1985 · Bo BurnhamTHE INSIDE OUTTAKES℗ 2022 Attic Bedroom, Corp. under exclusive license to IMPERIALReleased on: 2022...
            string name = description;
            if (string.IsNullOrEmpty(description))
            {
                return string.Empty;
            }
            string fs = " · ";
            string es = "℗";
            if (description.IndexOf(fs) > 0 && description.IndexOf(es) > 0)
            {
                int pFrom = description.IndexOf(fs) + fs.Length;
                int pTo = description.LastIndexOf(es);

                name = description.Substring(pFrom, pTo - pFrom);
            }
            //fix special cases
            name = name.Replace("THE INSIDE OUTTAKES", "");
            name = name.Replace("Pinkerton", "");
            return name;
        }

        private UInt32 GetYearFromReleaseDate(string release_date)
        {
            UInt32 year = 0;
            if (string.IsNullOrEmpty(release_date))
            {
                return year;
            }
            DateTime dt;
            if (DateTime.TryParse(release_date, out dt))
            {
                year = (UInt32)dt.Year;
            }
            return year;
        }
    }
}
