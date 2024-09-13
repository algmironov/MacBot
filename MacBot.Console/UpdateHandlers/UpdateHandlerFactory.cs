using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MacBot.ConsoleApp.UpdateHandlers
{
    public class UpdateHandlerFactory : IUpdateHandlerFactory
    {
        private readonly IEnumerable<IUpdateHandler> _handlers;

        public UpdateHandlerFactory(IEnumerable<IUpdateHandler> handlers)
        {
            _handlers = handlers;
        }

        public IUpdateHandler GetHandler(Update update)
        {
            return update.Type switch
            {
                UpdateType.Message when update.Message?.Text != null
                    => _handlers.FirstOrDefault(h => h is TextUpdateHandler)
                       ?? throw new InvalidOperationException("TextUpdateHandler not found"),
                UpdateType.CallbackQuery
                    => _handlers.FirstOrDefault(h => h is CallbackUpdateHandler)
                       ?? throw new InvalidOperationException("CallbackUpdateHandler not found"),
                UpdateType.Message when update.Message?.Photo != null
                    => _handlers.FirstOrDefault(h => h is PhotoUpdateHandler)
                       ?? throw new InvalidOperationException("PhotoUpdateHandler not found"),
                _ => throw new NotSupportedException($"Unsupported update type: {update.Type}")
            };
        }
    }
}
