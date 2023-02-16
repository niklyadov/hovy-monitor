using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace HovyMonitor.TlgBot.Tasks
{
    public class TelegramErrorsReceiver
    {
        private readonly ILogger<TelegramErrorsReceiver> _logger;
        public TelegramErrorsReceiver(ILogger<TelegramErrorsReceiver> logger)
        {
            _logger = logger;
        }

        public async Task Handle(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                _logger.LogError($"{JsonConvert.SerializeObject(exception)}");
            });
        }
    }
}
