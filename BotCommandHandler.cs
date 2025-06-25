using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TgMangaBot
{
    public class BotCommandHandler
    {
        private readonly ITelegramBotClient _botClient;

        public BotCommandHandler(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task HandleUpdateAsync(Telegram.Bot.Types.Update update, UserPreferences userPreferences)
        {
            if (update.Message is not { } message)
            {
                return;
            }

            var chatId = message.Chat.Id;

            // Set user ID for forwarding messages
            if (userPreferences.UserId == 0)
            {
                userPreferences.UserId = chatId;
                ConfigurationManager.SaveUserPreferences(userPreferences);
            }

            if (string.IsNullOrEmpty(message.Text))
            {
                return;
            }

            if (message.Text.StartsWith("+topic"))
            {
                await HandleAddTopicCommand(message, userPreferences, chatId);
            }
            else if (message.Text.StartsWith("-topic"))
            {
                await HandleRemoveTopicCommand(message, userPreferences, chatId);
            }
            else if (message.Text.Equals("list", StringComparison.OrdinalIgnoreCase))
            {
                await HandleListCommand(userPreferences, chatId);
            }
            else if (message.Text.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                await HandleHelpCommand(chatId);
            }

            ConfigurationManager.SaveUserPreferences(userPreferences);
        }

        private async Task HandleAddTopicCommand(Telegram.Bot.Types.Message message, UserPreferences userPreferences, long chatId)
        {
            var topicsText = message.Text.Substring(6).Trim();
            if (string.IsNullOrEmpty(topicsText))
            {
                await _botClient.SendTextMessageAsync(chatId, "Please provide topics to add. Example:\n+topic\nOne Piece\nNaruto");
                return;
            }

            var topicsToAdd = topicsText.Split('\n').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t));
            var addedCount = 0;
            var duplicateCount = 0;

            foreach (var topic in topicsToAdd)
            {
                if (!userPreferences.Topics.Contains(topic, StringComparer.OrdinalIgnoreCase))
                {
                    userPreferences.Topics.Add(topic);
                    addedCount++;
                }
                else
                {
                    duplicateCount++;
                }
            }

            var responseMessage = $"✅ Added {addedCount} topic(s).";
            if (duplicateCount > 0)
            {
                responseMessage += $" Skipped {duplicateCount} duplicate(s).";
            }

            await _botClient.SendTextMessageAsync(chatId, responseMessage);
        }

        private async Task HandleRemoveTopicCommand(Telegram.Bot.Types.Message message, UserPreferences userPreferences, long chatId)
        {
            var topicsText = message.Text.Substring(6).Trim();
            if (string.IsNullOrEmpty(topicsText))
            {
                await _botClient.SendTextMessageAsync(chatId, "Please provide topics to remove. Example:\n-topic\nOne Piece\nNaruto");
                return;
            }

            var topicsToRemove = topicsText.Split('\n').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t));
            var removedCount = 0;

            foreach (var topic in topicsToRemove)
            {
                var existingTopic = userPreferences.Topics.FirstOrDefault(t => 
                    t.Equals(topic, StringComparison.OrdinalIgnoreCase));
                
                if (existingTopic != null)
                {
                    userPreferences.Topics.Remove(existingTopic);
                    removedCount++;
                }
            }

            await _botClient.SendTextMessageAsync(chatId, $"🗑️ Removed {removedCount} topic(s).");
        }

        private async Task HandleListCommand(UserPreferences userPreferences, long chatId)
        {
            if (!userPreferences.Topics.Any())
            {
                await _botClient.SendTextMessageAsync(chatId, "📝 No topics in your list.\n\nUse +topic to add some topics to monitor.");
                return;
            }

            var topicsList = string.Join("\n", userPreferences.Topics.Select((topic, index) => $"{index + 1}. {topic}"));
            var message = $"📝 Your monitored topics ({userPreferences.Topics.Count}):\n\n{topicsList}";
            
            await _botClient.SendTextMessageAsync(chatId, message);
        }

        private async Task HandleHelpCommand(long chatId)
        {
            var helpMessage = @"🤖 Telegram Manga Bot Commands:

📝 **Topic Management:**
• `+topic` - Add topics to monitor (one per line)
  Example:
  +topic
  One Piece
  Naruto

• `-topic` - Remove topics (one per line)
  Example:
  -topic
  One Piece

• `list` - Show all monitored topics

• `help` - Show this help message

🔍 **How it works:**
The bot monitors @lnmanga channel for posts containing your topics in the first line. When a match is found, the post is sent to you automatically.

⚡ **Tips:**
- Topics are case-insensitive
- The bot checks for new posts every 5 minutes
- Duplicate topics are automatically skipped";

            await _botClient.SendTextMessageAsync(chatId, helpMessage, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Bot error: {exception}");
        }
    }
}
