using MacBot.ConsoleApp.Keyboards;
using MacBot.ConsoleApp.Models;
using MacBot.ConsoleApp.Repository;
using MacBot.ConsoleApp.Repository.Database;
using MacBot.ConsoleApp.Services;
using MacBot.ConsoleApp.UpdateHandlers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SimpleLogger;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace MacBot.ConsoleApp;

class Program
{
    private static ServiceProvider _serviceProvider;

    private static Logger _logger;
    private static ISessionRepository _sessionRepository;
    private static IDeckService _deckService;
    private static IUpdateHandlerFactory _updateHandlerFactory;


    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var serviceCollection = new ServiceCollection();

        var storageSettings = configuration.GetSection("ObjectStorageServiceSettings");
        var accessKey = storageSettings.GetSection("AccessKey").Value;
        var secretKey = storageSettings.GetSection("SecretKey").Value;
        var serviceUrl = storageSettings.GetSection("ServiceUrl").Value;

        serviceCollection.AddSingleton<Logger>();

        serviceCollection.AddSingleton<IObjectStorageService, CloudService>(provider =>
        {
            var settings = new ObjectStorageServiceSettings(accessKey!, secretKey!, serviceUrl!);
            var logger = provider.GetRequiredService<Logger>();

            return new CloudService(settings, logger);
        });

        serviceCollection.AddDbContext<BotDbContext>();
        serviceCollection.AddTransient<IUserRepository, UserRepository>();
        serviceCollection.AddTransient<ISessionRepository, SessionRepository>();
        serviceCollection.AddTransient<ICodeStorage, CodeStorage>();
        serviceCollection.AddTransient<IDeckService, DeckService>();
        serviceCollection.AddTransient<IDeckRepository, DeckRepository>();
        serviceCollection.AddTransient<ICardRepository, CardRepository>();
        serviceCollection.AddTransient<IBotMessagesRepository, BotMessagesRepository>();
        serviceCollection.AddTransient<ISessionParametersRepository, SessionParametersRepository>();
        serviceCollection.AddTransient<ISessionCardRepository, SessionCardRepository>();
        serviceCollection.AddTransient<IPageManager, PageManager>();
        serviceCollection.AddSingleton<IPageMessagesManager, PageMessagesManager>();
        serviceCollection.AddTransient<IKeyboardFactory, KeyboardFactory>();
        serviceCollection.AddTransient<IUpdateHandler, CallbackUpdateHandler>();
        serviceCollection.AddTransient<IUpdateHandler, TextUpdateHandler>();
        serviceCollection.AddTransient<IUpdateHandler, PhotoUpdateHandler>();
        serviceCollection.AddTransient<IMasterUpdateHandler, MasterUpdatesHandler>();
        serviceCollection.AddTransient<IMasterActiveSessionUpdateHandler, MasterActiveSessionUpdatesHandler>();
        serviceCollection.AddTransient<IClientUpdateHandler, ClientUpdatesHandler>();
        serviceCollection.AddTransient<IClientActiveSessionUpdateHandler, ClientActiveSessionUpdatesHandler>();
        serviceCollection.AddTransient<IHandlerService, HandlerService>();
        serviceCollection.AddTransient<IFeedbackRepository, FeedbackRepository>();

        serviceCollection.AddSingleton<IUpdateHandlerFactory, UpdateHandlerFactory>();
        serviceCollection.AddSingleton<ISpecificUpdateHandlerFactory, SpecificUpdateHandlerFactory>();

        _serviceProvider = serviceCollection.BuildServiceProvider();

        using var context = _serviceProvider.GetRequiredService<BotDbContext>();
        context.Database.Migrate();

        _logger = _serviceProvider.GetRequiredService<Logger>();
        _sessionRepository = _serviceProvider.GetRequiredService<ISessionRepository>();
        _deckService = _serviceProvider.GetRequiredService<IDeckService>();
        _updateHandlerFactory = _serviceProvider.GetRequiredService<IUpdateHandlerFactory>();

        await _deckService.SynchronizeDecksWithStorage();

        await DropSessions();

        var telegramBotClient = new TelegramBotClient(configuration.GetSection("Bot").GetSection("Token").Value!);
        var me = await telegramBotClient.GetMeAsync();

        _logger.Info($"MacBot has started at {DateTime.Now} with ID = {me.Id}");

        try
        {
            telegramBotClient.StartReceiving(updateHandler: HandleUpdates, pollingErrorHandler: HandleErrors);
        }
        catch (Exception ex)
        {
            _logger.Error("Ошибка во время работы бота", ex);
        }

        Console.ReadLine();
    }

    private static async Task DropSessions()
    {
        var sessions = await _sessionRepository.GetAllAsync();
        foreach (var session in sessions)
        {
            session.EndSession();
            await _sessionRepository.UpdateAsync(session);
        }
    }

    private static async Task HandleErrors(ITelegramBotClient client, Exception exception, CancellationToken token)
    {
        var help = exception.HelpLink;
        var consoleMessage = $"Возникла ошибка: {exception.Message}\n{help}";
        _logger.Error($"{consoleMessage}", exception);
    }

    private static async Task HandleUpdates(ITelegramBotClient client, Update update, CancellationToken token)
    {
        try
        {
            var updateHandler = _updateHandlerFactory.GetHandler(update);

            await updateHandler.HandleUpdateAsync(client, update, token);
        }
        catch (Exception ex)
        {
            _logger.Error("Возникла ошибка при обработке обновления", ex);
        }
       
    }
}

