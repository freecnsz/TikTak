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
        private readonly int _screenIndex;
        private readonly SettingsService _settingsService;
        private AppSettings _settings;

        public FullscreenDisplayWindow(TimerModel timerModel, int screenIndex = 0)
        {
            InitializeComponent();
            _timerModel = timerModel;
            _screenIndex = screenIndex;
            _settingsService = new SettingsService();
            _settings = _settingsService.LoadSettings();

            // Subscribe to timer updates
            _timerModel.PropertyChanged += TimerModel_PropertyChanged;

            // Position on correct screen after window is loaded
            this.Loaded += (s, e) => PositionOnScreen();

            // Show initial value
            UpdateDisplay();
        }

        private void PositionOnScreen()
        {
            var screens = System.Windows.Forms.Screen.AllScreens;
            if (_screenIndex >= 0 && _screenIndex < screens.Length)
            {
                var screen = screens[_screenIndex];
                var bounds = screen.Bounds;

                // Get DPI scale
                var dpiScale = GetDpiScale();

                // First set to Normal state to position correctly
                this.WindowState = WindowState.Normal;
                
                // Position on selected screen
                this.Left = bounds.Left / dpiScale;
                this.Top = bounds.Top / dpiScale;
                this.Width = bounds.Width / dpiScale;
                this.Height = bounds.Height / dpiScale;
                
                // Now maximize on that screen
                this.WindowState = WindowState.Maximized;
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
                MainBorder.BorderBrush = new SolidColorBrush(warningColor);
                MainBorder.Background = new SolidColorBrush(bgColor);
                
                // Blinking "Time's Up" message
                var elapsedSeconds = Math.Abs((int)totalSeconds);
                var cyclePosition = elapsedSeconds % 4;
                
                if (cyclePosition < 3)
                {
                    TimeDisplay.Text = _timerModel.DisplayTime;
                }
                else
                {
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
                // ESC closes fullscreen window
                OnExitFullscreen?.Invoke();
                this.Close();
                e.Handled = true;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _timerModel.PropertyChanged -= TimerModel_PropertyChanged;
            base.OnClosed(e);
        }

        // Event to notify controller that fullscreen was exited
        public event Action? OnExitFullscreen;
    }
}
