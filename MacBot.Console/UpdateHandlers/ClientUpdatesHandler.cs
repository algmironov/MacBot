using MacBot.ConsoleApp.Keyboards;
using MacBot.ConsoleApp.Models;
using MacBot.ConsoleApp.Pages;
using MacBot.ConsoleApp.Repository;
using MacBot.ConsoleApp.Services;

using SimpleLogger;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace MacBot.ConsoleApp.UpdateHandlers
{
    public class ClientUpdatesHandler : IClientUpdateHandler
    {
        private Logger _logger;
        private IKeyboardFactory _keyboardFactory;
        private IPageManager _pageManager;
        private IHandlerService _handlerService;
        private IBotMessagesRepository _botMessagesRepository;
        private IUserRepository _userRepository;

        public ClientUpdatesHandler(IKeyboardFactory keyboardFactory,
                                    IPageManager pageManager,
                                    IHandlerService handlerService,
                                    IBotMessagesRepository botMessagesRepository,
                                    IUserRepository userRepository,
                                    Logger logger)
        {
            _logger = logger;
            _keyboardFactory = keyboardFactory;
            _pageManager = pageManager;
            _handlerService = handlerService;
            _botMessagesRepository = botMessagesRepository;
            _userRepository = userRepository;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            var chatId = _handlerService.GetChatId(update);

            var user = await _handlerService.GetOrAddBotUser(update);

            var messageData = update.CallbackQuery.Data;

            var lastMessage = await _handlerService.GetLast(chatId);

            var options = new CreatePageOptions
            {
                Client = client,
                Update = update,
                User = user,
                ChatId = chatId,
                Message = lastMessage,
                MessageData = messageData,
            };

            var commandHandlers = new Dictionary<string, Func<CreatePageOptions, Task>>
            {
                {"Клиент", ShowClientPage },
                {"Психолог", ShowMasterPage},
                {"О методике", ShowAboutPage},
                {"Обратная связь", ShowSendMessagePage },
                { "В главное меню", ShowMainPage }
            };

            if (commandHandlers.TryGetValue(messageData!, out var handler))
            {
                await handler(options);
                return;
            }

            var commandStartsWithHandlers = new Dictionary<string, Func<CreatePageOptions, Task>>
            {
                { "/pageName", ShowPreviousPage },
                { "/previousPage", ShowPreviousPage },
            };

            foreach (var handlerEntry in commandStartsWithHandlers)
            {
                if (messageData!.StartsWith(handlerEntry.Key))
                {
                    await handlerEntry.Value(options);
                    return;
                }
            }

        }

        private async Task ShowMainPage(CreatePageOptions options)
        {
            try
            {
                var clientPage = (TextPage)_pageManager.CreatePage(PageName.ClientPage);

                var keyboard = _keyboardFactory.CreateInlineKeyboard(clientPage.Buttons, PageName.WelcomePage);

                var message = await clientPage.Update(
                    client: options.Client,
                    chatId: options.ChatId,
                    messageId: options.Message.MessageId,
                    keyboard: keyboard);

                var botMessage = new BotMessage(
                    chatId: options.ChatId,
                    messageId: message.MessageId,
                    botMessageType: BotMessageType.Text
                    );
                await _handlerService.SetLastMessage(botMessage);

                _logger.Info($"Bot sent a message with id= {message.MessageId} to chat with id= {options.ChatId}");
                return;
            }
            catch (Exception ex)
            {
                _logger.Error($"Возникла ошибка: при обработке сообщения с данными:{options.MessageData}", ex);
            }
        }

        private async Task ShowAboutPage(CreatePageOptions options)
        {
            try
            {
                var clientPage = (TextPage)_pageManager.CreatePage(PageName.AboutPage);

                var keyboard = _keyboardFactory.CreateInlineKeyboard(clientPage.Buttons, PageName.ClientPage);

                var message = await clientPage.Update(
                    client: options.Client,
                    chatId: options.ChatId,
                    messageId: options.Message.MessageId,
                    keyboard: keyboard);

                var botMessage = new BotMessage(
                    chatId: options.ChatId,
                    messageId: message.MessageId,
                    botMessageType: BotMessageType.Text
                    );
                await _handlerService.SetLastMessage(botMessage);

                _logger.Info($"Bot sent a message with id= {message.MessageId} to chat with id= {options.ChatId}");
                return;
            }
            catch (Exception ex)
            {
                _logger.Error("Возникла ошибка при отображении страницы \"О методике\"", ex);
            }
        }

        private async Task ShowPreviousPage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var lastMessage = options.Message;
            var lastMessageId = lastMessage.MessageId;

            try
            {
                _ = Enum.TryParse(messageData.Split(' ')[1], out PageName pageName);

                var page = (TextPage)_pageManager.CreatePage(pageName);

                if (lastMessage.MessageType != BotMessageType.Text)
                {
                    try
                    {
                        await client.DeleteMessageAsync(
                                        chatId: chatId,
                                        messageId: lastMessageId);

                        lastMessage.IsDeleted = true;
                        await _botMessagesRepository.UpdateAsync(lastMessage);
                    }
                    catch (Exception)
                    {
                        _logger.Warn($"Невозможно удалить сообщение с id= {lastMessageId} в чате id = {chatId}");
                    }
                }

                var message = await page.Update(
                    client: client,
                    chatId: chatId,
                    messageId: lastMessage.MessageId,
                    keyboard: _keyboardFactory.CreateInlineKeyboard(page.Buttons, _pageManager.GetPreviousPageName(pageName))
                    );

                lastMessage.MessageId = message.MessageId;
                await _handlerService.SetLastMessage(lastMessage);

                _logger.Info($"Bot sent a message with id= {message.MessageId} to chat with id= {chatId}");
                return;
            }
            catch (Exception e)
            {
                _logger.Error($"Возникла ошибка: при обработке сообщения с данными:{messageData}", e);
            }
        }

        private async Task ShowMasterPage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var lastMessage = options.Message;
            var lastMessageId = lastMessage.MessageId;

            var user = options.User;
            await _handlerService.UpdateUserRole(user);

            try
            {
                var masterPage = (TextPage)_pageManager.CreatePage(PageName.MasterPage);

                var keyboard = _keyboardFactory.CreateInlineKeyboard(masterPage.Buttons, PageName.WelcomePage);

                var message = new Message();

                message = await masterPage.Update(
                           client: client,
                           chatId: chatId,
                           messageId: lastMessageId,
                           keyboard: keyboard);

                var botMessage = new BotMessage(
                    chatId: chatId,
                    messageId: message.MessageId,
                    botMessageType: BotMessageType.Text
                    );
                await _handlerService.SetLastMessage(botMessage);

                _logger.Info($"Bot sent a message with id= {message.MessageId} to chat with id= {chatId}");
                return;
            }
            catch (Exception e)
            {
                _logger.Error($"Возникла ошибка: при обработке сообщения с данными:{messageData}", e);
            }
        }

        private async Task ShowClientPage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var lastMessage = options.Message;
            var lastMessageId = lastMessage.MessageId;

            try
            {
                var clientPage = (TextPage)_pageManager.CreatePage(PageName.ClientPage);

                var keyboard = _keyboardFactory.CreateInlineKeyboard(clientPage.Buttons, PageName.WelcomePage);

                var message = await clientPage.Update(
                    client: client,
                    chatId: chatId,
                    messageId: lastMessageId,
                    keyboard: keyboard);

                var botMessage = new BotMessage(
                    chatId: chatId,
                    messageId: message.MessageId,
                    botMessageType: BotMessageType.Text
                    );
                await _handlerService.SetLastMessage(botMessage);

                _logger.Info($"Bot sent a message with id= {message.MessageId} to chat with id= {chatId}");
                return;
            }
            catch (Exception ex)
            {
                _logger.Error($"Возникла ошибка: при обработке сообщения с данными:{messageData}", ex);
            }
        }

        private async Task ShowSendMessagePage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var lastMessage = options.Message;
            var lastMessageId = lastMessage.MessageId;

            try
            {
                var clientPage = (TextPage)_pageManager.CreatePage(PageName.SendMessagePage);

                var keyboard = _keyboardFactory.CreateFeedbackWebAppKeyboard();
                var message = await clientPage.Update(
                         client: client,
                         messageId: lastMessageId,
                         chatId: chatId,
                         keyboard: keyboard
                         );

                var botMessage = new BotMessage(
                    chatId: chatId,
                    messageId: message.MessageId,
                    botMessageType: BotMessageType.Text
                    );
                await _handlerService.SetLastMessage(botMessage);

                _logger.Info($"Bot sent a message with id= {message.MessageId} to chat with id= {chatId}");
                return;
            }
            catch (Exception ex)
            {
                _logger.Error($"Возникла ошибка: при обработке сообщения с данными:{messageData}", ex);
            }
        }
    }
}
