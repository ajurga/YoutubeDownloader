using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;
using YoutubeExplode.Services;
using System.IO;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace YTD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
    
        public MainWindow()
        {
            InitializeComponent();
            Info.Visibility = Visibility.Collapsed;
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadButton.IsEnabled = false;
            //wybieranie ścieżki zapisu pliku
            FolderBrowserDialog FBD = new FolderBrowserDialog();
            if (FBD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (tbURL.Text != "")
                {
                    var id = YoutubeClient.ParseVideoId(tbURL.Text);

                    var client = new YoutubeClient();
                    if (FBD.SelectedPath != @"C:\")
                    {
                       // DownloadButton.Content = "Pobieranie video...";
                        var video = await client.GetVideoAsync(id.ToString());
                        
                        pbDownloadProgress.Visibility = Visibility.Visible;
                        tbInfoProgress.Visibility = Visibility.Visible;
                        tbInfoProgress.Text = "Pobieranie...";

                        var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(id.ToString());
                        var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();
                        if (ExtensionList.SelectedValue != null)
                        {
                            string ext = ExtensionList.SelectedValue.ToString();

                            await client.DownloadMediaStreamAsync(streamInfo, GetFullPath(FBD.SelectedPath, video.Title, ext));

                            DownloadButton.Background = Brushes.LightGreen;
                            //DownloadButton.Content = "Video pobrano pomyślnie!";
                            pbDownloadProgress.Visibility = Visibility.Hidden;
                            tbInfoProgress.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Nie wybrano rozszerzenia pliku!");
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Niepoprawna ściażka do zapisu pliku!");
                    }

                }
                else
                {
                    System.Windows.MessageBox.Show("Nie podałeś ścieżki URL do pliku!");
                }
            }
            DownloadButton.IsEnabled = true;
        }

        private async void LoadInfo_TextChanged(object sender, TextChangedEventArgs e)
        {
            Info.Visibility = Visibility.Collapsed;
            IEnumerable<string> listOfExtension = new List<string>();
            var client = new YoutubeClient();
            var id="";

            try
            {
                id = YoutubeClient.ParseVideoId(tbURL.Text);

                pbDownloadProgress.Visibility = Visibility.Visible;
                tbInfoProgress.Visibility = Visibility.Visible;
                tbInfoProgress.Text = "Przetwarzanie...";

                var video = await client.GetVideoAsync(id.ToString());

                tbInfoProgress.Text = "Przetworzono!";

                //Lista rozszerzeń plików
                var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(id.ToString());
                listOfExtension = streamInfoSet.GetAll().Select(s => s.Container.GetFileExtension()).Distinct();

                ExtensionList.ItemsSource = listOfExtension;

                // Dane o filmie
                lblTitle.Text = video.Title;
                lblDescription.Text = video.Description;
                lblAuthor.Text = video.Author;
                lblDuration.Text = video.Duration.ToString();
                Info.Visibility = Visibility.Visible;
            
                DownloadButton.Background = Brushes.Transparent;

                pbDownloadProgress.Visibility = Visibility.Hidden;
                tbInfoProgress.Visibility = Visibility.Hidden;

            }catch (FormatException)
            {
                System.Windows.MessageBox.Show("Niepoprawny format ścieżki! Nastąpi zakończenie działania programu.", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Error );
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }

        private string GetFullPath(string path, string title, string ext)
        {
            string newTitle = "";
            for (int i = 0; i < title.Length; i++)
            {
                if (title[i].Equals("|") || title[i].Equals("[") || title[i].Equals("]") || title[i].Equals("~") ||
                   title[i].Equals("#") || title[i].Equals("&") || title[i].Equals("*") || title[i].Equals("{") ||
                   title[i].Equals("}") || title[i].Equals(":") || title[i].Equals("<") || title[i].Equals(">") || 
                   title[i].Equals("?") || title[i].Equals("%") || title[i].Equals("*") || title[i].Equals("\""))
                {
                    newTitle += '_';
                }
                else
                {
                    newTitle += title[i];
                }
            }
            return (path + @"\" + newTitle + "." + ext);
        }
    }
}
