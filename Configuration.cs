using System;
using System.IO;
using Newtonsoft.Json;

namespace TgMangaBot
{
    public class AppConfig
    {
        public string telegram_bot_token { get; set; } = "";
        public string api_id { get; set; } = "";
        public string api_hash { get; set; } = "";
        public string phone_number { get; set; } = "";
        public string channel_username { get; set; } = "lnmanga";
        public int check_interval_minutes { get; set; } = 5;
        public int initial_messages_count { get; set; } = 100;
    }

    public class UserPreferences
    {
        public List<string> Topics { get; set; } = new List<string>();
        public int LastCheckedMessageId { get; set; } = 0;
        public long UserId { get; set; } = 0;
    }

    public static class ConfigurationManager
    {
        public static AppConfig? LoadConfig()
        {
            var filePath = "config.json";
            
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Config file not found. Please make sure config.json exists.");
                return null;
            }
            
            try
            {
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<AppConfig>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading config: {ex.Message}");
                return null;
            }
        }

        public static UserPreferences LoadUserPreferences()
        {
            var filePath = "user_preferences.json";

            if (!File.Exists(filePath))
            {
                Console.WriteLine("User preferences file not found. Creating a new one.");
                var defaultPreferences = new UserPreferences { Topics = new List<string>() };
                File.WriteAllText(filePath, JsonConvert.SerializeObject(defaultPreferences, Formatting.Indented));
                return defaultPreferences;
            }

            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<UserPreferences>(json) ?? new UserPreferences { Topics = new List<string>() };
        }

        public static void SaveUserPreferences(UserPreferences userPreferences)
        {
            var filePath = "user_preferences.json";
            var json = JsonConvert.SerializeObject(userPreferences, Formatting.Indented);
            File.WriteAllText(filePath, json);
            Console.WriteLine("User preferences saved.");
        }
    }
}
