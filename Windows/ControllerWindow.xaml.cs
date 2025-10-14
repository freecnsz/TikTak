using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using Microsoft.Win32;
using TikTak.Models;
using TikTak.Services;

namespace TikTak.Windows
{
    public partial class ControllerWindow : Window
    {
        private readonly TimerModel _timerModel;
        private readonly TimerService _timerService;
        private DisplayWindow? _displayWindow;
        private NotifyIcon? _notifyIcon;
        
        // Windows API for global hotkey
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        
        private const int HOTKEY_ID = 9000;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint VK_T = 0x54; // T key

        public ControllerWindow()
        {
            InitializeComponent();

            _timerModel = new TimerModel();
            _timerService = new TimerService(_timerModel);

            _timerModel.PropertyChanged += TimerModel_PropertyChanged;

            // Position window at top-left
            Loaded += OnWindowLoaded;
            
            // Initial value - 20 minutes
            _timerService.SetTime(20);
            
            ResetPlayPauseButton();
            UpdateExitButtonState();
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Do heavy lifting after window is loaded
            LoadScreens();
            InitializeSystemTray();

            // Listen for display changes
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            
            Left = 50;
            Top = 50;
        }

        private void TimerModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_timerModel.DisplayTime))
            {
                TimeDisplay.Text = _timerModel.DisplayTime;
                UpdateStatus();
            }
            
            UpdateExitButtonState();
        }

        private void UpdateStatus()
        {
            if (_timerModel.IsRunning)
                StatusText.Text = "Çalışıyor • F5: Durdur • F8: Göster • F9: Gizle";
            else
                StatusText.Text = "F5: Başlat • F7: Sıfırla • F8: Göster • F9: Gizle";
        }

        private void UpdateExitButtonState()
        {
            // ExitButton always enabled
            ExitButton.IsEnabled = true;
            
            if (_timerModel.IsRunning)
            {
                ExitButton.Opacity = 1.0;
                ExitButton.ToolTip = "Sayacı Kapat";
            }
            else
            {
                ExitButton.Opacity = 1.0;
                ExitButton.ToolTip = "Sistem Tepsisine Gizle";
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F3:
                    SubtractButton_Click(sender, e);
                    break;
                case Key.F4:
                    AddButton_Click(sender, e);
                    break;
                case Key.F5:
                    PlayPauseButton_Click(sender, e);
                    break;
                case Key.F7:
                    ResetButton_Click(sender, e);
                    break;
                case Key.F8:
                    ShowButton_Click(sender, e);
                    break;
                case Key.F9:
                    HideButton_Click(sender, e);
                    break;
            }
        }

        private void SetTimeButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(MinutesInput.Text, out int minutes) && minutes > 0)
            {
                _timerService.SetTime(minutes);
                StatusText.Text = $"Süre {minutes} dakika olarak ayarlandı";
            }
            else
            {
                System.Windows.MessageBox.Show("Geçerli bir dakika değeri girin (1 veya daha büyük).", "Geçersiz Giriş", MessageBoxButton.OK, MessageBoxImage.Warning);
                StatusText.Text = "Geçersiz giriş";
            }
            
            this.Focus(); // Focus main window
            Keyboard.ClearFocus(); // Clear all focus
        }

        private void MinutesInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Set time on Enter key
            if (e.Key == Key.Enter)
            {
                SetTimeButton_Click(sender, new RoutedEventArgs());
                return;
            }

            // Allow Escape key to clear focus
            if (e.Key == Key.Escape)
            {
                this.Focus(); // Focus main window
                Keyboard.ClearFocus(); // Clear all focus
                return;
            }

            // Allow only numbers and control keys
            bool isNumericKey = (e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9);
            bool isControlKey = e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Left || e.Key == Key.Right || 
                               e.Key == Key.Home || e.Key == Key.End || e.Key == Key.Tab;

            // If not a numeric or control key, suppress the key press
            if (!isNumericKey && !isControlKey)
            {
                e.Handled = true;
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MinutesInput.IsFocused)
            {
                this.Focus();
                Keyboard.ClearFocus();
            }
        }

        private void MinutesInput_LostFocus(object sender, RoutedEventArgs e)
        {

            if (sender is System.Windows.Controls.TextBox textBox)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text) || !int.TryParse(textBox.Text, out int value) || value <= 0)
                {
                    textBox.Text = "20";
                }
                else if (value > 999)
                {
                    textBox.Text = "999";
                }
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_timerModel.IsRunning)
            {
                _timerService.Stop();
                PlayPauseButton.Content = "▶";
                PlayPauseButton.ToolTip = "Başlat";
                PlayPauseButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(39, 174, 96)); // Green
                StatusText.Text = "Sayaç duraklatıldı";
            }
            else
            {
                _timerService.Start();
                PlayPauseButton.Content = "⏸";
                PlayPauseButton.ToolTip = "Duraklat";
                PlayPauseButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 126, 34)); // Orange
                StatusText.Text = "Sayaç çalışıyor";

                // Auto-open display window when started
                if (_displayWindow == null || !_displayWindow.IsLoaded)
                {
                    ShowButton_Click(sender, e);
                }
            }
            
            UpdateExitButtonState();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _timerService.Reset();
            ResetPlayPauseButton();
            UpdateExitButtonState();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            _timerService.AddMinute();
            StatusText.Text = "1 dakika eklendi";
        }

        private void Add5Button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 5; i++)
            {
                _timerService.AddMinute();
            }
            StatusText.Text = "5 dakika eklendi";
        }

        private void SubtractButton_Click(object sender, RoutedEventArgs e)
        {
            _timerService.SubtractMinute();
            StatusText.Text = "1 dakika çıkarıldı";
        }

        private void ShowButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_displayWindow == null || !_displayWindow.IsLoaded)
                {
                    // Get selected screen index
                    var selectedScreenIndex = 0; // Default
                    if (ScreenComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedScreenItem)
                    {
                        if (int.TryParse(selectedScreenItem.Tag?.ToString(), out int screenIndex))
                        {
                            selectedScreenIndex = screenIndex;
                        }
                    }
                    
                    var timerTitle = TitleTextBlock.Text;
                    _displayWindow = new DisplayWindow(_timerModel, selectedScreenIndex, timerTitle);
                    
                    // Update control panel title
                    this.Title = $"TikTak - {timerTitle}";
                    
                    // Listen to DisplayWindow events
                    _displayWindow.OnResetAndClose += () =>
                    {
                        _timerService.Stop();
                        _timerService.Reset();
                        ResetPlayPauseButton();
                    };
                    
                    _displayWindow.OnSetTimeAndStart += (minutes) =>
                    {
                        _timerService.SetTime(minutes);
                        _timerService.Start();
                        PlayPauseButton.Content = "⏸";
                        PlayPauseButton.ToolTip = "Duraklat";
                        PlayPauseButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 126, 34)); // Orange
                        StatusText.Text = $"Sayaç {minutes} dakika ile başlatıldı";
                    };
                    
                    // Apply selected position on first load
                    _displayWindow.Loaded += (s, args) => {
                        if (PositionComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
                        {
                            var position = selectedItem.Tag?.ToString();
                            if (!string.IsNullOrEmpty(position))
                            {
                                _displayWindow.SetPosition(position);
                            }
                        }
                    };
                    
                    _displayWindow.Show();
                    StatusText.Text = "Sayaç ekranda gösteriliyor";
                }
                else
                {
                    _displayWindow.Show();
                    StatusText.Text = "Sayaç ekranda gösteriliyor";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Hata: {ex.Message}";
                System.Windows.MessageBox.Show($"Sayaç ekranı açılırken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HideButton_Click(object sender, RoutedEventArgs e)
        {
            if (_displayWindow != null && _displayWindow.IsLoaded)
            {
                _displayWindow.Hide();
                StatusText.Text = "Sayaç gizlendi";
            }
        }

        private void ResetPlayPauseButton()
        {
            PlayPauseButton.Content = "▶";
            PlayPauseButton.ToolTip = "Başlat";
            PlayPauseButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(39, 174, 96)); // Green
            StatusText.Text = "Hazır";
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            if (_timerModel.IsRunning)
            {
                _timerService.Stop();
                _timerService.Reset();
                
                if (_displayWindow != null)
                {
                    _displayWindow.Close();
                    _displayWindow = null;
                }
                
                ResetPlayPauseButton();
                StatusText.Text = "Sayaç durduruldu ve sıfırlandı";
            }
            else if (_displayWindow != null)
            {
                _displayWindow.Close();
                _displayWindow = null;
                StatusText.Text = "Görüntü penceresi kapatıldı";
            }
            else
            {
                this.Hide();
                StatusText.Text = "Sistem tepsisine gizlendi";
            }
        }

        private void PositionComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_displayWindow != null && _displayWindow.IsLoaded && PositionComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
            {
                var position = selectedItem.Tag?.ToString();
                if (!string.IsNullOrEmpty(position))
                {
                    _displayWindow.SetPosition(position);
                }
            }
        }

        private void LoadScreens()
        {
            ScreenComboBox.Items.Clear();
            
            // Auto-detect system screens
            var screens = System.Windows.Forms.Screen.AllScreens;
            
            for (int i = 0; i < screens.Length; i++)
            {
                var screen = screens[i];
                string screenName;
                string iconEmoji = "🖥️";
                
                if (screen.Primary)
                {
                    screenName = $"{iconEmoji} Ana Ekran ({screen.Bounds.Width}x{screen.Bounds.Height})";
                }
                else
                {
                    screenName = $"{iconEmoji} Ekran {i + 1} ({screen.Bounds.Width}x{screen.Bounds.Height})";
                }
                
                var item = new System.Windows.Controls.ComboBoxItem
                {
                    Content = screenName,
                    Tag = i.ToString() // Store screen index as tag
                };
                
                // Select first screen (usually primary) as default
                if (i == 0)
                {
                    item.IsSelected = true;
                }
                
                ScreenComboBox.Items.Add(item);
            }
        }

        private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() => {
                try
                {
                    int currentlySelectedScreenIndex = 0;
                    if (ScreenComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
                    {
                        if (int.TryParse(selectedItem.Tag?.ToString(), out int index))
                        {
                            currentlySelectedScreenIndex = index;
                        }
                    }

                    LoadScreens();
                    
                    if (currentlySelectedScreenIndex < ScreenComboBox.Items.Count)
                    {
                        ScreenComboBox.SelectedIndex = currentlySelectedScreenIndex;
                    }
                    
                    if (_displayWindow != null && _displayWindow.IsLoaded)
                    {
                        var screens = System.Windows.Forms.Screen.AllScreens;
                        if (currentlySelectedScreenIndex >= screens.Length)
                        {
                            ScreenComboBox.SelectedIndex = 0;
                            _displayWindow.SetScreen(0);
                            
                            if (PositionComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem positionItem)
                            {
                                var position = positionItem.Tag?.ToString();
                                if (!string.IsNullOrEmpty(position))
                                {
                                    _displayWindow.SetPosition(position);
                                }
                            }
                            
                            StatusText.Text = "Ekran değişikliği algılandı - Ana ekrana taşındı";
                        }
                        else
                        {
                            StatusText.Text = "Ekran değişikliği algılandı - Ekranlar güncellendi";
                        }
                    }
                    else
                    {
                        StatusText.Text = "Ekran değişikliği algılandı - Ekran listesi güncellendi";
                    }
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"Ekran güncelleme hatası: {ex.Message}";
                }
            });
        }

        private void TitleArea_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Drag window
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void TitleTextBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // If double-clicked, enter edit mode
            if (e.ClickCount == 2)
            {
                // Hide TextBlock and show TextBox
                TitleTextBlock.Visibility = Visibility.Collapsed;
                TitleTextBox.Visibility = Visibility.Visible;
                TitleTextBox.Focus();
                TitleTextBox.SelectAll();
                e.Handled = true; // Prevent drag event
            }
        }

        private void TitleTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Finish editing when Enter is pressed
                FinishTitleEditing();
            }
            else if (e.Key == Key.Escape)
            {
                // Cancel when Escape is pressed
                TitleTextBox.Text = TitleTextBlock.Text; // Restore original text
                FinishTitleEditing();
            }
        }

        private void TitleTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Finish editing when focus is lost
            FinishTitleEditing();
        }

        private void FinishTitleEditing()
        {
            // Apply new title
            string newTitle = TitleTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(newTitle))
            {
                TitleTextBlock.Text = newTitle;
                this.Title = $"TikTak - {newTitle}";
                
                // Update system tray tooltip
                if (_notifyIcon != null)
                {
                    _notifyIcon.Text = $"TikTak - {newTitle} - Ctrl+Shift+T";
                    
                    // Update context menu title
                    UpdateContextMenuTitle(newTitle);
                }
            }
            
            // Return UI to normal
            TitleTextBox.Visibility = Visibility.Collapsed;
            TitleTextBlock.Visibility = Visibility.Visible;
        }

        private void UpdateContextMenuTitle(string newTitle)
        {
            if (_notifyIcon?.ContextMenuStrip != null && _notifyIcon.ContextMenuStrip.Items.Count > 0)
            {
                var titleItem = _notifyIcon.ContextMenuStrip.Items[0];
                titleItem.Text = newTitle;
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(_timerService, _displayWindow);
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void ScreenComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Reposition DisplayWindow when screen selection changes
            if (_displayWindow != null && _displayWindow.IsLoaded && ScreenComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
            {
                var screenIndexStr = selectedItem.Tag?.ToString();
                if (!string.IsNullOrEmpty(screenIndexStr) && int.TryParse(screenIndexStr, out int screenIndex))
                {
                    // Move DisplayWindow to selected screen
                    _displayWindow.SetScreen(screenIndex);
                    
                    // Adjust position according to current position selection
                    if (PositionComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem positionItem)
                    {
                        var position = positionItem.Tag?.ToString();
                        if (!string.IsNullOrEmpty(position))
                        {
                            _displayWindow.SetPosition(position);
                        }
                    }

                    StatusText.Text = $"Sayaç şu ekrana taşındı: {selectedItem.Content}";
                }
            }
        }

        private void InitializeSystemTray()
        {
            _notifyIcon = new NotifyIcon();
            
            try 
            {
                // Try to load from embedded resource first
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream("TikTak.icon.ico"))
                {
                    if (stream != null)
                    {
                        _notifyIcon.Icon = new System.Drawing.Icon(stream);
                    }
                    else
                    {
                        // Fallback to file system
                        var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
                        if (System.IO.File.Exists(iconPath))
                        {
                            _notifyIcon.Icon = new System.Drawing.Icon(iconPath);
                        }
                        else
                        {
                            _notifyIcon.Icon = SystemIcons.Application;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Icon yükleme hatası: {ex.Message}");
                _notifyIcon.Icon = SystemIcons.Application;
            }
            
            _notifyIcon.Text = $"TikTak - {TitleTextBlock.Text} - Ctrl+Shift+T";
            _notifyIcon.Visible = true;
            
            // Create context menu
            var contextMenu = new ContextMenuStrip();
            
            // Sayaç bilgisi başlığı
            var timerInfoItem = new ToolStripMenuItem(TitleTextBlock.Text)
            {
                Enabled = false,
                Font = new System.Drawing.Font(System.Drawing.SystemFonts.MenuFont!, System.Drawing.FontStyle.Bold)
            };
            
            var showHideItem = new ToolStripMenuItem("Göster/Gizle");
            showHideItem.Click += (s, e) => ToggleWindowVisibility();
            
            var settingsItem = new ToolStripMenuItem("Ayarlar");
            settingsItem.Click += (s, e) => {
                var settingsWindow = new SettingsWindow(_timerService, _displayWindow);
                settingsWindow.Owner = this;
                settingsWindow.ShowDialog();
            };
            
            var aboutItem = new ToolStripMenuItem("Hakkında");
            aboutItem.Click += (s, e) => {
                var aboutWindow = new AboutWindow();
                aboutWindow.Owner = this;
                aboutWindow.ShowDialog();
            };
            
            var exitItem = new ToolStripMenuItem("Çıkış");
            exitItem.Click += (s, e) => ExitApplication();
            
            contextMenu.Items.Add(timerInfoItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(showHideItem);
            contextMenu.Items.Add(settingsItem);
            contextMenu.Items.Add(aboutItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(exitItem);
            
            _notifyIcon.ContextMenuStrip = contextMenu;
            
            // Show/hide with double-click
            _notifyIcon.MouseDoubleClick += (s, e) => ToggleWindowVisibility();
        }

        private void ToggleWindowVisibility()
        {
            if (this.IsVisible && this.WindowState != WindowState.Minimized)
            {
                this.Hide();
            }
            else
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.Activate();
            }
        }

        private void ExitApplication()
        {
            // Remove event listeners
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
            
            // Remove hotkey
            var helper = new System.Windows.Interop.WindowInteropHelper(this);
            UnregisterHotKey(helper.Handle, HOTKEY_ID);
            
            // Stop timer
            _timerService.Stop();
            _timerService.Reset();
            
            // Close display window
            _displayWindow?.Close();
            _displayWindow = null;
            
            // Remove system tray icon
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
            
            // Completely close application
            System.Windows.Application.Current.Shutdown();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Don't show in taskbar
            this.ShowInTaskbar = false;
            
            // Register global hotkey (Ctrl+Shift+T)
            var helper = new System.Windows.Interop.WindowInteropHelper(this);
            const uint MOD_CONTROL = 0x0002;
            const uint MOD_SHIFT = 0x0004;
            const uint VK_T = 0x54;
            
            bool success = RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, VK_T);
            if (success)
            {
                var source = System.Windows.Interop.HwndSource.FromHwnd(helper.Handle);
                source?.AddHook(WndProc);
            }
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // When X button is clicked, don't close window, just hide it
            e.Cancel = true;
            this.Hide();
        }

        // Access to NotifyIcon for TimerService to send notifications
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (id == HOTKEY_ID)
                {
                    // Toggle window visibility
                    if (this.IsVisible)
                    {
                        this.Hide();
                    }
                    else
                    {
                        this.Show();
                        this.Activate();
                    }
                    handled = true;
                }
            }
            
            return IntPtr.Zero;
        }

        public NotifyIcon? GetNotifyIcon()
        {
            return _notifyIcon;
        }
    }
}
