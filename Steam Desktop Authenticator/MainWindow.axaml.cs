using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Layout;

namespace Steam_Desktop_Authenticator
{
    public partial class MainWindow : Window
    {
        private Manifest? _manifest;
        private string? _encryptionKey;
        private bool _silent;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void SetEncryptionKey(string? encryptionKey)
        {
            _encryptionKey = encryptionKey;
        }

        public void StartSilent(bool silent)
        {
            _silent = silent;
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            LoadAccounts();
        }

        private void LoadAccounts()
        {
            try
            {
                _manifest = Manifest.GetManifest();
                RefreshAccountList();
            }
            catch (Exception ex)
            {
                ShowError("Error loading accounts", ex.Message);
            }
        }

        private void RefreshAccountList()
        {
            if (_manifest?.Entries == null) return;

            var accountsPanel = this.FindControl<StackPanel>("AccountsPanel");
            if (accountsPanel == null) return;

            accountsPanel.Children.Clear();

            foreach (var entry in _manifest.Entries)
            {
                var accountCard = CreateAccountCard(entry);
                accountsPanel.Children.Add(accountCard);
            }
        }

        private Border CreateAccountCard(Manifest.ManifestEntry entry)
        {
            var card = new Border
            {
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.LightGray),
                BorderThickness = new Avalonia.Thickness(1),
                CornerRadius = new Avalonia.CornerRadius(8),
                Padding = new Avalonia.Thickness(15),
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Account info
            var accountInfo = new StackPanel();
            var accountName = new TextBlock
            {
                Text = entry.SteamID.ToString(),
                FontWeight = Avalonia.Media.FontWeight.Bold,
                FontSize = 16
            };
            var steamId = new TextBlock
            {
                Text = $"Steam ID: {entry.SteamID}",
                FontSize = 12,
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Gray)
            };
            accountInfo.Children.Add(accountName);
            accountInfo.Children.Add(steamId);

            // Buttons
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };
            var copyCodeButton = new Button { Content = "Copy Code", Width = 100 };
            var confirmationsButton = new Button { Content = "Confirmations", Width = 100 };
            var removeButton = new Button { Content = "Remove", Width = 80 };

            copyCodeButton.Click += (s, e) => CopyCode(entry);
            confirmationsButton.Click += (s, e) => ShowConfirmations(entry);
            removeButton.Click += (s, e) => RemoveAccount(entry);

            buttonPanel.Children.Add(copyCodeButton);
            buttonPanel.Children.Add(confirmationsButton);
            buttonPanel.Children.Add(removeButton);

            Grid.SetColumn(accountInfo, 0);
            Grid.SetColumn(buttonPanel, 1);

            grid.Children.Add(accountInfo);
            grid.Children.Add(buttonPanel);

            card.Child = grid;
            return card;
        }

        private void CopyCode(Manifest.ManifestEntry entry)
        {
            try
            {
                // This would need to be implemented to generate the actual Steam Guard code
                // For now, just show a placeholder
                ShowInfo("Copy Code", "Steam Guard code copying functionality needs to be implemented.");
            }
            catch (Exception ex)
            {
                ShowError("Error copying code", ex.Message);
            }
        }

        private void ShowConfirmations(Manifest.ManifestEntry entry)
        {
            try
            {
                // This would need to be implemented to show trade confirmations
                ShowInfo("Confirmations", "Trade confirmations functionality needs to be implemented.");
            }
            catch (Exception ex)
            {
                ShowError("Error showing confirmations", ex.Message);
            }
        }

        private void RemoveAccount(Manifest.ManifestEntry entry)
        {
            try
            {
                if (_manifest?.Entries != null)
                {
                    _manifest.Entries.Remove(entry);
                    _manifest.Save();
                    RefreshAccountList();
                    ShowInfo("Account Removed", "The account has been removed successfully.");
                }
            }
            catch (Exception ex)
            {
                ShowError("Error removing account", ex.Message);
            }
        }

        private void AddAccount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // This would need to be implemented to show the account addition form
                ShowInfo("Add Account", "Account addition functionality needs to be implemented.");
            }
            catch (Exception ex)
            {
                ShowError("Error adding account", ex.Message);
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // This would need to be implemented to show the settings form
                ShowInfo("Settings", "Settings functionality needs to be implemented.");
            }
            catch (Exception ex)
            {
                ShowError("Error opening settings", ex.Message);
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