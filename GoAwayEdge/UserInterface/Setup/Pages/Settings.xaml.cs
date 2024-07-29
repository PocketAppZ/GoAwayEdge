﻿using System.Windows;
using System.Windows.Controls;
using GoAwayEdge.Common;

namespace GoAwayEdge.UserInterface.Setup.Pages
{
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class Settings
    {
        public Settings()
        {
            InitializeComponent();

            EdgeChannelBox.Items.Add("Edge Stable");
            EdgeChannelBox.Items.Add("Edge Beta");
            EdgeChannelBox.Items.Add("Edge Dev");
            EdgeChannelBox.Items.Add("Edge Canary");
            EdgeChannelBox.SelectedIndex = 0;
            Configuration.Channel = EdgeChannel.Stable;

            SearchEngineBox.Items.Add("Google");
            SearchEngineBox.Items.Add("Bing");
            SearchEngineBox.Items.Add("DuckDuckGo");
            SearchEngineBox.Items.Add("Yahoo");
            SearchEngineBox.Items.Add("Yandex");
            SearchEngineBox.Items.Add("Ecosia");
            SearchEngineBox.Items.Add("Ask");
            SearchEngineBox.Items.Add("Qwant");
            SearchEngineBox.Items.Add("Perplexity");

            try
            {
                Dispatcher.Invoke(() =>
                {
                    var resourceValue = (string)Application.Current.MainWindow!.FindResource("SettingsSearchEngineCustomItem");
                    SearchEngineBox.Items.Add(!string.IsNullOrEmpty(resourceValue) ? resourceValue : "Custom");
                });
            }
            catch
            {
                SearchEngineBox.Items.Add("Custom");
            }

            if (Configuration.NoEdgeInstalled)
            {
                MsEdgeRemoveStackPanel.IsEnabled = false;
                EdgeStackPanel.IsEnabled = false;
            }

            SearchEngineBox.SelectedIndex = 0;
            Configuration.Search = SearchEngine.Google;
            Configuration.Uninstall = false;
            Configuration.InstallControlPanel = true;
            ControlPanelSwitch.IsChecked = true;
        }

        private void EdgeChannelBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Configuration.Channel = EdgeChannelBox.SelectedIndex switch
            {
                0 => EdgeChannel.Stable,
                1 => EdgeChannel.Beta,
                2 => EdgeChannel.Dev,
                3 => EdgeChannel.Canary,
                _ => EdgeChannel.Stable
            };
        }

        private void SearchEngineBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (SearchEngineBox.SelectedIndex)
            {
                case 0:
                    Configuration.Search = SearchEngine.Google;
                    CustomSearchPanel.Visibility = Visibility.Collapsed;
                    Installer.ContentWindow!.NextBtn.IsEnabled = true;
                    break;
                case 1:
                    Configuration.Search = SearchEngine.Bing;
                    CustomSearchPanel.Visibility = Visibility.Collapsed;
                    Installer.ContentWindow!.NextBtn.IsEnabled = true;
                    break;
                case 2:
                    Configuration.Search = SearchEngine.DuckDuckGo;
                    CustomSearchPanel.Visibility = Visibility.Collapsed;
                    Installer.ContentWindow!.NextBtn.IsEnabled = true;
                    break;
                case 3:
                    Configuration.Search = SearchEngine.Yahoo;
                    CustomSearchPanel.Visibility = Visibility.Collapsed;
                    Installer.ContentWindow!.NextBtn.IsEnabled = true;
                    break;
                case 4:
                    Configuration.Search = SearchEngine.Yandex;
                    CustomSearchPanel.Visibility = Visibility.Collapsed;
                    Installer.ContentWindow!.NextBtn.IsEnabled = true;
                    break;
                case 5:
                    Configuration.Search = SearchEngine.Ecosia;
                    CustomSearchPanel.Visibility = Visibility.Collapsed;
                    Installer.ContentWindow!.NextBtn.IsEnabled = true;
                    break;
                case 6:
                    Configuration.Search = SearchEngine.Ask;
                    CustomSearchPanel.Visibility = Visibility.Collapsed;
                    Installer.ContentWindow!.NextBtn.IsEnabled = true;
                    break;
                case 7:
                    Configuration.Search = SearchEngine.Qwant;
                    CustomSearchPanel.Visibility = Visibility.Collapsed;
                    Installer.ContentWindow!.NextBtn.IsEnabled = true;
                    break;
                case 8:
                    Configuration.Search = SearchEngine.Perplexity;
                    CustomSearchPanel.Visibility = Visibility.Collapsed;
                    Installer.ContentWindow!.NextBtn.IsEnabled = true;
                    break;
                case 9:
                    Configuration.Search = SearchEngine.Custom;
                    CustomSearchPanel.Visibility = Visibility.Visible;
                    if (string.IsNullOrEmpty(Configuration.CustomQueryUrl))
                        Installer.ContentWindow!.NextBtn.IsEnabled = false;
                    break;
            }
        }
        
        private void QueryUrlTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Configuration.CustomQueryUrl = QueryUrlTextBox.Text;
            Installer.ContentWindow!.NextBtn.IsEnabled = Uri.TryCreate(QueryUrlTextBox.Text, UriKind.Absolute, out var uriResult)
                                                         && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private void MsEdgeUninstallSwitch_OnClickUninstallSwitch_OnClick(object sender, RoutedEventArgs e)
        {
            Configuration.UninstallEdge = MsEdgeUninstallSwitch.IsChecked!.Value;
        }

        private void ControlPanelSwitch_OnClick(object sender, RoutedEventArgs e)
        {
            Configuration.InstallControlPanel = ControlPanelSwitch.IsChecked!.Value;
        }
    }
}
