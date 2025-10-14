using System.ComponentModel;

namespace TikTak.Models
{
    public class TimerModel : INotifyPropertyChanged
    {
        private TimeSpan _totalTime;
        private TimeSpan _remainingTime;
        private bool _isRunning;

        public TimeSpan TotalTime
        {
            get => _totalTime;
            set
            {
                _totalTime = value;
                OnPropertyChanged(nameof(TotalTime));
            }
        }

        public TimeSpan RemainingTime
        {
            get => _remainingTime;
            set
            {
                _remainingTime = value;
                OnPropertyChanged(nameof(RemainingTime));
                OnPropertyChanged(nameof(DisplayTime));
            }
        }

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        public string DisplayTime 
        { 
            get 
            {
                TimeSpan timeToShow;
                bool isNegative = RemainingTime.TotalSeconds < 0;
                
                if (isNegative)
                {
                    timeToShow = TimeSpan.FromSeconds(Math.Abs(RemainingTime.TotalSeconds));
                }
                else
                {
                    timeToShow = RemainingTime;
                }
                
                string timeString;
                if (timeToShow.TotalHours >= 1)
                {
                    // Hours:Minutes:Seconds format
                    timeString = $"{(int)timeToShow.TotalHours:D2}:{timeToShow.Minutes:D2}:{timeToShow.Seconds:D2}";
                }
                else
                {
                    // Minutes:Seconds format
                    timeString = $"{timeToShow.Minutes:D2}:{timeToShow.Seconds:D2}";
                }
                
                return isNegative ? $"- {timeString}" : timeString;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}