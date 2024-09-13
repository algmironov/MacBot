using MacBot.ConsoleApp.Exceptions;
using MacBot.ConsoleApp.Models;
using MacBot.ConsoleApp.Repository;

using SimpleLogger;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MacBot.ConsoleApp.UpdateHandlers
{
    public class HandlerService : IHandlerService
    {
        private readonly Logger _logger;
        private readonly IBotMessagesRepository _botMessagesRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISessionRepository _sessionRepository;

        public HandlerService(Logger logger, IBotMessagesRepository botMessagesRepository, IUserRepository userRepository, ISessionRepository sessionRepository)
        {
            _logger = logger;
            _botMessagesRepository = botMessagesRepository;
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;
        }

        public async Task RemoveUserMessage(ITelegramBotClient client, Update update)
        {
            try
            {
                var messageId = update.Message.MessageId;
                var chatId = update.Message.From.Id;

                await client.DeleteMessageAsync(chatId: chatId, messageId: messageId);
            }
            catch (Exception ex)
            {
                _logger.Error("Возникла ошибка при удалении сообщения от пользователя", ex);
            }
        }

        public async Task ClearChat(ITelegramBotClient client, long chatId)
        {
            var chatMessages = await _botMessagesRepository.GetAllByChatId(chatId);

            foreach (var chatMessage in chatMessages)
            {
                try
                {
                    await client.DeleteMessageAsync(chatId, chatMessage.MessageId);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Это сообщение messageId= {chatMessage.MessageId} в чате id= {chatId} уже было удалено", ex);
                }
                finally
                {
                    chatMessage.IsDeleted = true;
                    await _botMessagesRepository.UpdateAsync(chatMessage);
                    _logger.Info($"Сообщение messageId= {chatMessage.MessageId} удалено из чата id= {chatId}");
                }
            }

            await _botMessagesRepository.ClearDeletedMesages(chatId);
        }

        public async Task DeleteImageMessages(ITelegramBotClient client, List<BotMessage> botMessages)
        {
            try
            {
                foreach (var message in botMessages)
                {
                    message.IsDeleted = true;

                    await _botMessagesRepository.UpdateAsync(message);

                    await client.DeleteMessageAsync(
                    chatId: message.ChatId,
                    messageId: message.MessageId
                    );
                    _logger.Info($"Сообщение с изображением MessageId= {message.MessageId} в чате ChatID= {message.ChatId} удалено успешно");
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Возникла ошибка при удалении изображений", e);
            }
        }

        public async Task DeleteLastMessage(ITelegramBotClient client, BotMessage botMessage)
        {
            try
            {
                await client.DeleteMessageAsync(
                    chatId: botMessage.ChatId,
                    messageId: botMessage.MessageId
                    );
                _logger.Info($"Сообщение  в чате ChatID= {botMessage.ChatId} удалено успешно");
            }
            catch (Exception e)
            {
                _logger.Error($"Возникла ошибка при удалении сообщения", e);
            }
            finally
            {
                botMessage.IsDeleted = true;
                await _botMessagesRepository.UpdateAsync(botMessage);
            }
        }

        public Dictionary<string, string> GetButtonsWithCallbacks(List<string> buttons, string prefix)
        {
            Dictionary<string, string> buttonsWithCallbacks = [];

            foreach (var button in buttons)
            {
                buttonsWithCallbacks[button] = $"{prefix} {button}";
            }
            return buttonsWithCallbacks;
        }

        public long GetChatId(Update update)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    {
                        return update.Message.Chat.Id;
                    }
                case UpdateType.CallbackQuery:
                    {
                        return update.CallbackQuery.Message.Chat.Id;
                    }
                case UpdateType.EditedMessage:
                    {
                        return update.EditedMessage.Chat.Id;
                    }
                default:
                    {
                        throw new InvalidUpdateTypeException("Неподдерживаемый тип обновления.");
                    }
            }
        }

        public async Task<BotMessage> GetLast(long chatId)
        {
            return await _botMessagesRepository.GetLast(chatId);
        }

        public async Task<BotUser> GetOrAddBotUser(Update update)
        {
            var chatId = GetChatId(update);
            var botUser = await _userRepository.GetByChatIdAsync(chatId);

            if (botUser == null)
            {
                botUser = new BotUser
                    (
                        name: $"{update.Message.From.FirstName} {update.Message.From.LastName}",
                        chatId: chatId,
                        activeRole: Role.Default
                    );
                await _userRepository.AddAsync(botUser);
                return botUser;
            }

            try
            {
                if (botUser.ActiveRole == Role.Default)
                {
                    switch (update!.CallbackQuery!.Data!.ToLower())
                    {
                        case "психолог":
                            botUser.ActiveRole = Role.Master;
                            await _userRepository.UpdateAsync(botUser);
                            break;
                        case "клиент":
                            botUser.ActiveRole = Role.Client;
                            await _userRepository.UpdateAsync(botUser);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Возникла ошибка", ex);
            }

            return botUser;
        }

        public async Task UpdateUserRole(BotUser user)
        {
            user.ChangeRole();
            await _userRepository.UpdateAsync(user);
        }

        public async Task<bool> HasActiveSession(BotUser user)
        {
            var session = await _sessionRepository.GetActiveByUser(user);
            if (session != null)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> IsLastMessageEditable(long chatId)
        {
            var lastMessage = await _botMessagesRepository.GetLast(chatId);

            if (lastMessage == null)
                return false;

            return lastMessage.IsLast
                   && lastMessage.MessageType != BotMessageType.Image
                   && !lastMessage.IsDeleted;
        }

        public async Task RemoveImagePageKeyboard(ITelegramBotClient client, long chatId, int messageId)
        {
            try
            {
                await client.EditMessageReplyMarkupAsync
                    (
                        chatId: chatId,
                        messageId: messageId,
                        replyMarkup: null
                    );
            }
            catch (Exception ex)
            {
                _logger.Error($"Не удалось удалить клавиатуру у сообщения messageId= {messageId} в чате id= {chatId} ", ex);
            }
        }

        public async Task SetLastMessage(BotMessage botMessage)
        {
            try
            {
                var chatId = botMessage.ChatId;
                var lastMessage = await _botMessagesRepository.GetLast(chatId);
                if (lastMessage != null)
                {
                    lastMessage.IsLast = false;
                    await _botMessagesRepository.AddOrUpdateAsync(lastMessage);
                }
                botMessage.IsLast = true;
                await _botMessagesRepository.AddOrUpdateAsync(botMessage);
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка при установке последнего сообщения", ex);
            }
        }

        public bool IsOwner(long chatId)
        {
            return chatId == 1142309198;
        }
    }
}
