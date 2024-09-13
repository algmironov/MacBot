using System.Text;

using MacBot.ConsoleApp.Keyboards;
using MacBot.ConsoleApp.Models;
using MacBot.ConsoleApp.Pages;
using MacBot.ConsoleApp.Repository;
using MacBot.ConsoleApp.Services;

using SimpleLogger;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MacBot.ConsoleApp.UpdateHandlers
{
    public class MasterUpdatesHandler : IMasterUpdateHandler
    {

        #region Fields
        private readonly int _defaultInviteLinkLength = 10;
        private Logger _logger;
        private IKeyboardFactory _keyboardFactory;
        private IPageMessagesManager _messagesManager;
        private IUserRepository _userRepository;
        private ISessionRepository _sessionRepository;
        private ICodeStorage _codeStorage;
        private IPageManager _pageManager;
        private ICardRepository _cardRepository;
        private IDeckRepository _deckRepository;
        private IBotMessagesRepository _botMessagesRepository;
        private ISessionParametersRepository _sessionParametersRepository;
        private IDeckService _deckService;
        private IHandlerService _handlerService;
        private IObjectStorageService _objectStorageService;
        #endregion


        #region CTOR
        public MasterUpdatesHandler(IKeyboardFactory keyboardFactory,
                                    IPageMessagesManager pageMessagesManager,
                                    IUserRepository userRepository,
                                    ISessionRepository sessionRepository,
                                    ICodeStorage codeStorage,
                                    IPageManager pageManager,
                                    ICardRepository cardRepository,
                                    IDeckRepository deckRepository,
                                    IBotMessagesRepository botMessagesRepository,
                                    ISessionParametersRepository sessionParametersRepository,
                                    IDeckService deckService,
                                    IHandlerService handlerService,
                                    IObjectStorageService objectStorageService,
                                    Logger logger)
        {
            _logger = logger;
            _keyboardFactory = keyboardFactory;
            _messagesManager = pageMessagesManager;
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;
            _codeStorage = codeStorage;
            _pageManager = pageManager;
            _cardRepository = cardRepository;
            _deckRepository = deckRepository;
            _botMessagesRepository = botMessagesRepository;
            _sessionParametersRepository = sessionParametersRepository;
            _deckService = deckService;
            _handlerService = handlerService;
            _objectStorageService = objectStorageService;
        } 
        #endregion

        public async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            await HandleMasterUpdates(client, update);
        }

        private async Task HandleMasterUpdates(ITelegramBotClient client, Update update)
        {
            var chatId = _handlerService.GetChatId(update);

            var user = await _handlerService.GetOrAddBotUser(update);

            var messageData = update.CallbackQuery.Data;

            var lastMessage = await _handlerService.GetLast(chatId);

            int lastMessageId = lastMessage.MessageId;

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
                { "В главное меню", ShowMainPage },
                { "Психолог", ShowMasterPage },
                { "Клиент", ShowClientPage },
                { "Назад", async (opts) =>
                    {
                        try
                        {
                            await ShowPreviousPage(opts);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"Возникла ошибка при нажатии на кнопку \"Назад\"", ex);
                        }
                    }
                },
                { "Колоды", ShowDecksPage },
                { "Добавить колоду", ShowCreateDeckPage },
                { "Все колоды", ShowAllDecksPage },
                { "История сессий", ShowSessionsHistoryPage },
                { "Экспорт в Excel", ShowExportSessionsHistoryPage },
                { "Поиск по имени", ShowFindByNamePage },
                { "Все сессии", ShowAllSessionsPage },
                { "Новая сессия", ShowCreateNewSessionPage },
                { "Создать ссылку", ShowCreateInviteLinkPage },
                { "Выбор колоды", ShowChooseDeckPage },
                { "Установить продолжительность", ShowSetDurationPage },
                { "Сколько карт показать", ShowCardsCountToShowPage }
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
                { "/cloudDeck", ShowCloudDeckPage },
                { "/cloudCard", ShowCloudCardPage },
                { "/historyOfClient", ShowHistoryOfClientPage },
                { "/showHistory", ShowHistoryPage },
                { "/deckId", ShowDeckIsChosenPage },
                { "/duration", ShowDurationIsSetPage },
                { "/numberOfCards", ShowCardsCountIsChosenPage }
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
                var masterPage = (TextPage)_pageManager.CreatePage(PageName.MasterPage);

                var keyboard = _keyboardFactory.CreateInlineKeyboard(masterPage.Buttons, PageName.WelcomePage);

                var message = await masterPage.Update(
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
                _logger.Error($"Возникла ошибка при обработке сообщения с данными:{options.MessageData}", ex);
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
                var user = options.User;
                await _handlerService.UpdateUserRole(user);

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

        private async Task ShowDecksPage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var lastMessage = options.Message;
            var lastMessageId = lastMessage.MessageId;

            try
            {
                var page = (TextPage)_pageManager.CreatePage(PageName.DecksViewPage);
                var message = await page.Update(
                        client: client,
                        chatId: chatId,
                        messageId: lastMessage.MessageId,
                        keyboard: _keyboardFactory.CreateInlineKeyboard(page.Buttons, PageName.MasterPage)
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

        private async Task ShowCreateDeckPage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var lastMessage = options.Message;
            var lastMessageId = lastMessage.MessageId;

            try
            {
                var predefinedText = "Название колоды: ";
                var buttonText = "Ввести название колоды";

                var keyboard = new InlineKeyboardMarkup(new[]
                {
                     InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(buttonText, predefinedText)
                });

                var page = (TextPage)_pageManager.CreatePage(PageName.AddDeckPage);
                var message = await page.Update(
                        client: client,
                        chatId: chatId,
                        messageId: lastMessage.MessageId,
                        keyboard: keyboard
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

        private async Task ShowAllDecksPage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var lastMessage = options.Message;
            var lastMessageId = lastMessage.MessageId;

            try
            {
                var buttons = await _deckService.ListDecksAsync();

                var keyboard = _keyboardFactory.CreateCloudDecksKeyboard(buttons, "pageName DecksViewPage");

                var page = (TextPage)_pageManager.CreatePage(PageName.DecksViewPage);
                page.Text = "Все колоды";

                var message = await page.Update(
                    client: client,
                    chatId: chatId,
                    messageId: lastMessageId,
                    keyboard: keyboard
                    );

                lastMessage.MessageId = message.MessageId;
                await _handlerService.SetLastMessage(lastMessage);

                _logger.Info($"Bot sent a message with id= {message.MessageId} to chat with id= {chatId}");
                return;
            }
            catch (Exception e)
            {
                _logger.Error($"Возникла ошибка при отправке сообщения со всеми колодами из облака", e);
            }
            return;
        }

        private async Task ShowCloudDeckPage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var lastMessage = options.Message;
            var lastMessageId = lastMessage.MessageId;

            try
            {
                if (lastMessage.MessageType == BotMessageType.Image)
                {
                    await _handlerService.DeleteLastMessage(client, lastMessage);

                    _logger.Info($"Бот удалил сообщение с изображением: id= {lastMessage.Id} из чата: id= {chatId}");
                }


            }
            catch (Exception ex)
            {
                _logger.Error("Возникла ошибка при удалении последнего сообщения с изображением", ex);
            }

            try
            {
                var folderName = messageData.Split(' ')[1];

                var cards = await _cardRepository.GetAllByDeckNameAsync(folderName);

                var keyboard = _keyboardFactory.CreateCloudCardsKeyboard(cards, "pageName DecksViewPage");

                var page = (TextPage)_pageManager.CreatePage(PageName.ShowAllCardsFromDeckPage);

                var text = $"{page.Text} {folderName}";

                var message = await page.Update(
                        client: client,
                        chatId: chatId,
                        messageId: lastMessageId,
                        keyboard: keyboard
                        );

                lastMessage.MessageId = message.MessageId;
                await _handlerService.SetLastMessage(lastMessage);

                _logger.Info($"Bot sent a message with id= {message.MessageId} to chat with id= {chatId}");
                return;

            }
            catch (Exception e)
            {
                _logger.Error($"Возникла ошибка при отправке сообщения со всеми колодами из облака", e);
            }
        }

        private async Task ShowCloudCardPage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var lastMessage = options.Message;
            var lastMessageId = lastMessage.MessageId;

            try
            {
                var deckId = Guid.Parse(messageData.Split(" ")[1]);
                var folder = await _deckService.GetDeckAsync(deckId);
                var card = await _cardRepository.TryGetAsync(folder, int.Parse(messageData.Split(" ")[2]));
                var fileName = card.Link.Split("/").Last();

                var page = (ImagePage)_pageManager.CreatePage(PageName.ShowCardFromDeckPage);
                page.ImageStream = await _objectStorageService.GetFileAsync(folder.Name, fileName);

                var keyboard = _keyboardFactory.CreateBackButtonKeyboard(Constants.ShowCardFromDeckPageButtons, "cloudDeck", folder.Name);

                var message = await page.ShowStream(
                    client: client,
                    chatId: chatId,
                    keyboard: keyboard
                    );

                await _handlerService.DeleteLastMessage(client, lastMessage);

                var imageMessage = new BotMessage(message.MessageId, chatId, BotMessageType.Image);

                await _handlerService.SetLastMessage(imageMessage);

                _logger.Info($"Bot sent a message with id= {message.MessageId} to chat with id= {chatId}");
                return;
            }
            catch (Exception ex)
            {
                _logger.Error($"Возникла ошибка при отправке карты", ex);
            }
        }

        private async Task ShowSessionsHistoryPage(CreatePageOptions options)
        {
            var client = options.Client;
            var chatId = options.ChatId;
            var lastMessage = options.Message;

            try
            {
                var page = (TextPage)_pageManager.CreatePage(PageName.SessionHistoryPage);

                var message = await page.Update(
                    client: client,
                    chatId: chatId,
                    messageId: lastMessage.MessageId,
                    keyboard: _keyboardFactory.CreateInlineKeyboard(page.Buttons, page.PreviousPage)
                    );

                lastMessage.MessageId = message.MessageId;
                await _handlerService.SetLastMessage(lastMessage);

                _logger.Info($"Bot sent a message with id= {message.MessageId} to chat with id= {chatId}");
                return;
            }
            catch (Exception e)
            {
                _logger.Error($"Возникла ошибка при отправке сообщения", e);
            }
        }

        private async Task ShowExportSessionsHistoryPage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var lastMessage = options.Message;
            var lastMessageId = lastMessage.MessageId;
            var user = options.User;
            try
            {
                await _handlerService.DeleteLastMessage(client, lastMessage);

                var page = (FilePage)_pageManager.CreatePage(PageName.ExportHistoryPage);
                var keyboard = _keyboardFactory.CreateInlineKeyboard(page.Buttons, page.PreviousPage);

                var sessions = await _sessionRepository.GetAllForExport(user);
                using var export = SessionExporter.ExportSessionsToExcel(sessions.ToList());

                page.File = export;
                page.FileName = $"{user.Name}'s_session_history.xlsx";

                var message = await page.Show(
                    client: client,
                    chatId: chatId,
                    keyboard: _keyboardFactory.CreateInlineKeyboard(page.Buttons, page.PreviousPage)
                    );

                lastMessage.MessageId = message.MessageId;
                lastMessage.MessageType = BotMessageType.File;

                await _handlerService.SetLastMessage(lastMessage);

                _logger.Info($"Бот отправил историю сессий сообщением с id= {message.MessageId} в чат id= {chatId}");
                return;
            }
            catch (Exception e)
            {
                _logger.Error($"Возникла ошибка при экспорте истории сессий'", e);
            }
        }

        private async Task ShowFindByNamePage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var lastMessage = options.Message;
            var lastMessageId = lastMessage.MessageId;
            var user = options.User;

            try
            {
                var sessions = await _sessionRepository.GetAllByMaster(user);
                var clients = new Dictionary<Guid, string>();

                foreach (var session in sessions)
                {
                    var masterClient = await _userRepository.GetByIdAsync(session.ClientId);
                    if (!clients.ContainsKey(masterClient.Id))
                    {
                        clients.Add(masterClient.Id, masterClient.Name);
                    }
                }

                var keyboard = _keyboardFactory.CreateClientsFromHistoryKeyboard(clients);
                var page = (TextPage)_pageManager.CreatePage(PageName.SessionsByClientPage);

                var message = await page.Update(
                    client: client,
                    chatId: chatId,
                    messageId: lastMessage.MessageId,
                    keyboard: keyboard
                    );

                lastMessage.MessageId = message.MessageId;
                await _handlerService.SetLastMessage(lastMessage);

                _logger.Info($"Bot sent a message with id= {message.MessageId} to chat with id= {chatId}");
                return;
            }
            catch (Exception e)
            {
                _logger.Error($"Возникла ошибка при формировании страницы \"Поиск по имени\"", e);
            }
        }

        private async Task ShowAllSessionsPage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var lastMessage = options.Message;
            var lastMessageId = lastMessage.MessageId;
            var user = options.User;

            try
            {
                var sessions = await _sessionRepository.GetAllByMaster(user);
                var sessionsToShow = new Dictionary<Guid, string>();

                foreach (var session in sessions)
                {
                    var date = session.Date.ToString("dd-MM-yyyy");
                    var clientName = await _userRepository.GetByIdAsync(session.ClientId);
                    var button = $"{date} клиент: {clientName.Name}";

                    sessionsToShow.Add(session.SessionId, button);
                }

                var keyboard = _keyboardFactory.CreateAllSessionsKeyboard(sessionsToShow);
                var page = (TextPage)_pageManager.CreatePage(PageName.AllSessionsPage);

                var message = await page.Update(
                    client: client,
                    chatId: chatId,
                    messageId: lastMessage.MessageId,
                    keyboard: keyboard
                    );

                lastMessage.MessageId = message.MessageId;
                await _handlerService.SetLastMessage(lastMessage);

                _logger.Info($"Bot sent a message with id= {message.MessageId} to chat with id= {chatId}");
                return;
            }
            catch (Exception e)
            {
                _logger.Error($"Возникла ошибка при формировании страницы \"Все сессии\"", e);
            }
        }

        private async Task ShowHistoryOfClientPage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var lastMessage = options.Message;
            var lastMessageId = lastMessage.MessageId;
            var user = options.User;

            try
            {
                var clientId = Guid.Parse(messageData.Split(' ')[1]);
                var masterClient = await _userRepository.GetByIdAsync(clientId);
                var sessions = await _sessionRepository.GetAllByMasterAndClient(user, masterClient);

                var sessionsToShow = new Dictionary<Guid, string>();
                foreach (var session in sessions)
                {
                    var date = session.Date.ToString("dd-MM-yyyy");
                    var button = $"Дата: {date} Продолжительность: {session.Duration}";

                    sessionsToShow.Add(session.SessionId, button);
                }

                var keyboard = _keyboardFactory.CreateAllSessionsKeyboard(sessionsToShow);
                var page = (TextPage)_pageManager.CreatePage(PageName.AllSessionsPage);

                var message = await page.Update(
                    client: client,
                    chatId: chatId,
                    messageId: lastMessage.MessageId,
                    keyboard: keyboard
                    );

                lastMessage.MessageId = message.MessageId;
                await _handlerService.SetLastMessage(lastMessage);

                _logger.Info($"Bot sent a message with id= {message.MessageId} to chat with id= {chatId}");
                return;
            }
            catch (Exception ex)
            {
                _logger.Error($"Возникла ошибка при формировании списка сессий клиента", ex);
            }
        }

        private async Task ShowHistoryPage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var lastMessage = options.Message;
            var lastMessageId = lastMessage.MessageId;
            var user = options.User;

            try
            {
                var session = await _sessionRepository.GetByIdAsync(Guid.Parse(messageData.Split(' ')[1]));
                var sessionClient = await _userRepository.GetByIdAsync(session.ClientId);

                var cards = await _cardRepository.GetAllBySessionCards(session.ChoosenCards);
                var cardLinks = string.Empty;
                var sb = new StringBuilder();
                foreach (var card in cards)
                {
                    sb.AppendLine(card.Link);
                }
                cardLinks = sb.ToString();

                var messageText = $"Дата: {session.Date.ToString("dd-MM-yyyy")} \nКлиент: {sessionClient.Name}\nПродолжительность: {session.Duration}\nПоказанные карты: {cardLinks}";

                var page = (TextPage)_pageManager.CreatePage(PageName.SessionFromHistoryPage);
                page.Text = messageText;

                var message = await page.Update(
                    client: client,
                    chatId: chatId,
                    messageId: lastMessage.MessageId,
                    keyboard: _keyboardFactory.CreateBackButtonKeyboard(Constants.SessionFromHistoryButtons, PageName.SessionHistoryPage)
                    );

                lastMessage.MessageId = message.MessageId;
                await _handlerService.SetLastMessage(lastMessage);

                _logger.Info($"Bot sent a message with id= {message.MessageId} to chat with id= {chatId}");
                return;
            }
            catch (Exception ex)
            {
                _logger.Error($"Возникла ошибка при отображения сессии из истории", ex);
            }
        }

        private async Task ShowCreateNewSessionPage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var lastMessage = options.Message;
            var lastMessageId = lastMessage.MessageId;
            var user = options.User;

            try
            {
                var page = (TextPage)_pageManager.CreatePage(PageName.NewSessionPage);
                var message = await page.Update(
                    client: client,
                    chatId: chatId,
                    messageId: lastMessage.MessageId,
                    keyboard: _keyboardFactory.CreateInlineKeyboard(page.Buttons, PageName.MasterPage)
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

        private async Task ShowCreateInviteLinkPage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var lastMessage = options.Message;
            var lastMessageId = lastMessage.MessageId;
            var user = options.User;

            try
            {
                var generatedLink = InviteLinkGenerator.GenerateInviteUrl(_defaultInviteLinkLength);
                var inviteLink = new InviteCode(user, generatedLink);

                await _codeStorage.AddAsync(inviteLink);

                var page = (TextPage)_pageManager.CreatePage(PageName.CreateInviteLinkPage);
                page.Text = generatedLink;

                var message = await page.Update(
                    client: client,
                    chatId: chatId,
                    messageId: lastMessage.MessageId,
                    keyboard: _keyboardFactory.CreateInlineKeyboard(page.Buttons, PageName.MasterPage)
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

        private async Task ShowChooseDeckPage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var lastMessage = options.Message;
            var lastMessageId = lastMessage.MessageId;
            var user = options.User;

            try
            {
                var page = (TextPage)_pageManager.CreatePage(PageName.ChooseDeckPage);

                var message = await page.Update(
                    client: client,
                    chatId: chatId,
                    messageId: lastMessage.MessageId,
                    keyboard: _keyboardFactory.CreateAllDecksKeyboard(await _deckService.GetDecksAsync())
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

        private async Task ShowDeckIsChosenPage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var lastMessage = options.Message;
            var lastMessageId = lastMessage.MessageId;
            var user = options.User;

            try
            {
                Guid.TryParse(messageData.Split(' ')[1], out Guid deckId);
                var deck = await _deckRepository.FindAsync(deckId);

                var parameters = new SessionParameters(masterId: user.Id);
                parameters.DeckId = deckId;
                await _sessionParametersRepository.AddAsync(parameters);

                var page = (TextPage)_pageManager.CreatePage(PageName.NewSessionPage);
                page.Text = $"Выбрана колода: {deck.Name}";
                var message = await page.Update(
                    client: client,
                    chatId: chatId,
                    messageId: lastMessage.MessageId,
                    keyboard: _keyboardFactory.CreateInlineKeyboard(page.Buttons, PageName.MasterPage)
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

        private async Task ShowSetDurationPage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var lastMessage = options.Message;
            var lastMessageId = lastMessage.MessageId;
            var user = options.User;

            try
            {
                var page = (TextPage)_pageManager.CreatePage(PageName.SetSessionDurationPage);

                var message = await page.Update(
                    client: client,
                    chatId: chatId,
                    messageId: lastMessage.MessageId,
                    keyboard: _keyboardFactory.CreateKeyboardWithCallbacks(_handlerService.GetButtonsWithCallbacks(page.Buttons, _messagesManager.GetPrefix("Duration")))
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

        private async Task ShowDurationIsSetPage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var lastMessage = options.Message;
            var lastMessageId = lastMessage.MessageId;
            var user = options.User;

            try
            {
                int.TryParse(messageData.Split(' ')[1], out int duration);
                var sessionParameters = await _sessionParametersRepository.GetByMasterId(user.Id);
                sessionParameters.Duration = duration;

                var page = (TextPage)_pageManager.CreatePage(PageName.NewSessionPage);
                page.Text = $"Установлена продолжительность сессии: {duration} минут";
                var message = await page.Update(
                    client: client,
                    chatId: chatId,
                    messageId: lastMessage.MessageId,
                    keyboard: _keyboardFactory.CreateInlineKeyboard(page.Buttons, PageName.MasterPage)
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

        private async Task ShowCardsCountToShowPage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var lastMessage = options.Message;
            var lastMessageId = lastMessage.MessageId;
            var user = options.User;

            try
            {

                var page = (TextPage)_pageManager.CreatePage(PageName.SetCardsAmountToShowPage);

                var message = await page.Update(
                    client: client,
                    chatId: chatId,
                    messageId: lastMessage.MessageId,
                    keyboard: _keyboardFactory.CreateChooseCardsAmountKeyboard(page.Buttons)
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

        private async Task ShowCardsCountIsChosenPage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var lastMessage = options.Message;
            var lastMessageId = lastMessage.MessageId;
            var user = options.User;

            try
            {
                _ = int.TryParse(messageData.Split(' ')[1], out int cardsAmount);
                var sessionParameters = await _sessionParametersRepository.GetByMasterId(user.Id);
                sessionParameters.CardsToShowCount = cardsAmount;

                var page = (TextPage)_pageManager.CreatePage(PageName.NewSessionPage);

                page.Text = $"Установлено количество карт для показа: {cardsAmount}";

                var message = await page.Update(
                    client: client,
                    chatId: chatId,
                    messageId: lastMessage.MessageId,
                    keyboard: _keyboardFactory.CreateInlineKeyboard(page.Buttons, PageName.MasterPage)
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
    }
}
