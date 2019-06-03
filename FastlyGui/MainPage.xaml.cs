using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using FastlyLogs;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FastlyGui
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Scanner scan;
        public MainPage()
        {
            this.InitializeComponent();
            GridResults.Visibility = Visibility.Collapsed;
            BtnScan.IsEnabled = false;
        }
        private void BtnAddSearchTerm_Click(Object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(TxtSearchTerm.Text))
            {
                LstSearchTerms.Items.Add(TxtSearchTerm.Text);
            }
            
            TxtSearchTerm.Text = string.Empty;
        }

        private void BtnRemoveSearchTerm_Click(object sender, RoutedEventArgs e)
        {
            if(LstSearchTerms.SelectedItem != null)
            {
                LstSearchTerms.Items.Remove(LstSearchTerms.SelectedItem);
            }
        }

        public async Task<int> UpdateProgress()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PrgBar.Value = (double)scan.ScannedFiles / scan.TotalFiles;
            });
            
            return 0;
        }

        public async void DisplayResults()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ToggleInitialParameters();
                GridResults.Visibility = Visibility.Visible;

                //Top level stats
                LblProjectRequests.Text = $"Total Requests To Project {scan.ProjectGuid}:";
                TxtProjectRequests.Text = scan.ProjectTotalRequests.ToString();

                LblTotalFiles.Text = "Total Log Files Scanned:";
                TxtTotalFiles.Text = scan.ScannedFiles.ToString();

                LblTotalRequests.Text = "Total Requests Scanned:";
                TxtTotalRequests.Text = scan.ScannedLines.ToString();

                //status codes tally
                TxtStatusCodesLeft.Text = "200:";
                TxtStatusCodesRight.Text = (scan.ProjectTotalRequests - scan.NotOKs.Count).ToString();
                var otherstatuses = scan.NotOKs.Select(error => error.StatusCode).Distinct();
                foreach(string status in otherstatuses)
                {
                    TxtStatusCodesLeft.Text += $"\r\n{status}:";
                    TxtStatusCodesRight.Text += $"\r\n{scan.NotOKs.Where(error => error.StatusCode == status).Count()}";
                }

                //search terms tally
                LblSearchTermsTerm.Text = "TERM";
                LblSearchTermsUrl.Text = "URL Occurances";
                LblSearchTermsQuery.Text = "QUERY Occurances";

                foreach (var term in scan.SearchTerms)
                {
                    LblSearchTermsTerm.Text += $"\r\n{term.Text}";
                    LblSearchTermsUrl.Text += $"\r\n{term.UrlOccurances}";
                    LblSearchTermsQuery.Text += $"\r\n{term.QueryStringOccurances}";
                }

                foreach(var error in scan.NotOKs)
                {
                    LstBadRequests.Items.Add(error.RequestData.ToString());
                }
            });
        }

        private void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            ToggleInitialParameters();
            PrgBar.Visibility = Visibility.Visible;
            scan = new Scanner
            {
                ScanDirectory = TxtDirectory.Text,
                ProjectGuid = TxtProjectID.Text
            };
            foreach(string term in LstSearchTerms.Items)
            {
                scan.SearchTerms.Add(new SearchTerm(term));
            }
            Task.Run(() => ScanAndUpdate()); 
        }

        public void ScanAndUpdate()
        {
            scan.Scan(UpdateProgress);
            DisplayResults();
        }

        private async void BtnFolderPicker_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                TxtDirectory.Text = folder.Path;
                ValidateScanButton();
            }
        }
        public void ValidateScanButton()
        {
            if(string.IsNullOrEmpty(TxtProjectID.Text) || string.IsNullOrEmpty(TxtDirectory.Text))
            {
                BtnScan.IsEnabled = false;
            }
            else
            {
                BtnScan.IsEnabled = true;
            }
        }

        public void ToggleInitialParameters()
        {
            BtnFolderPicker.IsEnabled = !BtnFolderPicker.IsEnabled;
            TxtProjectID.IsEnabled = !TxtProjectID.IsEnabled;
            LstSearchTerms.IsEnabled = !LstSearchTerms.IsEnabled;
            TxtSearchTerm.IsEnabled = !TxtSearchTerm.IsEnabled;
            BtnAddSearchTerm.IsEnabled = !BtnAddSearchTerm.IsEnabled;
            BtnRemoveSearchTerm.IsEnabled = !BtnRemoveSearchTerm.IsEnabled;
        }

        private void TxtProjectID_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateScanButton();
        }

        
    }
}
