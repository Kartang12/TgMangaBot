using System;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TgMangaBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Telegram Manga Bot...");

            // Load configuration
            var appConfig = ConfigurationManager.LoadConfig();
            if (appConfig == null)
            {
                Console.WriteLine("Failed to load configuration. Please check config.json file.");
                return;
            }

            // Validate configuration
            if (string.IsNullOrEmpty(appConfig.telegram_bot_token) || 
                appConfig.telegram_bot_token == "YOUR_TELEGRAM_BOT_TOKEN")
            {
                Console.WriteLine("Please set up your Telegram bot token in config.json");
                return;
            }

            // Telegram bot setup
            var botClient = new TelegramBotClient(appConfig.telegram_bot_token);
            var commandHandler = new BotCommandHandler(botClient);

            // Load user preferences
            var userPreferences = ConfigurationManager.LoadUserPreferences();

            Console.WriteLine("Monitoring @lnmanga for topics:");
            foreach (var topic in userPreferences.Topics)
            {
                Console.WriteLine(topic);
            }

            // Start receiving messages
            using var cts = new CancellationTokenSource();

            var receiverOptions = new Telegram.Bot.Polling.ReceiverOptions
            {
                AllowedUpdates = new Telegram.Bot.Types.Enums.UpdateType[] { Telegram.Bot.Types.Enums.UpdateType.Message }
            };

            botClient.StartReceiving(
                async (bot, update, token) => await commandHandler.HandleUpdateAsync(update, userPreferences),
                BotCommandHandler.HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token
            );

            // Start channel monitoring
            var channelMonitor = new ChannelMonitor(appConfig, botClient);
            _ = Task.Run(async () => await channelMonitor.StartMonitoring(userPreferences, cts.Token));

            Console.WriteLine("Bot started. Press any key to exit");
            Console.ReadKey();
            cts.Cancel();
        }
    }
}
