using HovyMonitor.TlgBot;
using HovyMonitor.TlgBot.Tasks;
using Telegram.Bot;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<TelegramBotConfiguration>(
            context.Configuration.GetRequiredSection("TelegramBot"));

        services.AddSingleton<TelegramErrorsReceiver>();
        services.AddSingleton<TelegramUpdatesReceiver>();

        services.AddSingleton<ITelegramBotClient>(x => 
            new TelegramBotClient(context.Configuration["TelegramBot:Key"], new HttpClient()));
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
