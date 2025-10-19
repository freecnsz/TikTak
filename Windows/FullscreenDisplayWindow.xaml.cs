using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using TikTak.Models;
using TikTak.Services;

namespace TikTak.Windows
{
    public partial class FullscreenDisplayWindow : Window
    {
        private readonly TimerModel _timerModel;
        private int _currentScreenIndex;
        private readonly SettingsService _settingsService;
        private AppSettings _settings;
        private string _inputBuffer = "";

        public FullscreenDisplayWindow(TimerModel timerModel, int screenIndex = 0)
        {
            InitializeComponent();
            
            _timerModel = timerModel;
            _currentScreenIndex = screenIndex;
            
            _settingsService = new SettingsService();
            _settings = _settingsService.LoadSettings();

            // Subscribe to timer updates
            _timerModel.PropertyChanged += TimerModel_PropertyChanged;

            // Position on correct screen after window is shown and rendered
            this.Loaded += (s, e) => {
                this.Dispatcher.BeginInvoke(new Action(() => {
                    PositionOnScreen();
                    
                    // CRITICAL: Subscribe to LocationChanged AFTER positioning!
                    // Otherwise it triggers before positioning and resets _currentScreenIndex to 0
                    this.LocationChanged += FullscreenDisplayWindow_LocationChanged;
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            };

            // Setup keyboard and mouse events for input
            this.Focusable = true;
            this.MouseLeftButtonDown += Window_MouseLeftButtonDown;

            // Show initial value
            UpdateDisplay();
        }

        private void PositionOnScreen()
        {
            var screens = System.Windows.Forms.Screen.AllScreens;
            
            if (_currentScreenIndex >= 0 && _currentScreenIndex < screens.Length)
            {
                var screen = screens[_currentScreenIndex];
                var bounds = screen.Bounds;

                // Get DPI scale
                var dpiScale = GetDpiScale();

                // CRITICAL: Must set these BEFORE positioning
                this.WindowStyle = WindowStyle.None;
                this.ResizeMode = ResizeMode.NoResize;
                this.WindowState = WindowState.Normal;
                
                // Calculate position and size in WPF logical units
                var left = bounds.Left / dpiScale;
                var top = bounds.Top / dpiScale;
                var width = bounds.Width / dpiScale;
                var height = bounds.Height / dpiScale;
                
                // Apply position and size
                this.Left = left;
                this.Top = top;
                this.Width = width;
                this.Height = height;
                
                // Ensure window stays on top and in correct position
                this.Topmost = true;
                
                // Force update
                this.UpdateLayout();
            }
        }
        
        private void FullscreenDisplayWindow_LocationChanged(object? sender, EventArgs e)
        {
            // Detect screen changes when window is moved (e.g., via Win+Shift+Arrow)
            var screens = System.Windows.Forms.Screen.AllScreens;
            var dpiScale = GetDpiScale();
            
            for (int i = 0; i < screens.Length; i++)
            {
                var screen = screens[i];
                
                // Calculate window center in physical pixels
                var windowCenterX = (this.Left + this.ActualWidth / 2) * dpiScale;
                var windowCenterY = (this.Top + this.ActualHeight / 2) * dpiScale;
                
                // Check if window center is on this screen
                if (screen.Bounds.Contains((int)windowCenterX, (int)windowCenterY))
                {
                    if (i != _currentScreenIndex)
                    {
                        _currentScreenIndex = i;
                        OnScreenChanged?.Invoke(i);
                        break;
                    }
                }
            }
        }

        private double GetDpiScale()
        {
            var source = System.Windows.PresentationSource.FromVisual(this);
            if (source?.CompositionTarget != null)
            {
                return source.CompositionTarget.TransformToDevice.M11;
            }
            return 1.0;
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
            var (bgColor, fontColor, warningColor) = GetThemeColors();

            if (totalSeconds < 0) // Negative time - Time's up
            {
                // Stop any existing animations
                MainBorder.BeginAnimation(Border.BorderThicknessProperty, null);
                
                // Use warning colors
                TimeDisplay.Foreground = new SolidColorBrush(warningColor);
                MinusSign.Foreground = new SolidColorBrush(warningColor);
                MainBorder.BorderBrush = new SolidColorBrush(warningColor);
                MainBorder.Background = new SolidColorBrush(bgColor);
                
                // Blinking "Time's Up" message
                var elapsedSeconds = Math.Abs((int)totalSeconds);
                var cyclePosition = elapsedSeconds % 4;
                
                if (cyclePosition < 3)
                {
                    // Show time without minus sign in the text (we'll show it separately)
                    var displayText = _timerModel.DisplayTime;
                    if (displayText.StartsWith("-"))
                    {
                        displayText = displayText.Substring(1); // Remove minus from text
                        MinusSign.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MinusSign.Visibility = Visibility.Collapsed;
                    }
                    TimeDisplay.Text = displayText;
                }
                else
                {
                    MinusSign.Visibility = Visibility.Collapsed;
                    TimeDisplay.Text = LanguageManager.GetTimeUpMessage(_settings.Language);
                }
                
                // Border thickness animation
                var borderAnimation = new ThicknessAnimation
                {
                    From = new Thickness(0),
                    To = new Thickness(20),
                    Duration = TimeSpan.FromSeconds(2.0),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };
                MainBorder.BeginAnimation(Border.BorderThicknessProperty, borderAnimation);
            }
            else // Normal time - Progressive warnings
            {
                MinusSign.Visibility = Visibility.Collapsed;
                TimeDisplay.Text = _timerModel.DisplayTime;
                
                // Stop any animations
                MainBorder.BeginAnimation(Border.BorderThicknessProperty, null);
                MainBorder.BorderThickness = new Thickness(0);
                MainBorder.Background = new SolidColorBrush(bgColor);
                
                if (totalSeconds > 300) // More than 5 minutes - Normal
                {
                    TimeDisplay.Foreground = new SolidColorBrush(fontColor);
                }
                else if (totalSeconds > 60) // 1-5 minutes - Yellow warning
                {
                    var yellowColor = _settings.Theme == "Light" || _settings.Theme == "Light Blue" ||
                                      _settings.Theme == "Açık" || _settings.Theme == "Açık Mavi" ? 
                        System.Windows.Media.Color.FromRgb(218, 165, 32) : // Dark yellow for light themes
                        System.Windows.Media.Color.FromRgb(255, 215, 0); // Bright yellow for dark themes
                    TimeDisplay.Foreground = new SolidColorBrush(yellowColor);
                }
                else // Less than 1 minute - Warning color
                {
                    TimeDisplay.Foreground = new SolidColorBrush(warningColor);
                }
            }
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

        public void ApplySettings(AppSettings? newSettings = null)
        {
            if (newSettings != null)
            {
                _settings = newSettings;
            }
            UpdateDisplay();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // Clear input buffer if typing, otherwise close
                if (!string.IsNullOrEmpty(_inputBuffer))
                {
                    _inputBuffer = "";
                    e.Handled = true;
                    return;
                }
                
                // ESC closes fullscreen window
                OnExitFullscreen?.Invoke();
                this.Close();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter && !string.IsNullOrEmpty(_inputBuffer))
            {
                // Enter sets minute value and starts timer (validate 1-999)
                if (int.TryParse(_inputBuffer, out int minutes))
                {
                    if (minutes >= 1 && minutes <= 999)
                    {
                        SetTimerMinutes(minutes);
                    }
                    // Silently ignore invalid values (out of range)
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
                
                // Show input buffer temporarily (optional)
                // Could add visual feedback here
                
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

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Clear input buffer and focus
            _inputBuffer = "";
            this.Focus();
        }

        private void SetTimerMinutes(int minutes)
        {
            // Set minute value and start timer
            OnSetTimeAndStart?.Invoke(minutes);
        }

        protected override void OnClosed(EventArgs e)
        {
            _timerModel.PropertyChanged -= TimerModel_PropertyChanged;
            this.LocationChanged -= FullscreenDisplayWindow_LocationChanged;
            base.OnClosed(e);
        }

        // Event to notify controller that fullscreen was exited
        public event Action? OnExitFullscreen;
        
        // Event to set time and start timer
        public event Action<int>? OnSetTimeAndStart;
        
        // Event to notify when screen changes
        public event Action<int>? OnScreenChanged;
    }
}
