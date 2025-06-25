using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using TL;

namespace TgMangaBot
{
    public class ChannelMonitor
    {
        private readonly AppConfig _config;
        private readonly ITelegramBotClient _botClient;

        public ChannelMonitor(AppConfig config, ITelegramBotClient botClient)
        {
            _config = config;
            _botClient = botClient;
        }

        public async Task StartMonitoring(UserPreferences userPreferences, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("Setting up Telegram client for channel monitoring...");
                
                // Initialize WTelegram client
                using var client = new WTelegram.Client(GetConfig);
                await client.LoginUserIfNeeded();
                
                Console.WriteLine("Telegram client authenticated successfully!");
                
                // Get the channel
                var chats = await client.Messages_GetAllChats();
                var lnmangaChannel = chats.chats.Values.FirstOrDefault(c => c.MainUsername == _config.channel_username);
                
                if (lnmangaChannel == null)
                {
                    Console.WriteLine($"@{_config.channel_username} channel not found. Make sure you have access to it.");
                    return;
                }
                
                Console.WriteLine($"Found channel: {lnmangaChannel.Title}");
                
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await CheckForNewMessages(client, lnmangaChannel, userPreferences);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error checking channel messages: {ex.Message}");
                    }
                    
                    await Task.Delay(TimeSpan.FromMinutes(_config.check_interval_minutes), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in channel monitoring: {ex.Message}");
                Console.WriteLine("Please make sure you have set up API credentials correctly.");
            }
        }

        private async Task CheckForNewMessages(WTelegram.Client client, TL.ChatBase channel, UserPreferences userPreferences)
        {
            // Get channel messages
            var limit = userPreferences.LastCheckedMessageId == 0 ? _config.initial_messages_count : 50;
            var inputPeer = channel.ToInputPeer();
            var history = await client.Messages_GetHistory(inputPeer, limit: limit);
            
            var newMessages = history.Messages.Where(m => m.ID > userPreferences.LastCheckedMessageId).ToList();
            
            foreach (var msg in newMessages.OrderBy(m => m.ID))
            {
                if (msg is TL.Message message && !string.IsNullOrEmpty(message.message))
                {
                    var firstLine = message.message.Split('\n').FirstOrDefault()?.Trim();
                    
                    if (!string.IsNullOrEmpty(firstLine))
                    {
                        // Check if any topic matches the first line
                        var matchingTopic = userPreferences.Topics.FirstOrDefault(topic => 
                            firstLine.Contains(topic, StringComparison.OrdinalIgnoreCase));
                        
                        if (matchingTopic != null)
                        {
                            Console.WriteLine($"Found matching post for topic '{matchingTopic}': {firstLine}");
                            
                            // Send the message to the user
                            var messageText = $"📖 Found post for '{matchingTopic}':\n\n{message.message}";
                            await _botClient.SendTextMessageAsync(userPreferences.UserId, messageText);
                        }
                    }
                    
                    userPreferences.LastCheckedMessageId = Math.Max(userPreferences.LastCheckedMessageId, message.ID);
                }
            }
            
            if (newMessages.Any())
            {
                ConfigurationManager.SaveUserPreferences(userPreferences);
                Console.WriteLine($"Processed {newMessages.Count} new messages. Last checked ID: {userPreferences.LastCheckedMessageId}");
            }
        }

        private string GetConfig(string what)
        {
            switch (what)
            {
                case "api_id": return _config.api_id;
                case "api_hash": return _config.api_hash;
                case "phone_number": return _config.phone_number;
                default: return null;
            }
        }
    }
}
