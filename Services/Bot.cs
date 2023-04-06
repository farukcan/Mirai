using OpenAI_API;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Mirai.Models;
using Serilog;

namespace Mirai.Services
{
    public class Bot : IHostedService
    {
        public static Bot? Instance;
        string? BotToken => Environment.GetEnvironmentVariable("BOT_TOKEN");
        private TelegramBotClient client;
        private static OpenAIAPI? api;
        public List<Prompt> prompts = new();
        public static OpenAIAPI Api => api ??= new OpenAIAPI();
        public Bot(){
            // create Telegram.Bot client
            if(BotToken is null)
                throw new ArgumentNullException("BOT_TOKEN is null");
            client = new TelegramBotClient(BotToken);
            Log.Information("Bot is instantiated");
            Instance = this;
        }
        public async Task StartAsync(CancellationToken cancellationToken){
            Log.Information("Bot is running");
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };
            client.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cancellationToken
            );
            var me = await client.GetMeAsync(cancellationToken);
            Log.Information("Start receiving updates for {BotName}", me.Username ?? "No username");
            LoadPrompts();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await client.CloseAsync();
        }

        private void LoadPrompts()
        {
            string[] files = Directory.GetFiles("Prompts");
            foreach(string file in files){
                string name = Path.GetFileNameWithoutExtension(file);
                string text = System.IO.File.ReadAllText(file);
                Log.Information($"Prompt '{name}' is loaded");
                prompts.Add(new Prompt(name, text));
            }
        }

        private async Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cts)
        {
            throw exception;
        }

        private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cts)
        {
            Log.Information(update.ToString());
            if (update.Type == UpdateType.Message && update.Message is not null)
            {
                await client.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: "Hey"
                );
            }
        }


    }
}