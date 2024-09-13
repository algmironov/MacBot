using MacBot.ConsoleApp.Models;

using SimpleLogger;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace MacBot.ConsoleApp.UpdateHandlers
{
    public class CallbackUpdateHandler : IUpdateHandler
    {
        private Logger _logger;
        private IHandlerService _handlerService;
        private ISpecificUpdateHandlerFactory _handlerFactory;

        public CallbackUpdateHandler(IHandlerService handlerService, ISpecificUpdateHandlerFactory factory, Logger logger)
        {
            _logger = logger;
            _handlerService = handlerService;
            _handlerFactory = factory;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            var user = await _handlerService.GetOrAddBotUser(update);

            var handler = await GetHandler(user);

            _logger.Info($"Handler {handler} выбран для обработки обновления юзера с ролью: {user.ActiveRole}");

            await handler.HandleUpdateAsync(client, update, cancellationToken);
        }

        public async Task<IUpdateHandler> GetHandler(BotUser user)
        {
            return user.ActiveRole switch
            {

                Role.Master when await _handlerService.HasActiveSession(user)
                    => _handlerFactory.GetMasterActiveSessionUpdateHandler()
                    ?? throw new InvalidOperationException("MasterActiveSessionUpdatesHandler not found"),

                Role.Master
                    => _handlerFactory.GetMasterUpdateHandler()
                    ?? throw new InvalidOperationException("MasterUpdatesHandler not found"),

                Role.Client when await _handlerService.HasActiveSession(user)
                    => _handlerFactory.GetClientActiveSessionUpdateHandler()
                    ?? throw new InvalidOperationException("ClientActiveSessionUpdatesHandler not found"),

                Role.Client
                    => _handlerFactory.GetClientUpdateHandler()
                    ?? throw new InvalidOperationException("ClientUpdatesHandler not found"),

                _ => throw new NotSupportedException($"Unsupported user Role: {user.ActiveRole}")

            };
        }

    }
}
