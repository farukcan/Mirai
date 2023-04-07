using OpenAI_API;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Mirai.Models;
using Serilog;
using OpenAI_API.Models;
using Newtonsoft.Json;
using System.Globalization;

namespace Mirai.Services
{
    public class Bot : IHostedService
    {
        public static Bot? Instance;
        string? BotToken => Environment.GetEnvironmentVariable("BOT_TOKEN");
        private TelegramBotClient client;
        private static OpenAIAPI? api;
        public Dictionary<string, Prompt> prompts = new();
        public List<Dialog> dialogs = new();
        public static OpenAIAPI Api => api ??= new OpenAIAPI();
        private Rethink rethink;
        public Bot(){
            // create Telegram.Bot client
            if(BotToken is null)
                throw new ArgumentNullException("BOT_TOKEN is null");
            client = new TelegramBotClient(BotToken);
            Log.Information("Bot is instantiated");
            Instance = this;
            rethink = Rethink.Instance ?? throw new ArgumentNullException("Rethink is null");
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
                prompts[name] = new Prompt(name, text);
            }
        }

        private async Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cts)
        {
            throw exception;
        }

        private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cts)
        {
            Log.Information(update.Type.ToString() + " type update");
            if (update.Type == UpdateType.Message && update.Message is not null)
            {
                var username = update.Message.From?.Username ?? "no-username";
                Log.Information($"Message from {username} received");
                var dialog = new Dialog(update.Message, username);
                await CreateDialog(dialog);
            }
        }

        public async Task CreateDialog(Dialog dialog){
            Log.Information($"Creating Dialog {dialog.Id}");
            await rethink.Begin(out var R).End(R.Db("Mirai").Table("Dialogs").Insert(dialog));
            await ProcessDialog(dialog);
        }

        public async Task ProcessDialog(Dialog dialog){
            Log.Information($"Processing {dialog.Status} Dialog");
            if(dialog.Status != Dialog.State.Answered){
                if(dialog.Status == Dialog.State.Waiting){
                    await Analyze(dialog);
                }else if(dialog.Status == Dialog.State.Information){
                    await Inform(dialog);
                }else if(dialog.Status == Dialog.State.Question){
                    await AnswerQuestion(dialog);
                }else if(dialog.Status == Dialog.State.Order){
                    await Answer(dialog,"Emir verdiniz");
                }
            }else{
                Log.Warning("Cannot process dialog because it is already answered");
            }
        }
        public async Task Analyze(Dialog dialog){
            if(dialog.Message.Text is null){
                await SetNoAnswer(dialog,"text is null");
                return;
            }
            Log.Information($"Analyzing : {dialog.Message.Text}");
            var prompt = prompts["Analysis"]
                            .Use()
                            .Set("message",dialog.Message.Text)
                            .Get();
            dialog.Results["AnalysisPrompt"] = prompt;
            dialog.Status = Dialog.State.Analysis;
            await SaveDialog(dialog);
            var result = await Api.Completions.CreateCompletionAsync(
                prompt,
                model : Model.DavinciText,
                temperature: 1,
                max_tokens: 256,
                top_p: 1,
                frequencyPenalty: 0,
                presencePenalty: 0
            );
            dialog.Results["Analysis"] = result;
            // get first choice
            var choice = result.Completions.FirstOrDefault();
            if(choice is null){
                await SetNoAnswer(dialog,"Choice is null");
                return;
            }
            var analysisAnswers = choice.Text.Split(",");
            if(analysisAnswers.Length!=4){
                await SetNoAnswer(dialog,"Failed analysis");
                return;
            }
            dialog.IsQuestion = analysisAnswers[0].ToLower().Contains("yes");
            dialog.IsInformation = analysisAnswers[1].ToLower().Contains("yes");
            dialog.IsOrder = analysisAnswers[2].ToLower().Contains("yes");
            bool isTurkish = analysisAnswers[3].ToLower().Contains("yes");
            if(!isTurkish){
                await Answer(dialog,"Lütfen türkçe konuşun. Yabancı diller bana yasaklandı.");
                return;
            }
            // TODO: inline keyboard desteği ekle
            if(dialog.IsQuestion && dialog.IsQuestion == dialog.IsInformation){
                await Answer(dialog,"Soru mu sordunuz? Yoksa bilgi mi veriyorsunuz?");
                return;
            }
            if(dialog.IsQuestion && dialog.IsQuestion == dialog.IsOrder){
                await Answer(dialog,"Soru mu sordunuz? Yoksa emir mi verdiniz?");
                return;
            }
            if(dialog.IsInformation && dialog.IsInformation == dialog.IsOrder){
                await Answer(dialog,"Bilgi mi verdiniz? Yoksa emir mi verdiniz?");
                return;
            }
            if(dialog.IsQuestion){
                dialog.Status = Dialog.State.Question;
            }
            else if(dialog.IsInformation){
                dialog.Status = Dialog.State.Information;
            }else if(dialog.IsOrder){
                dialog.Status =  Dialog.State.Order;
            }
            await SaveDialog(dialog);
            await ProcessDialog(dialog);
        }
        public async Task Inform(Dialog dialog){
            if(dialog.Message.Text is null) return;
            Log.Information($"Informing : {dialog.Message.Text}");
            var prompt = prompts["Inform"]
                            .Use()
                            .Set("from",dialog.Interlocutor)
                            .Set("message",dialog.Message.Text)
                            .Get();
            dialog.Results["InformPrompt"] = prompt;
            await SaveDialog(dialog);
            var result = await Api.Completions.CreateCompletionAsync(
                prompt,
                model : Model.DavinciText,
                temperature: 1,
                max_tokens: 256,
                top_p: 1,
                frequencyPenalty: 0,
                presencePenalty: 0
            );
            dialog.Results["Inform"] = result;
            // get first choice
            var choice = result.Completions.FirstOrDefault();
            if(choice is null){
                await SetNoAnswer(dialog,"[Inform] Choice is null");
                return;
            }
            var information = Information.FromUser(dialog.Interlocutor,choice.Text.Trim());
            await CreateInformation(information);
            // save information
            await Answer(dialog,information.Data);
        }
        public async Task AnswerQuestion(Dialog dialog){
            if(dialog.Message.Text is null) return;
            Log.Information($"Answering Question : {dialog.Message.Text}");
            // TODO: Optimize list
            var infos = rethink.Linq<Information>("Mirai","Informations")
                            .OrderByDescending(i=>i.Time)
                            .ToArray();
            var list = "";
            foreach(var info in infos){
                list += $"- {info.Data}";
            }
            var prompt = prompts["AnswerQuestion"]
                            .Use()
                            .Set("from",dialog.Interlocutor)
                            .Set("list",list)
                            .Set("question",dialog.Message.Text)
                            .Get();
            dialog.Results["AnswerQuestionPrompt"] = prompt;
            await SaveDialog(dialog);
            var result = await Api.Completions.CreateCompletionAsync(
                prompt,
                model : Model.DavinciText,
                temperature: 1,
                max_tokens: 256,
                top_p: 1,
                frequencyPenalty: 0,
                presencePenalty: 0
            );
            dialog.Results["AnswerQuestion"] = result;
            // get first choice
            var choice = result.Completions.FirstOrDefault();
            if(choice is null){
                await SetNoAnswer(dialog,"[AnswerQuestion] Choice is null");
                return;
            }
            await Answer(dialog,choice.Text.Trim());
        }
        public async Task SetNoAnswer(Dialog dialog, string reason){
            dialog.Status = Dialog.State.NoAnswer;
            dialog.Results["NoAnswer"] = reason;
            await SaveDialog(dialog);
        }
        public async Task Answer(Dialog dialog, string answer){
            dialog.Status = Dialog.State.Answered;
            dialog.Answer = answer;
            await SaveDialog(dialog);
            await client.SendTextMessageAsync(
                chatId: dialog.Message.Chat.Id,
                text: answer
            );
        }
        public async Task SaveDialog(Dialog dialog){
            Log.Information($"Saving Dialog {dialog.Id}");
            dialog.UpdatedAt = DateTime.UtcNow;
            await rethink.Begin(out var R).End(R.Db("Mirai").Table("Dialogs").Get(dialog.Id).Update(dialog));
        }
        public async Task CreateInformation(Information information){
            Log.Information($"Creating Information {information.Id}");
            await rethink.Begin(out var R).End(R.Db("Mirai").Table("Informations").Insert(information));
        }
    }
}