using System.Windows.Threading;
using System.Windows.Forms;
using TikTak.Models;

namespace TikTak.Services
{
    public class TimerService
    {
        private readonly TimerModel _timerModel;
        private readonly DispatcherTimer _timer;
        private readonly SettingsService _settingsService;
        private AppSettings _settings;
        private bool _fiveMinuteWarningShown = false;
        private bool _oneMinuteWarningShown = false;
        private bool _timeEndNotificationShown = false;

        public TimerService(TimerModel timerModel)
        {
            _timerModel = timerModel;
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            
            _settingsService = new SettingsService();
            _settings = _settingsService.LoadSettings();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _timerModel.RemainingTime = _timerModel.RemainingTime.Subtract(TimeSpan.FromSeconds(1));
            
            // Refresh notification settings
            _settings = _settingsService.LoadSettings();
            
            // 5 minute warning
            if (_settings.FiveMinuteWarning && !_fiveMinuteWarningShown && 
                _timerModel.RemainingTime.TotalMinutes <= 5 && _timerModel.RemainingTime.TotalMinutes > 4.98)
            {
                _fiveMinuteWarningShown = true;
                ShowNotification("Son 5 Dakika", "Sürenin bitmesine 5 dakika kaldı!", ToolTipIcon.None);
            }
            
            // 1 minute warning
            if (_settings.OneMinuteWarning && !_oneMinuteWarningShown && 
                _timerModel.RemainingTime.TotalMinutes <= 1 && _timerModel.RemainingTime.TotalMinutes > 0.98)
            {
                _oneMinuteWarningShown = true;
                ShowNotification("Son 1 Dakika", "Sürenin bitmesine 1 dakika kaldı!", ToolTipIcon.None);
            }
            
            // When time reaches zero
            if (_timerModel.RemainingTime.TotalSeconds <= 0 && !_timeEndNotificationShown)
            {
                _timeEndNotificationShown = true;
                
                if (_settings.TimeEndNotification)
                {
                    ShowNotification("Süre Doldu!", "Timer süresi tamamlandı.", ToolTipIcon.None);
                }
            }
        }

        public void Start()
        {
            _timerModel.IsRunning = true;
            _timer.Start();
        }

        public void Stop()
        {
            _timerModel.IsRunning = false;
            _timer.Stop();
        }

        public void Reset()
        {
            Stop();
            _timerModel.RemainingTime = _timerModel.TotalTime;
            ResetWarningFlags();
        }

        public void AddMinute()
        {
            _timerModel.RemainingTime = _timerModel.RemainingTime.Add(TimeSpan.FromMinutes(1));
            _timerModel.TotalTime = _timerModel.TotalTime.Add(TimeSpan.FromMinutes(1));
            ResetWarningFlags();
        }

        public void SubtractMinute()
        {
            if (_timerModel.RemainingTime.TotalMinutes >= 1)
            {
                _timerModel.RemainingTime = _timerModel.RemainingTime.Subtract(TimeSpan.FromMinutes(1));
                _timerModel.TotalTime = _timerModel.TotalTime.Subtract(TimeSpan.FromMinutes(1));
                ResetWarningFlags();
            }
        }

        public void SetTime(int minutes)
        {
            var time = TimeSpan.FromMinutes(minutes);
            _timerModel.TotalTime = time;
            _timerModel.RemainingTime = time;
            ResetWarningFlags();
        }

        private void ShowNotification(string title, string text, ToolTipIcon icon)
        {
            if (!_settings.DesktopNotifications) return;
            
            // Use static reference to access NotifyIcon from ControllerWindow
            var notifyIcon = GetNotifyIcon();
            if (notifyIcon != null)
            {
                notifyIcon.ShowBalloonTip(1500, title, text, icon);
            }
        }
        
        // Static method to access NotifyIcon
        private static NotifyIcon? GetNotifyIcon()
        {
            // Check all open windows
            foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
            {
                if (window is TikTak.Windows.ControllerWindow controller)
                {
                    return controller.GetNotifyIcon();
                }
            }
            return null;
        }
        
        private void ResetWarningFlags()
        {
            _fiveMinuteWarningShown = false;
            _oneMinuteWarningShown = false;
            _timeEndNotificationShown = false;
        }
    }
}