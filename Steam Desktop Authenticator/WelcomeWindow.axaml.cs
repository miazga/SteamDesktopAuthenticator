using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Steam_Desktop_Authenticator
{
    public partial class WelcomeWindow : Window
    {
        private Manifest? _manifest;

        public WelcomeWindow()
        {
            InitializeComponent();
            _manifest = Manifest.GetManifest();
        }

        private async void ImportConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var options = new FolderPickerOpenOptions
                {
                    Title = "Select the folder of your old Steam Desktop Authenticator install"
                };

                var folders = await StorageProvider.OpenFolderPickerAsync(options);
                if (folders.Count > 0)
                {
                    var selectedPath = folders[0].Path.LocalPath;
                    await ImportConfigFromPath(selectedPath);
                }
            }
            catch (Exception ex)
            {
                ShowError("Import Error", ex.Message);
            }
        }

        private async Task ImportConfigFromPath(string path)
        {
            string pathToCopy = null;

            if (Directory.Exists(Path.Combine(path, "maFiles")))
            {
                // User selected the root install dir
                pathToCopy = Path.Combine(path, "maFiles");
            }
            else if (File.Exists(Path.Combine(path, "manifest.json")))
            {
                // User selected the maFiles dir
                pathToCopy = path;
            }
            else
            {
                // Could not find either.
                ShowError("Error", "This folder does not contain either a manifest.json or an maFiles folder.\nPlease select the location where you had Steam Desktop Authenticator installed.");
                return;
            }

            // Copy the contents of the config dir to the new config dir
            string currentPath = Manifest.GetExecutableDir();

            // Create config dir if we don't have it
            string currentMaFilesPath = Path.Combine(currentPath, "maFiles");
            if (!Directory.Exists(currentMaFilesPath))
            {
                Directory.CreateDirectory(currentMaFilesPath);
            }

            // Copy all files from the old dir to the new one
            foreach (string filePath in Directory.GetFiles(pathToCopy, "*.*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(pathToCopy, filePath);
                string destinationPath = Path.Combine(currentMaFilesPath, relativePath);
                
                // Ensure destination directory exists
                string destinationDir = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(destinationDir))
                {
                    Directory.CreateDirectory(destinationDir);
                }
                
                File.Copy(filePath, destinationPath, true);
            }

            // Set first run in manifest
            try
            {
                _manifest = Manifest.GetManifest(true);
                _manifest.FirstRun = false;
                _manifest.Save();
            }
            catch (ManifestParseException)
            {
                // Manifest file was corrupted, generate a new one.
                try
                {
                    ShowInfo("Settings Reset", "Your settings were unexpectedly corrupted and were reset to defaults.");
                    _manifest = Manifest.GenerateNewManifest(true);
                }
                catch (MaFileEncryptedException)
                {
                    // An maFile was encrypted, we're fucked.
                    ShowError("Recovery Failed", "Sorry, but SDA was unable to recover your accounts since you used encryption.\nYou'll need to recover your Steam accounts by removing the authenticator.\nClick OK to view instructions.");
                    OpenUrl(@"https://github.com/Jessecar96/SteamDesktopAuthenticator/wiki/Help!-I'm-locked-out-of-my-account");
                    Close();
                    return;
                }
            }

            // All done!
            ShowInfo("Import Complete", "All accounts and settings have been imported! Click OK to continue.");
            ShowMainWindow();
        }

        private void JustStart_Click(object sender, RoutedEventArgs e)
        {
            // Mark as not first run anymore
            if (_manifest != null)
            {
                _manifest.FirstRun = false;
                _manifest.Save();
            }

            ShowMainWindow();
        }

        private void ShowMainWindow()
        {
            Hide();
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void OpenUrl(string url)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    System.Diagnostics.Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    System.Diagnostics.Process.Start("open", url);
                }
            }
            catch (Exception)
            {
                // Fallback for any platform
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
            }
        }

        private void ShowInfo(string title, string message)
        {
            var window = new Window
            {
                Title = title,
                Content = new TextBlock
                {
                    Text = message,
                    Margin = new Avalonia.Thickness(20),
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                },
                Width = 400,
                Height = 150,
                CanResize = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                // Owner = this
            };
            window.Show();
        }

        private void ShowError(string title, string message)
        {
            var window = new Window
            {
                Title = title,
                Content = new TextBlock
                {
                    Text = message,
                    Margin = new Avalonia.Thickness(20),
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Red)
                },
                Width = 400,
                Height = 150,
                CanResize = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                // Owner = this
            };
            window.Show();
        }
    }
} 