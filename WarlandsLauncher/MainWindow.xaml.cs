using System;
using System.Windows;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Collections.Generic;
using System.Windows.Controls;
using Squirrel;

namespace WarlandsLauncher
{
    enum LauncherStatus
    {
        Download,
        ready,
        failed,
        downloadingGame,
        downloadingUpdate,
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region private variables
        private string rootPath;
        private string versionFile;
        private string gameZip;
        private string gameExe;
        private string region;
        private string buildDownloadLink;
        private string versionDownloadLink;
        private string unityRegion;
        private bool isUpdate = false;
        Version _localVersion;
        Version _onlineVersion;
        string remoteUrl;
        private string launcherVersion = "V0.5";
        UpdateManager manager;
        #endregion

        #region static strings
        private static string mightTakeTime = "Might take some time...Do not close the Launcher";
        private static string finalizing = "Finalizing...";
        private static string download = "Download";
        private static string play = "Play";
        private static string updateFailed = "Update Failed - Retry";
        private static string downloadingGame = "Downloading Game";
        private static string downloadingUpdate = "Downloading Update";
        private static string uninstalling = "Uninstalling...";
        private static string wait = "Wait...";
        #endregion

        #region server names
        private const string _asia = "Asia";
        private const string _paris = "Europe - Paris";
        private const string _ireland = "Europe - Ireland";
        private const string _us = "US";
        private const string _southAmerica = "South America";

        #endregion

        private LauncherStatus _status;
        

        internal LauncherStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case LauncherStatus.Download:
                        PlayButton.Content = download;
                        progressBar1.Visibility = Visibility.Hidden;
                        Percentage.Visibility = Visibility.Hidden;
                        Region.IsEnabled = true;
                        Region.IsEditable = true;
                        PlayButton.IsEnabled = true;
                        UninstallPopup.IsEnabled = false;
                        UninstallPopup.Visibility = Visibility.Hidden;
                        break;
                    case LauncherStatus.ready:
                        PlayButton.Content = play;
                        regionTxt.Visibility = Visibility.Visible;
                        Region.Visibility = Visibility.Visible;
                        Region.IsEnabled = true;
                        Region.IsEditable = true;
                        PlayButton.IsEnabled = true;
                        UninstallPopup.IsEnabled = true;
                        UninstallPopup.Visibility = Visibility.Visible;
                        break;
                    case LauncherStatus.failed:
                        PlayButton.Content = updateFailed;
                        regionTxt.Visibility = Visibility.Visible;
                        Region.Visibility = Visibility.Visible;
                        PlayButton.IsEnabled = true;
                        UninstallPopup.IsEnabled = false;
                        UninstallPopup.Visibility = Visibility.Hidden;
                        break;
                    case LauncherStatus.downloadingGame:
                        PlayButton.Content = downloadingGame;
                        regionTxt.Visibility = Visibility.Hidden;
                        Region.Visibility = Visibility.Hidden;
                        PlayButton.IsEnabled = false;
                        UninstallPopup.IsEnabled = false;
                        UninstallPopup.Visibility = Visibility.Hidden;
                        break;
                    case LauncherStatus.downloadingUpdate:
                        PlayButton.Content = downloadingUpdate;
                        regionTxt.Visibility = Visibility.Hidden;
                        Region.Visibility = Visibility.Hidden;
                        PlayButton.IsEnabled = false;
                        UninstallPopup.IsEnabled = false;
                        UninstallPopup.Visibility = Visibility.Hidden;
                        break;
                    default:
                        break;
                }
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            rootPath = Directory.GetCurrentDirectory();
            versionFile = System.IO.Path.Combine(rootPath, "Version.txt");
            gameZip = System.IO.Path.Combine(rootPath, "WarlandPreAlpha.zip");
            gameExe = System.IO.Path.Combine(rootPath, "WarlandPreAlpha", "WarlandsPreAlpha.exe");
            VersionText.Text = launcherVersion;

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            manager = await UpdateManager
                .GitHubUpdateManager(@"https://github.com/meJevin/WPFCoreTest");
        }

        /// <summary>
        /// It will check for updates
        /// </summary>
        private void CheckForUpdates(string region)
        {
            switch (region)
            {
                case _asia:
                    remoteUrl = "https://warland-region-2.s3.ap-south-1.amazonaws.com/";
                    buildDownloadLink = "https://warland-region-2.s3.ap-south-1.amazonaws.com/WarlandPreAlpha.zip";
                    versionDownloadLink = "https://warland-region-2.s3.ap-south-1.amazonaws.com/Version.txt";
                    unityRegion = "asia";
                    break;
                case _paris:
                    remoteUrl = "https://warland-region-test.s3.eu-west-3.amazonaws.com/";
                    buildDownloadLink = "https://warland-region-test.s3.eu-west-3.amazonaws.com/WarlandPreAlpha.zip";
                    versionDownloadLink = "https://warland-region-test.s3.eu-west-3.amazonaws.com/Version.txt";
                    unityRegion = "eu";
                    break;
                case _ireland:
                    remoteUrl = "https://warland-launcher-eu-ireland.s3.eu-west-1.amazonaws.com/";
                    buildDownloadLink = "https://warland-launcher-eu-ireland.s3.eu-west-1.amazonaws.com/WarlandPreAlpha.zip";
                    versionDownloadLink = "https://warland-launcher-eu-ireland.s3.eu-west-1.amazonaws.com/Version.txt";
                    unityRegion = "eu";
                    break;
                case _us:
                    remoteUrl = "https://warlands-main-t2.s3.us-west-1.amazonaws.com/";
                    buildDownloadLink = "https://warlands-main-t2.s3.us-west-1.amazonaws.com/WarlandPreAlpha.zip";
                    versionDownloadLink = "https://warlands-main-t2.s3.us-west-1.amazonaws.com/Version.txt";
                    unityRegion = "us";
                    break;
                case _southAmerica:
                    remoteUrl = "https://warland-launcher-sa-sao-paulo.s3.sa-east-1.amazonaws.com/";
                    buildDownloadLink = "https://warland-launcher-sa-sao-paulo.s3.sa-east-1.amazonaws.com/WarlandPreAlpha.zip";
                    versionDownloadLink = "https://warland-launcher-sa-sao-paulo.s3.sa-east-1.amazonaws.com/Version.txt";
                    unityRegion = "sa";
                    break;
            }

            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));
                _localVersion = localVersion;
                //VersionText.Text = "v" + localVersion.ToString();
                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersion = new Version(webClient.DownloadString(versionDownloadLink));
                    _onlineVersion = onlineVersion;
                    if (onlineVersion.IsDifferentThan(localVersion))
                    {
                        Status = LauncherStatus.Download;
                        isUpdate = true;
                    }
                    else
                    {
                        Percentage.Visibility = Visibility.Hidden;
                        progressBar1.Visibility = Visibility.Hidden;
                        Status = LauncherStatus.ready;
                    }
                }
                catch (Exception ex)
                {
                    Status = LauncherStatus.failed;
                    MessageBox.Show($"Error checking for game updates : {ex}");
                }
            }
            else
            {
                Status = LauncherStatus.Download;
                isUpdate=false;
            }
        }
        
        /// <summary>
        /// It will install the game files
        /// </summary>
        private void InstallGameFiles(bool _isUpdate, Version _onlineVersion)
        {
            Percentage.Visibility = Visibility.Visible;
            progressBar1.Visibility = Visibility.Visible;
            try
            {
                WebClient webClient = new WebClient();
                if (_isUpdate)
                {
                    Status = LauncherStatus.downloadingUpdate;
                    VersionText.Visibility = Visibility.Hidden;
                }
                else
                {
                    Status = LauncherStatus.downloadingGame;
                    _onlineVersion = new Version(webClient.DownloadString(versionDownloadLink));
                }
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
                webClient.DownloadFileAsync(new Uri(buildDownloadLink), gameZip, _onlineVersion);
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                MessageBox.Show($"Error installing game files : {ex}");
            }
        }
        /// <summary>
        /// It will be called when download is complete
        /// </summary>
        private void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                VersionText.Visibility = Visibility.Visible;
                string onlineVersion = ((Version)e.UserState).ToString();
                ZipFile.ExtractToDirectory(gameZip, rootPath, true);
                File.Delete(gameZip);

                File.WriteAllText(versionFile, onlineVersion);
                FileStream fileStream = new FileStream(System.IO.Path.Combine(rootPath, "WarlandPreAlpha", "WarlandsPreAlpha_Data") + "/" + "GameRegion.txt", FileMode.Create);

                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    writer.Write(unityRegion);
                }
                //VersionText.Text = onlineVersion;
                Percentage.Visibility = Visibility.Hidden;
                progressBar1.Visibility = Visibility.Hidden;
                Status = LauncherStatus.ready;
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                MessageBox.Show($"Error finishing download : {ex}");
            }
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (region != null)
                CheckForUpdates(region);
        }
        /// <summary>
        /// It will be called when there is a progress in download
        /// </summary>
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {

            double val = 1073741824;
            DateTime dt1 = DateTime.Now;
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            DateTime dt2 = DateTime.Now;
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            double gbIn = bytesIn / val;
            double totalGb = totalBytes / val;
            //double speed = Math.Round((bytesIn / 1024000) / (dt2 - dt1).TotalSeconds, 2) * 0.0125 ;
            if (percentage <= 99f)
                Percentage.Text = "Downloading... " + percentage.ToString("f2") + "%" + " - " + gbIn.ToString("f2") + " GB" + "/" + totalGb.ToString("f2") + " GB";
            else
            {
                Percentage.Text = mightTakeTime;
                PlayButton.Content = finalizing;
                progressBar1.Visibility = Visibility.Hidden;
            }
            progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());

        }
        /// <summary>
        /// click function of play
        /// </summary>
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (Status == LauncherStatus.Download)
            {
                if (isUpdate)
                {
                    if (_onlineVersion.IsDifferentThan(_localVersion))
                    {
                        PlayButton.Content = wait;
                        InstallGameFiles(true, _onlineVersion);
                    }
                }
                else
                {
                    PlayButton.Content = wait;
                    InstallGameFiles(false, Version.zero);
                }
            }
            if (File.Exists(gameExe) && Status == LauncherStatus.ready)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(gameExe);
                startInfo.WorkingDirectory = System.IO.Path.Combine(rootPath, "WarlandPreAlpha");
                Process.Start(startInfo);

                Close();
            }
            else if (Status == LauncherStatus.failed)
            {
                if (region != null)
                    CheckForUpdates(region);
            }
        }
        /// <summary>
        /// combobox setup for region selection
        /// </summary>
        private void Region_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> data = new List<string>();
            data.Add(_asia);
            data.Add(_paris);
            data.Add(_ireland);
            data.Add(_us);
            data.Add(_southAmerica);

            var Region = sender as ComboBox;
            Region.ItemsSource = data;
            Region.SelectedIndex = 0;
            region = Region.SelectedItem as string;
        }

        private void Region_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Region = sender as ComboBox;
            region = Region.SelectedItem as string;
            switch (region)
            {
                case _asia:
                    unityRegion = "asia";
                    break;
                case _paris:
                    unityRegion = "eu";
                    break;
                case _ireland:
                    unityRegion = "eu";
                    break;
                case _us:
                    unityRegion = "us";
                    break;
                case _southAmerica:
                    unityRegion = "sa";
                    break;
            }
            if (File.Exists(System.IO.Path.Combine(rootPath, "WarlandPreAlpha", "WarlandsPreAlpha_Data", "GameRegion.txt")))
            {
                File.Delete(System.IO.Path.Combine(rootPath, "WarlandPreAlpha", "WarlandsPreAlpha_Data", "GameRegion.txt"));
                FileStream fileStream = new FileStream(System.IO.Path.Combine(rootPath, "WarlandPreAlpha", "WarlandsPreAlpha_Data") + "/" + "GameRegion.txt", FileMode.Create);

                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    writer.Write(unityRegion);
                }
            }
        }
       
        /// <summary>
        /// version comparison logic
        /// </summary>
        struct Version
        {
            internal static Version zero = new Version(0, 0, 0);

            private short major;
            private short minor;
            private short subMinor;

            internal Version(short _major, short _minor, short _subMinor)
            {
                major = _major;
                minor = _minor;
                subMinor = _subMinor;
            }
            internal Version(string _version)
            {
                string[] _versionStrings = _version.Split('.');
                if (_versionStrings.Length != 3)
                {
                    major = 0;
                    minor = 0;
                    subMinor = 0;
                    return;
                }
                major = short.Parse(_versionStrings[0]);
                minor = short.Parse(_versionStrings[1]);
                subMinor = short.Parse(_versionStrings[2]);
            }

            internal bool IsDifferentThan(Version _otherVersion)
            {
                if (major != _otherVersion.major)
                {
                    return true;
                }
                else
                {
                    if (minor != _otherVersion.minor)
                    {
                        return true;
                    }
                    else
                    {
                        if (subMinor != _otherVersion.subMinor)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            public override string ToString()
            {
                return $"{major}.{minor}.{subMinor}";
            }
        }

        private void Uninstall_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(gameExe))
            {
                UninstallPanel.IsOpen = false;
                PlayButton.Content = uninstalling;
                PlayButton.IsEnabled = false;
                Directory.Delete(System.IO.Path.Combine(rootPath, "WarlandPreAlpha"),true);
                File.Delete(versionFile);
                Status = LauncherStatus.Download;
            }
            if (!File.Exists(gameExe))
            {
                UninstallPanel.IsOpen = false;
                Status = LauncherStatus.Download;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            UninstallPanel.IsOpen = false;
            UninstallPopup.Visibility = Visibility.Visible;
            UninstallPopup.IsEnabled = true;
        }

        private void UninstallPopup_Click(object sender, RoutedEventArgs e)
        {
            UninstallPopup.Visibility = Visibility.Hidden;
            UninstallPopup.IsEnabled = false;
            UninstallPanel.IsOpen = true;
        }
    }
}
