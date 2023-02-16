using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Microsoft.Extensions.Options;
using HovyMonitor.Entity;
using System.Text;
using Newtonsoft.Json;

namespace HovyMonitor.TlgBot.Tasks
{
    public class TelegramUpdatesReceiver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TelegramUpdatesReceiver> _logger;
        private readonly TelegramBotConfiguration _configurationBot;

        public TelegramUpdatesReceiver(ILogger<TelegramUpdatesReceiver> logger, IServiceProvider serviceProvider,
            IOptions<TelegramBotConfiguration> configurationOptions)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configurationBot = configurationOptions.Value;
        }


        public async Task HandleAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        await HandleMessageAsync(botClient, update.Message, cancellationToken);
                        break;
                    //case UpdateType.CallbackQuery:
                    //    await HandleCallbackQuery(botClient, update.CallbackQuery, cancellationToken);
                    //    break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to execute command {ex.ToString()}");

                await botClient.SendTextMessageAsync(_configurationBot.AllowedChatIds.First(), "Something went wrong. See console.");
            }
        }

        private async Task HandleMessageAsync(ITelegramBotClient botClient, Message? message, CancellationToken cancellationToken)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (string.IsNullOrEmpty(message.Text))
                throw new InvalidOperationException("Message text should be set");

            if (message.From == null)
                throw new InvalidOperationException("Message sender should be set");

            if (message.From.IsBot)
                throw new InvalidOperationException("Bots is not allowed as sender");

            if (!_configurationBot.AllowedChatIds.Contains(message.From.Id))
                throw new InvalidOperationException($"User with id {message.From.Id} is not a whitelisted");

            switch (message.Text?.Trim().ToLower())
            {
                case "/all":

                    var text = await GetAllDetectionsAsync();
                    await botClient.SendTextMessageAsync(message.From.Id, text);

                    break;
            }
        }

        private async Task<String> GetAllDetectionsAsync()
        {

            var detections = await GetSensorDetectionsAsync();
            var detectionsGrouped = detections.GroupBy(x => x.SensorName);

            var resultSB = new StringBuilder();

            foreach (var group in detectionsGrouped)
            {
                resultSB
                    .Append("👁 ")
                    .AppendLine(group.Key);

                foreach (var item in group)
                {
                    resultSB
                        .Append("\t ")
                        .Append(item.Name)
                        .Append(" = ")
                        .Append(item.Value)
                        .Append("(")
                        //.Append(item.DateTime.ToShortDateString())
                        //.Append(" ")
                        .Append(item.DateTime.ToShortTimeString()) // todo: time ago YOU BROKE IT, YOU FIX IT!!!
                        .AppendLine(")");
                }
            }


            return resultSB.ToString();
        }

        private async Task<List<SensorDetection>> GetSensorDetectionsAsync()
        {
            // todo: put in config pls
            Uri uri = new ("http://192.168.1.125:6600/detections/last");

            using var response = await new HttpClient().GetAsync(uri).ConfigureAwait(false);

            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var detections = JsonConvert.DeserializeObject<List<SensorDetection>>(responseString);

            if (detections == null) throw new NullReferenceException();

            return detections;
        }
    }
}
