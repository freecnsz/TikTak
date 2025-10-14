using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TikTak.Models;
using TikTak.Services;

namespace TikTak.Windows
{
    public partial class DisplayWindow : Window
    {
        private readonly TimerModel _timerModel;
        private int _currentScreenIndex = 0;
        private string _inputBuffer = "";
        private readonly SettingsService _settingsService;
        private AppSettings _settings;
        private string _currentPosition = "TopLeft";

        public DisplayWindow(TimerModel timerModel, int screenIndex = 0, string title = "TikTak Sayaç")
        {
            InitializeComponent();
            _timerModel = timerModel;
            _currentScreenIndex = screenIndex;
            _settingsService = new SettingsService();
            _settings = _settingsService.LoadSettings();
            
            // Set window title
            this.Title = title;
            
            // Apply settings to UI
            ApplySettings();
            
            _timerModel.PropertyChanged += TimerModel_PropertyChanged;
            
            // Show initial value
            UpdateDisplay();
            
            // Keep window on top
            Topmost = true;
            
            // Setup keyboard events
            this.Focusable = true;
            this.KeyDown += DisplayWindow_KeyDown;
            this.MouseLeftButtonDown += DisplayWindow_MouseLeftButtonDown;
        }

        private void ApplySettings()
        {
            // Set background color
            var backgroundColor = GetBrushFromColorName(_settings.BackgroundColor);
            MainBorder.Background = backgroundColor;
            
            // Apply size and padding settings
            ApplySizeAndPadding();
        }
        
        public void ApplySettings(AppSettings? newSettings = null)
        {
            if (newSettings != null)
            {
                _settings = newSettings;
            }
            
            // Set background color
            var backgroundColor = GetBrushFromColorName(_settings.BackgroundColor);
            MainBorder.Background = backgroundColor;
            
            // Apply size and padding settings
            ApplySizeAndPadding();
            
            // Update display to apply font color changes
            UpdateDisplay();
        }

        private void ApplySizeAndPadding()
        {
            // Size settings
            switch (_settings.DisplaySize)
            {
                case "Small":
                    this.Width = 120;
                    this.Height = 40;
                    TimeDisplay.FontSize = 16;
                    MainBorder.CornerRadius = new CornerRadius(6);
                    TimeDisplay.Margin = new Thickness(4, 2, 4, 2);
                    break;
                case "Medium":
                    this.Width = 150;
                    this.Height = 50;
                    TimeDisplay.FontSize = 20;
                    MainBorder.CornerRadius = new CornerRadius(8);
                    TimeDisplay.Margin = new Thickness(6, 3, 6, 3);
                    break;
                case "Large":
                    this.Width = 180;
                    this.Height = 60;
                    TimeDisplay.FontSize = 24;
                    MainBorder.CornerRadius = new CornerRadius(10);
                    TimeDisplay.Margin = new Thickness(8, 4, 8, 4);
                    break;
            }
            
            SetPosition(_currentPosition);
        }

        private System.Windows.Media.Brush GetBrushFromColorName(string colorName)
        {
            return colorName switch
            {
                "Black" => System.Windows.Media.Brushes.Black,
                "DarkBlue" => new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 139)),
                "DarkGreen" => new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 100, 0)),
                "DarkRed" => new SolidColorBrush(System.Windows.Media.Color.FromRgb(139, 0, 0)),
                "Brown" => new SolidColorBrush(System.Windows.Media.Color.FromRgb(139, 69, 19)),
                "White" => System.Windows.Media.Brushes.White,
                "LightBlue" => new SolidColorBrush(System.Windows.Media.Color.FromRgb(173, 216, 230)),
                "Yellow" => System.Windows.Media.Brushes.Yellow,
                "Orange" => System.Windows.Media.Brushes.Orange,
                "Red" => System.Windows.Media.Brushes.Red,
                "Lime" => System.Windows.Media.Brushes.Lime,
                "Cyan" => System.Windows.Media.Brushes.Cyan,
                _ => System.Windows.Media.Brushes.Black
            };
        }

        private (System.Windows.Media.Color bgColor, System.Windows.Media.Color fontColor, System.Windows.Media.Color warningColor) GetThemeColors()
        {
            return _settings.Theme switch
            {
                "Koyu" => (System.Windows.Media.Color.FromRgb(0, 0, 0), System.Windows.Media.Color.FromRgb(255, 255, 255), System.Windows.Media.Color.FromRgb(220, 20, 60)),
                "Açık" => (System.Windows.Media.Color.FromRgb(255, 255, 255), System.Windows.Media.Color.FromRgb(0, 0, 0), System.Windows.Media.Color.FromRgb(178, 34, 34)),
                "Koyu Mavi" => (System.Windows.Media.Color.FromRgb(0, 0, 139), System.Windows.Media.Color.FromRgb(255, 255, 0), System.Windows.Media.Color.FromRgb(255, 69, 0)),
                "Açık Mavi" => (System.Windows.Media.Color.FromRgb(173, 216, 230), System.Windows.Media.Color.FromRgb(25, 25, 112), System.Windows.Media.Color.FromRgb(139, 0, 0)), 
                "Yeşil" => (System.Windows.Media.Color.FromRgb(0, 100, 0), System.Windows.Media.Color.FromRgb(255, 255, 255), System.Windows.Media.Color.FromRgb(255, 69, 0)),
                "Dark" => (System.Windows.Media.Color.FromRgb(0, 0, 0), System.Windows.Media.Color.FromRgb(255, 255, 255), System.Windows.Media.Color.FromRgb(220, 20, 60)),
                "Light" => (System.Windows.Media.Color.FromRgb(255, 255, 255), System.Windows.Media.Color.FromRgb(0, 0, 0), System.Windows.Media.Color.FromRgb(178, 34, 34)),
                "Dark Blue" => (System.Windows.Media.Color.FromRgb(0, 0, 139), System.Windows.Media.Color.FromRgb(255, 255, 0), System.Windows.Media.Color.FromRgb(255, 69, 0)),
                "Light Blue" => (System.Windows.Media.Color.FromRgb(173, 216, 230), System.Windows.Media.Color.FromRgb(25, 25, 112), System.Windows.Media.Color.FromRgb(139, 0, 0)),
                "Green" => (System.Windows.Media.Color.FromRgb(0, 100, 0), System.Windows.Media.Color.FromRgb(255, 255, 255), System.Windows.Media.Color.FromRgb(255, 69, 0)),
                _ => (System.Windows.Media.Color.FromRgb(0, 0, 139), System.Windows.Media.Color.FromRgb(255, 255, 0), System.Windows.Media.Color.FromRgb(255, 69, 0)) 
            };
        }

        private void TimerModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_timerModel.DisplayTime) || e.PropertyName == nameof(_timerModel.RemainingTime))
            {
                UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            var totalSeconds = _timerModel.RemainingTime.TotalSeconds;
            
            if (totalSeconds < 0) // Negative time - Border animation
            {
                // Stop any existing animations
                MainBorder.BeginAnimation(System.Windows.Controls.Border.BorderThicknessProperty, null);
                MainBorder.BeginAnimation(OpacityProperty, null);
                MainBorder.Opacity = 0.85;
                
                // Use warning colors for negative time
                var (bgColor, fontColor, warningColor) = GetThemeColors();
                TimeDisplay.Foreground = new SolidColorBrush(warningColor);
                MainBorder.BorderBrush = new SolidColorBrush(warningColor);
                
                // Blinking "Time's Up" message
                var elapsedSeconds = Math.Abs((int)totalSeconds);
                var cyclePosition = elapsedSeconds % 4;
                
                if (cyclePosition < 3) // İlk 3 saniye normal süre göster
                {
                    TimeDisplay.Text = _timerModel.DisplayTime;
                }
                else // Son 1 saniye dil ayarına göre mesaj göster
                {
                    TimeDisplay.Text = LanguageManager.GetTimeUpMessage(_settings.Language);
                }
                
                // Border thickness animation - more subtle
                var borderAnimation = new System.Windows.Media.Animation.ThicknessAnimation
                {
                    From = new Thickness(2),
                    To = new Thickness(6),
                    Duration = TimeSpan.FromSeconds(2.0),
                    AutoReverse = true,
                    RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever
                };
                MainBorder.BeginAnimation(System.Windows.Controls.Border.BorderThicknessProperty, borderAnimation);
            }
            else // Normal time - Use theme colors with progressive warnings
            {
                TimeDisplay.Text = _timerModel.DisplayTime;
                
                // Stop any animations
                MainBorder.BeginAnimation(System.Windows.Controls.Border.BorderThicknessProperty, null);
                MainBorder.BeginAnimation(OpacityProperty, null);
                MainBorder.Opacity = 0.85;
                MainBorder.BorderThickness = new Thickness(2);
                
                var (bgColor, fontColor, warningColor) = GetThemeColors();
                
                if (totalSeconds > 300) // More than 5 minutes - Normal theme colors
                {
                    TimeDisplay.Foreground = new SolidColorBrush(fontColor);
                    MainBorder.BorderBrush = new SolidColorBrush(fontColor);
                }
                else if (totalSeconds > 60) // 1-5 minutes - Yellow warning (theme compatible)
                {
                    var yellowColor = _settings.Theme == "Light" || _settings.Theme == "Light Blue" ? 
                        System.Windows.Media.Color.FromRgb(218, 165, 32) : // Dark yellow for light themes
                        System.Windows.Media.Color.FromRgb(255, 215, 0); // Bright yellow for dark themes
                    TimeDisplay.Foreground = new SolidColorBrush(yellowColor);
                    MainBorder.BorderBrush = new SolidColorBrush(yellowColor);
                }
                else // Less than 1 minute - Warning color
                {
                    TimeDisplay.Foreground = new SolidColorBrush(warningColor);
                    MainBorder.BorderBrush = new SolidColorBrush(warningColor);
                }
            }
        }

        public void SetScreen(int screenIndex)
        {
            _currentScreenIndex = screenIndex;
            var screens = System.Windows.Forms.Screen.AllScreens;
            
            if (screenIndex >= 0 && screenIndex < screens.Length)
            {
                var screen = screens[screenIndex];
                var workingArea = screen.WorkingArea;
                
                // Use default values if window size not yet determined
                var windowWidth = ActualWidth > 0 ? ActualWidth : 130;
                var windowHeight = ActualHeight > 0 ? ActualHeight : 45;
                
                // Center of screen
                Left = workingArea.Left + (workingArea.Width - windowWidth) / 2;
                Top = workingArea.Top + (workingArea.Height - windowHeight) / 2;
                
                // Fix position if outside screen bounds
                if (Left < workingArea.Left) Left = workingArea.Left + 20;
                if (Top < workingArea.Top) Top = workingArea.Top + 20;
                if (Left + windowWidth > workingArea.Right) Left = workingArea.Right - windowWidth - 20;
                if (Top + windowHeight > workingArea.Bottom) Top = workingArea.Bottom - windowHeight - 20;
            }
        }

        public void SetPosition(string? position)
        {
            if (string.IsNullOrEmpty(position)) return;
            
            _currentPosition = position;
            
            var screens = System.Windows.Forms.Screen.AllScreens;
            if (_currentScreenIndex >= 0 && _currentScreenIndex < screens.Length)
            {
                var screen = screens[_currentScreenIndex];
                var workingArea = screen.WorkingArea;
                
                // Window dimensions
                var windowWidth = ActualWidth > 0 ? ActualWidth : 130;
                var windowHeight = ActualHeight > 0 ? ActualHeight : 45;
                
                // Get margin from settings
                int margin = GetMarginPixels();
                
                switch (position)
                {
                    case "TopLeft":
                        Left = workingArea.Left + margin;
                        Top = workingArea.Top + margin;
                        break;
                    case "TopCenter":
                        Left = workingArea.Left + (workingArea.Width - windowWidth) / 2;
                        Top = workingArea.Top + margin;
                        break;
                    case "TopRight":
                        Left = workingArea.Right - windowWidth - margin;
                        Top = workingArea.Top + margin;
                        break;
                    case "MiddleLeft":
                        Left = workingArea.Left + margin;
                        Top = workingArea.Top + (workingArea.Height - windowHeight) / 2;
                        break;
                    case "Center":
                        Left = workingArea.Left + (workingArea.Width - windowWidth) / 2;
                        Top = workingArea.Top + (workingArea.Height - windowHeight) / 2;
                        break;
                    case "MiddleRight":
                        Left = workingArea.Right - windowWidth - margin;
                        Top = workingArea.Top + (workingArea.Height - windowHeight) / 2;
                        break;
                    case "BottomLeft":
                        Left = workingArea.Left + margin;
                        Top = workingArea.Bottom - windowHeight - margin;
                        break;
                    case "BottomCenter":
                        Left = workingArea.Left + (workingArea.Width - windowWidth) / 2;
                        Top = workingArea.Bottom - windowHeight - margin;
                        break;
                    case "BottomRight":
                        Left = workingArea.Right - windowWidth - margin;
                        Top = workingArea.Bottom - windowHeight - margin;
                        break;
                }
            }
        }

        private int GetMarginPixels()
        {
            return _settings.DisplayMargin switch
            {
                "Close" => 10,
                "Normal" => 20,
                "Far" => 40,
                _ => 20
            };
        }

        private void DisplayWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Clear input buffer and focus
            _inputBuffer = "";
            this.Focus();
            
            // Drag window
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void DisplayWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // ESC closes and resets timer (like Exit button)
                ResetAndCloseTimer();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter && !string.IsNullOrEmpty(_inputBuffer))
            {
                // Enter sets minute value and starts timer
                if (int.TryParse(_inputBuffer, out int minutes) && minutes > 0)
                {
                    SetTimerMinutes(minutes);
                }
                

                _inputBuffer = "";
                e.Handled = true;
                return;
            }

            // Accept only numbers (for minute input)
            if ((e.Key >= Key.D0 && e.Key <= Key.D9) || 
                (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {

                
                string digit = "";
                if (e.Key >= Key.D0 && e.Key <= Key.D9)
                    digit = ((int)(e.Key - Key.D0)).ToString();
                else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                    digit = ((int)(e.Key - Key.NumPad0)).ToString();

                _inputBuffer += digit;
                // Don't call UpdateDisplay to keep input hidden
                e.Handled = true;
            }
            else if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                // Backspace deletes last digit
                if (_inputBuffer.Length > 0)
                {
                    _inputBuffer = _inputBuffer.Substring(0, _inputBuffer.Length - 1);
                }
                e.Handled = true;
            }
        }

        private void SetTimerMinutes(int minutes)
        {
            // Set minute value and start timer
            _timerModel.TotalTime = TimeSpan.FromMinutes(minutes);
            _timerModel.RemainingTime = _timerModel.TotalTime;
            
            // Start timer (using TimerService)
            // Need access to timer service in main control panel
            // Send event to main window
            OnSetTimeAndStart?.Invoke(minutes);
        }

        private void ResetAndCloseTimer()
        {
            // Stop and reset timer, close window
            OnResetAndClose?.Invoke();
            this.Close();
        }



        protected override void OnClosed(EventArgs e)
        {
            _timerModel.PropertyChanged -= TimerModel_PropertyChanged;
            base.OnClosed(e);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            // Ensure window stays on top when activated
            Topmost = true;
        }

        // Events for communicating with controller (no longer used but kept for compatibility)
        public event Action? OnResetAndClose;
        public event Action<int>? OnSetTimeAndStart;
    }
}