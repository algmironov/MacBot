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
    public class TextUpdateHandler : IUpdateHandler
    {
        private string _newDeckName;

        private readonly Logger _logger;
        private readonly IKeyboardFactory _keyboardFactory;
        private readonly IUserRepository _userRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly ICodeStorage _codeStorage;
        private readonly IPageManager _pageManager;
        private readonly IHandlerService _handlerService;
        private readonly IObjectStorageService _objectStorageService;
        private readonly IFeedbackRepository _feedbackRepository;

        public TextUpdateHandler(IKeyboardFactory keyboardFactory,
                                    IUserRepository userRepository,
                                    ISessionRepository sessionRepository,
                                    ICodeStorage codeStorage,
                                    IPageManager pageManager,
                                    IHandlerService handlerService,
                                    IObjectStorageService objectStorageService,
                                    IFeedbackRepository feedbackRepository,
                                    Logger logger)
        {
            _logger = logger;
            _keyboardFactory = keyboardFactory;
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;
            _codeStorage = codeStorage;
            _pageManager = pageManager;
            _handlerService = handlerService;
            _objectStorageService = objectStorageService;
            _feedbackRepository = feedbackRepository;

        }
        public async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            await HandleTextUpdates(client, update, cancellationToken);
        }

        private async Task HandleTextUpdates(ITelegramBotClient client, Update update, CancellationToken token)
        {
            var chatId = update.Message!.From!.Id;
            var messageText = update.Message.Text;

            var user = await _handlerService.GetOrAddBotUser(update);

            if (update.Message.ReplyToMessage != null) 
            {
                var options = new CreatePageOptions()
                {
                    Client = client,
                    MessageData = messageText,
                    Update = update,
                    User = user,
                    ChatId = chatId,
                };
                await _handlerService.ClearChat(client, chatId);
                await ShowMessageSentPage(options);
            }

            if (messageText!.StartsWith("/start"))
            {
                await _handlerService.RemoveUserMessage(client, update);
                await _handlerService.ClearChat(client, chatId);

                var textArgs = messageText.Trim().Split(" ");

                if (textArgs.Length == 2)
                {
                    var code = textArgs[1];
                    user.ActiveRole = Role.Client;
                    await HandleClientStartSession(client, user, code);
                    return;
                }

                if (textArgs.Length == 1)
                {
                    var welcomePage = (TextPage)_pageManager.CreatePage(PageName.WelcomePage);

                    var keyboard = _keyboardFactory.CreateInlineKeyboard(welcomePage.Buttons!);
                    var message = await welcomePage.Show(client, chatId, keyboard);
                    var botMessage = new BotMessage(
                        messageId: message.MessageId,
                        chatId: chatId,
                        botMessageType: BotMessageType.Text);

                    await _handlerService.SetLastMessage(botMessage);

                    _logger.Info($"Bot sent a welcome message with id= {botMessage.MessageId} to chat with id= {chatId}");
                    return;
                }
            }

            if (messageText!.StartsWith("/feedback"))
            {
                var options = new CreatePageOptions()
                {
                    MessageData = messageText,
                    Update = update,
                    User = user,
                    ChatId = chatId,
                };
                await _handlerService.ClearChat(client, chatId);
                await ShowMessageSentPage(options);

            }
            if (messageText.Contains("Название колоды"))
            {
                _newDeckName = messageText.Split(" ")[3].Trim();

                await _objectStorageService.CreateFolderAsync(_newDeckName);

                _logger.Info($"Создана новая папка в облаке: {_newDeckName}");

                return;
            }

        }

        private async Task HandleClientStartSession(ITelegramBotClient client, BotUser user, string code)
        {
            try
            {
                var masterId = await _codeStorage.GetCreatorAsync(code);
                var master = await _userRepository.GetByIdAsync(masterId);

                var session = new Session(master, user);
                await _sessionRepository.AddAsync(session);

                var lastMasterMessage = await _handlerService.GetLast(master.ChatId);

                var clientHasJoinedSessionPage = (TextPage)_pageManager.CreatePage(PageName.ClientHasJoinedSessionPage);

                var notificationMessage = await clientHasJoinedSessionPage.Update(
                                    client: client,
                                    chatId: master.ChatId,
                                    messageId: lastMasterMessage.MessageId,
                                    keyboard: _keyboardFactory.CreateInlineKeyboard(clientHasJoinedSessionPage.Buttons)
                                    );
                lastMasterMessage.MessageId = notificationMessage.MessageId;
                await _handlerService.SetLastMessage(lastMasterMessage);

                var sessionWillStartSoonPage = (TextPage)_pageManager.CreatePage(PageName.SessionWillStartSoonPage);

                var clientMessage = await sessionWillStartSoonPage.Show(
                    client: client,
                    chatId: user.ChatId
                    );

                var botMessage = new BotMessage(
                                    chatId: user.ChatId,
                                    messageId: clientMessage.MessageId,
                                    botMessageType: BotMessageType.Text
                                    );

                await _handlerService.SetLastMessage(botMessage);
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка при начале сессии", ex);
            }
        }

        private async Task ShowMessageSentPage(CreatePageOptions options)
        {
            var client = options.Client;
            var messageData = options.MessageData;
            var chatId = options.ChatId;
            var name = options.User.Name;

            var feedback = new Feedback(chatId, name, messageData);
            await _feedbackRepository.SaveAsync(feedback);

            var clientPage = (TextPage) _pageManager.CreatePage(PageName.MessageSentPage);
            var message = await clientPage.Show(client: client, chatId: chatId);

            await _handlerService.SetLastMessage(new BotMessage(message.MessageId, message.Chat.Id, BotMessageType.Text));
        }
    }
}
