using System.IO;
using System.Text.Json;

namespace TikTak.Services
{
    public class SettingsService
    {
        private readonly string _settingsPath;

        public SettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var tikTakFolder = Path.Combine(appDataPath, "TikTak");
            
            if (!Directory.Exists(tikTakFolder))
            {
                Directory.CreateDirectory(tikTakFolder);
            }
            
            _settingsPath = Path.Combine(tikTakFolder, "settings.json");
        }

        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    return settings ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                // Log error if needed
                Console.WriteLine($"Settings load error: {ex.Message}");
            }
            
            return new AppSettings(); // Return default settings
        }

        public void SaveSettings(AppSettings settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                // Log error if needed
                Console.WriteLine($"Settings save error: {ex.Message}");
            }
        }
    }

    public class AppSettings
    {
        public string Theme { get; set; } = "Koyu"; 
        public string BackgroundColor { get; set; } = "Black"; 
        public string FontColor { get; set; } = "White"; 
        public string Language { get; set; } = "Türkçe"; 
        public bool DesktopNotifications { get; set; } = false; 
        public bool TimeEndNotification { get; set; } = true;
        public bool FiveMinuteWarning { get; set; } = true;
        public bool OneMinuteWarning { get; set; } = true;
        public string DisplaySize { get; set; } = "Small";
        public string DisplayMargin { get; set; } = "Minimal";
        
        // Window position settings
        public string LastPosition { get; set; } = "TopRight";
        public int LastScreenIndex { get; set; } = 0;
        public double LastCustomLeft { get; set; } = 0;
        public double LastCustomTop { get; set; } = 0;
    }

    public class TimerTheme
    {
        public string Name { get; set; } = "";
        public string BackgroundColor { get; set; } = "";
        public string FontColor { get; set; } = "";
        public string Description { get; set; } = "";
        public string Icon { get; set; } = "";
    }

    public static class ThemeManager
    {
        public static List<TimerTheme> GetAvailableThemes()
        {
            return new List<TimerTheme>
            {
                new TimerTheme
                {
                    Name = "Koyu",
                    BackgroundColor = "Black",
                    FontColor = "White",
                    Description = "Koyu tema (Siyah zemin / Beyaz yazı)",
                    Icon = "⚫"
                },
                new TimerTheme
                {
                    Name = "Açık",
                    BackgroundColor = "White",
                    FontColor = "Black",
                    Description = "Açık tema (Beyaz zemin / Siyah yazı)",
                    Icon = "⚪"
                },
                new TimerTheme
                {
                    Name = "Koyu Mavi",
                    BackgroundColor = "DarkBlue",
                    FontColor = "Yellow",
                    Description = "Koyu mavi tema (Koyu mavi zemin / Sarı yazı)",
                    Icon = "🎤"
                },
                new TimerTheme
                {
                    Name = "Açık Mavi",
                    BackgroundColor = "LightBlue",
                    FontColor = "DarkBlue",
                    Description = "Açık mavi tema (Açık mavi zemin / Koyu yazı)",
                    Icon = "🔵"
                },
                new TimerTheme
                {
                    Name = "Yeşil",
                    BackgroundColor = "DarkGreen",
                    FontColor = "White",
                    Description = "Yeşil tema (Yeşil zemin / Beyaz yazı)",
                    Icon = "🌲"
                }
            };
        }

        public static TimerTheme? GetThemeByName(string themeName)
        {
            return GetAvailableThemes().FirstOrDefault(t => t.Name == themeName);
        }

        public static void ApplyThemeToSettings(AppSettings settings, string themeName)
        {
            var theme = GetThemeByName(themeName);
            if (theme != null)
            {
                settings.Theme = themeName;
                settings.BackgroundColor = theme.BackgroundColor;
                settings.FontColor = theme.FontColor;
            }
        }
    }

    public static class LanguageManager
    {
        public static List<string> GetAvailableLanguages()
        {
            return new List<string> { "Türkçe", "English" };
        }

        public static string GetTimeUpMessage(string language)
        {
            return language switch
            {
                "English" => "TIME'S UP",
                "Türkçe" => "SÜRE BİTTİ",
                _ => "SÜRE BİTTİ" // Default Turkish
            };
        }

        public static string GetLanguageDisplayName(string language)
        {
            return language switch
            {
                "English" => "🇺🇸 English",
                "Türkçe" => "🇹🇷 Türkçe",
                _ => "🇹🇷 Türkçe"
            };
        }
    }
}