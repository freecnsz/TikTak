using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading.Tasks;
using TikTak.Services;

namespace TikTak.Windows
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsService _settingsService;
        private readonly TimerService? _timerService;
        private DisplayWindow? _displayWindow;
        private AppSettings? _settings;
        private AppSettings? _originalSettings;

        public SettingsWindow(TimerService? timerService = null, DisplayWindow? displayWindow = null)
        {
            InitializeComponent();
            _settingsService = new SettingsService();
            _timerService = timerService;
            _displayWindow = displayWindow;
            Loaded += SettingsWindow_Loaded;
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            _originalSettings = _settingsService.LoadSettings();
            _settings = new AppSettings
            {
                Theme = _originalSettings.Theme,
                BackgroundColor = _originalSettings.BackgroundColor,
                FontColor = _originalSettings.FontColor,
                Language = _originalSettings.Language,
                DesktopNotifications = _originalSettings.DesktopNotifications,
                TimeEndNotification = _originalSettings.TimeEndNotification,
                FiveMinuteWarning = _originalSettings.FiveMinuteWarning,
                OneMinuteWarning = _originalSettings.OneMinuteWarning,
                DisplaySize = _originalSettings.DisplaySize,
                DisplayMargin = _originalSettings.DisplayMargin
            };
            
            // Load themes
            LoadThemes();
            
            // Load languages
            LoadLanguages();
            
            // Set selected theme and language
            SetSelectedTheme(_settings.Theme);
            SetSelectedLanguage(_settings.Language);
            SetSelectedDisplaySize(_settings.DisplaySize);
            SetSelectedDisplayMargin(_settings.DisplayMargin);
            
            DesktopNotificationsCheckBox.IsChecked = _settings.DesktopNotifications;
            TimeEndNotificationCheckBox.IsChecked = _settings.TimeEndNotification;
            FiveMinuteWarningCheckBox.IsChecked = _settings.FiveMinuteWarning;
            OneMinuteWarningCheckBox.IsChecked = _settings.OneMinuteWarning;
        }

        private void LoadThemes()
        {
            ThemeComboBox.Items.Clear();
            
            foreach (var theme in ThemeManager.GetAvailableThemes())
            {
                var item = new System.Windows.Controls.ComboBoxItem
                {
                    Tag = theme.Name,
                    Content = new System.Windows.Controls.StackPanel
                    {
                        Orientation = System.Windows.Controls.Orientation.Horizontal,
                        Children =
                        {
                            new System.Windows.Controls.TextBlock
                            {
                                Text = theme.Icon,
                                FontSize = 14,
                                Margin = new Thickness(0, 0, 8, 0),
                                VerticalAlignment = VerticalAlignment.Center
                            },
                            new System.Windows.Controls.TextBlock
                            {
                                Text = theme.Name,
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        }
                    }
                };
                
                ThemeComboBox.Items.Add(item);
            }
        }

        private void LoadLanguages()
        {
            LanguageComboBox.Items.Clear();
            
            foreach (var language in LanguageManager.GetAvailableLanguages())
            {
                var item = new System.Windows.Controls.ComboBoxItem
                {
                    Tag = language,
                    Content = LanguageManager.GetLanguageDisplayName(language)
                };
                
                LanguageComboBox.Items.Add(item);
            }
        }

        private void SetSelectedLanguage(string languageName)
        {
            foreach (System.Windows.Controls.ComboBoxItem item in LanguageComboBox.Items)
            {
                if (item.Tag?.ToString() == languageName)
                {
                    item.IsSelected = true;
                    break;
                }
            }
        }

        private void SetSelectedTheme(string themeName)
        {
            foreach (System.Windows.Controls.ComboBoxItem item in ThemeComboBox.Items)
            {
                if (item.Tag?.ToString() == themeName)
                {
                    item.IsSelected = true;
                    UpdateThemePreview(themeName);
                    break;
                }
            }
        }



        private System.Windows.Media.Brush GetBrushFromColorName(string colorName)
        {
            return colorName switch
            {
                "Black" => System.Windows.Media.Brushes.Black,
                "DarkBlue" => System.Windows.Media.Brushes.DarkBlue,
                "DarkGreen" => System.Windows.Media.Brushes.DarkGreen,
                "DarkRed" => System.Windows.Media.Brushes.DarkRed,
                "Brown" => System.Windows.Media.Brushes.Brown,
                "White" => System.Windows.Media.Brushes.White,
                "LightBlue" => System.Windows.Media.Brushes.LightBlue,
                "Yellow" => System.Windows.Media.Brushes.Yellow,
                "Orange" => System.Windows.Media.Brushes.Orange,
                "Red" => System.Windows.Media.Brushes.Red,
                "Lime" => System.Windows.Media.Brushes.Lime,
                "Cyan" => System.Windows.Media.Brushes.Cyan,
                _ => System.Windows.Media.Brushes.Black
            };
        }



        private void ThemeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ThemeComboBox?.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
            {
                var themeName = selectedItem.Tag?.ToString() ?? "Dark";
                UpdateThemePreview(themeName);
                
                var theme = ThemeManager.GetThemeByName(themeName);
                if (theme != null && _settings != null)
                {
                    ThemeManager.ApplyThemeToSettings(_settings, themeName);
                }
            }
        }

        private void LanguageComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (LanguageComboBox?.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
            {
                var languageName = selectedItem.Tag?.ToString() ?? "Türkçe";
                if (_settings != null)
                {
                    _settings.Language = languageName;
                }
            }
        }

        private void UpdateThemePreview(string themeName)
        {
            var theme = ThemeManager.GetThemeByName(themeName);
            if (theme != null)
            {
                PreviewBackground.Fill = GetBrushFromColorName(theme.BackgroundColor);
                PreviewText.Foreground = GetBrushFromColorName(theme.FontColor);
                ThemeDescription.Text = theme.Description;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Apply selected theme
                if (ThemeComboBox?.SelectedItem is System.Windows.Controls.ComboBoxItem themeItem)
                {
                    var themeName = themeItem.Tag?.ToString() ?? "Koyu";
                    ThemeManager.ApplyThemeToSettings(_settings!, themeName);
                }
                
                _settings!.DesktopNotifications = DesktopNotificationsCheckBox?.IsChecked == true;
                _settings!.TimeEndNotification = TimeEndNotificationCheckBox?.IsChecked == true;
                _settings!.FiveMinuteWarning = FiveMinuteWarningCheckBox?.IsChecked == true;
                _settings!.OneMinuteWarning = OneMinuteWarningCheckBox?.IsChecked == true;

                _settingsService.SaveSettings(_settings!);
                
                ApplySettingsToDisplayWindow();
                
                ShowToastMessage("Ayarlar kaydedildi", "#27AE60");
            }
            catch (Exception ex)
            {
                ShowToastMessage($"Error: {ex.Message}", "#E74C3C");
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Varsayılan ayarları yükle
            _settings = new AppSettings();
            
            // UI'yı güncelle
            SetSelectedTheme(_settings.Theme);
            DesktopNotificationsCheckBox.IsChecked = _settings.DesktopNotifications;
            TimeEndNotificationCheckBox.IsChecked = _settings.TimeEndNotification;
            FiveMinuteWarningCheckBox.IsChecked = _settings.FiveMinuteWarning;
            OneMinuteWarningCheckBox.IsChecked = _settings.OneMinuteWarning;
            
            ShowToastMessage("Varsayılan ayarlara sıfırlandı (Kaydetmeyi unutmayın!)", "#F39C12");
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_originalSettings != null && _displayWindow != null)
            {
                _displayWindow.ApplySettings(_originalSettings);
            }
            this.Close();
        }
        
        private async void ShowToastMessage(string message, string color = "#27AE60")
        {
            ToastText.Text = message;
            ToastMessage.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color));
            ToastMessage.Visibility = Visibility.Visible;

            await Task.Delay(1500);
            ToastMessage.Visibility = Visibility.Collapsed;
        }
        
        private void ApplySettingsToDisplayWindow()
        {
            if (_displayWindow != null)
            {
                _displayWindow.ApplySettings(_settings);
            }
        }

        private void SetSelectedDisplaySize(string size)
        {
            if (DisplaySizeComboBox?.Items != null)
            {
                foreach (System.Windows.Controls.ComboBoxItem item in DisplaySizeComboBox.Items)
                {
                    if (item.Tag?.ToString() == size)
                    {
                        DisplaySizeComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void SetSelectedDisplayMargin(string margin)
        {
            if (DisplayMarginComboBox?.Items != null)
            {
                foreach (System.Windows.Controls.ComboBoxItem item in DisplayMarginComboBox.Items)
                {
                    if (item.Tag?.ToString() == margin)
                    {
                        DisplayMarginComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void DisplaySizeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_settings != null && DisplaySizeComboBox?.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
            {
                _settings.DisplaySize = selectedItem.Tag?.ToString() ?? "Small";
                ApplySettingsToDisplayWindow();
            }
        }

        private void DisplayMarginComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_settings != null && DisplayMarginComboBox?.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
            {
                _settings.DisplayMargin = selectedItem.Tag?.ToString() ?? "Normal";
                ApplySettingsToDisplayWindow();
            }
        }
    }
}
