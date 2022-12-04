

using Telegram.Bot;
using Telegram.Bot.Polling;

namespace HovyMonitor.TlgBot
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ITelegramBotClient _bot;
        private readonly IServiceProvider _services;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _bot = serviceProvider.GetRequiredService<ITelegramBotClient>();
            _services = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _bot.StartReceiving(
                _services.GetRequiredService<Tasks.TelegramUpdatesReceiver>().HandleAsync,
                _services.GetRequiredService<Tasks.TelegramErrorsReceiver>().Handle,
                new ReceiverOptions { AllowedUpdates = { } }, // receive all update types
                stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}