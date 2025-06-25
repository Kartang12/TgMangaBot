# Telegram Manga Bot

This is a Telegram bot designed to monitor the @lnmanga channel for specific book titles and send matching posts to your Saved Messages.

## Features

- **User Commands:** Manage topics using commands like `+topic`, `-topic`, `list`, and `help`.
- **Channel Monitoring:** Automatically checks for new posts with your specified book titles.
- **Data Persistence:** Saves user preferences and last checked post ID.

## Setup Instructions

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/yourusername/TelegramMangaBot.git
   cd TelegramMangaBot
   ```

2. **Configure the Bot:**
   - Open `config.json` and fill in the details:
     - `telegram_bot_token`: Your Bot API token from @BotFather
     - `api_id`: Your API ID from [my.telegram.org](https://my.telegram.org/apps)
     - `api_hash`: Your API hash from [my.telegram.org](https://my.telegram.org/apps)
     - `phone_number`: Your phone number for MTProto client

3. **Build and Run:**
   ```bash
   dotnet build
   dotnet run
   ```

4. **Authenticate:**
   - The first run will require you to authenticate your phone number to access the Telegram API with MTProto.

5. **Start Using the Bot:**
   - Add topics using the bot commands and start monitoring the channel.

## Notes

- The bot checks for new posts every 5 minutes by default. This can be adjusted in `config.json`.
- Ensure you have the necessary permissions and access for the channel you want to monitor.

## License

MIT License
