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
    public class MasterActiveSessionUpdatesHandler : IMasterActiveSessionUpdateHandler
    {
        private Logger _logger;
        private IKeyboardFactory _keyboardFactory;
        private IUserRepository _userRepository;
        private ISessionRepository _sessionRepository;
        private IPageManager _pageManager;
        private ICardRepository _cardRepository;
        private IDeckRepository _deckRepository;
        private IBotMessagesRepository _botMessagesRepository;
        private ISessionParametersRepository _sessionParametersRepository;
        private ISessionCardRepository _sessionCardRepository;
        private IDeckService _deckService;
        private IHandlerService _handlerService;
        private IObjectStorageService _objectStorageService;

        public MasterActiveSessionUpdatesHandler(IKeyboardFactory keyboardFactory,
                                    IUserRepository userRepository,
                                    ISessionRepository sessionRepository,
                                    IPageManager pageManager,
                                    ICardRepository cardRepository,
                                    IDeckRepository deckRepository,
                                    IBotMessagesRepository botMessagesRepository,
                                    ISessionParametersRepository sessionParametersRepository,
                                    ISessionCardRepository sessionCardRepository,
                                    IDeckService deckService,
                                    IHandlerService handlerService,
                                    IObjectStorageService objectStorageService,
                                    Logger logger)
        {
            _logger = logger;
            _keyboardFactory = keyboardFactory;
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;
            _pageManager = pageManager;
            _cardRepository = cardRepository;
            _deckRepository = deckRepository;
            _botMessagesRepository = botMessagesRepository;
            _sessionParametersRepository = sessionParametersRepository;
            _sessionCardRepository = sessionCardRepository;
            _deckService = deckService;
            _handlerService = handlerService;
            _objectStorageService = objectStorageService;

        }
        public async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            await HandleMasterActiveSession(client, update);
        }

        private async Task HandleMasterActiveSession(ITelegramBotClient client, Update update)
        {
            var chatId = _handlerService.GetChatId(update);
            var user = await _handlerService.GetOrAddBotUser(update);
            var messageData = update.CallbackQuery.Data;
            var lastMessage = await _handlerService.GetLast(chatId);
            var session = await _sessionRepository.GetActiveSession(user);

            var sessionClient = await _userRepository.GetByIdAsync(session.ClientId);
            var clientLastMessage = await _handlerService.GetLast(sessionClient.ChatId);

            var options = new CreatePageOptions
            {
                ChatId = chatId,
                Client = client,
                User = user,
                MessageData = messageData,
                Update = update,
                SessionClient = sessionClient,
                Message = lastMessage,
                ClientMessage = clientLastMessage,
                Session = session
            };

            if (messageData == "Начать сессию")
            {
                await ShowStartSessionPage(options);
                return;
            }

            if (messageData == "Отправить карту" || messageData == "Отправить еще одну карту")
            {
                await ShowSendCardPage(options);
                return;
            }

            if (messageData == "Завершить сессию")
            {
                await ShowFinishSessionPage(options);
                return;
            }
        }

        private async Task ShowStartSessionPage(CreatePageOptions options)
        {
            try
            {
                var parameters = await _sessionParametersRepository.GetByMasterId(options.User.Id);
                var userClient = await _userRepository.GetByIdAsync(options.Session.ClientId);
                var deck = await _deckRepository.GetDeckWithCards(id: (Guid)parameters.DeckId);
                var cards = deck.Cards.Select(card => card.Id.ToString()).ToList();
                var userClientLastMessage = await _botMessagesRepository.GetLast(userClient.ChatId);

                var clientPage = (TextPage)_pageManager.CreatePage(PageName.SelectCardsToShowPage);
                var clientKeyboard = _keyboardFactory.ChooseCardsKeyboard(cards, userClient.ChatId);

                var clientMessage = await UpdateOrShowMessage(clientPage, options.Client, userClient.ChatId, userClientLastMessage, (InlineKeyboardMarkup)clientKeyboard);

                var newClientMessage = new BotMessage(clientMessage.MessageId, userClient.ChatId, BotMessageType.Text);
                await _handlerService.SetLastMessage(newClientMessage);

                var masterPage = (TextPage)_pageManager.CreatePage(PageName.AwaitClientToChooseCardsPage);
                var masterMessage = await masterPage.Update(options.Client, options.ChatId, options.Message.MessageId, _keyboardFactory.CreateInlineKeyboard(masterPage.Buttons));

                options.Message.MessageId = masterMessage.MessageId;
                await _handlerService.SetLastMessage(options.Message);
            }
            catch (Exception e)
            {
                _logger.Error($"Возникла ошибка при начале сессии", e);
            }

        }

        private async Task<Message> UpdateOrShowMessage(TextPage page, ITelegramBotClient client, long chatId, BotMessage lastMessage, InlineKeyboardMarkup keyboard)
        {
            if (lastMessage != null && lastMessage.IsLast)
            {
                return await page.Update(client, chatId, lastMessage.MessageId, keyboard);
            }
            else
            {
                if (lastMessage != null)
                {
                    await _handlerService.DeleteLastMessage(client, lastMessage);
                }
                return await page.Show(client, chatId, keyboard);
            }
        }

        private async Task ShowSendCardPage(CreatePageOptions options)
        {
            try
            {
                var sessionCards = await _sessionCardRepository.GetSessionCardsAsync(options.Session.SessionId);
                var cardsToShow = sessionCards.Where(sc => !sc.IsShown).ToHashSet();

                if (cardsToShow.Count > 0)
                {
                    await ShowCards(options, cardsToShow);
                }
                else
                {
                    await ShowAdditionalCard(options);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Возникла ошибка при обработке сообщения с данными: {options.MessageData}", e);
            }
        }

        private async Task ShowCards(CreatePageOptions options, HashSet<SessionCard> cardsToShow)
        {
            var cards = await _cardRepository.GetAllBySessionCards(cardsToShow);
            var card = cards.First();
            var deck = await _deckService.GetDeckByCardAsync(card);

            using var masterStream = await _objectStorageService.GetFileAsync(deck.Name, card.Link);
            using var clientStream = await _objectStorageService.GetFileAsync(deck.Name, card.Link);

            var clientPage = (ImagePage)_pageManager.CreatePage(PageName.ShowCardForClientPage);
            var masterPage = (ImagePage)_pageManager.CreatePage(cardsToShow.Count > 1 ? PageName.ShowCardForMasterPage : PageName.FinalCardShownPage);

            clientPage.ImageStream = clientStream;
            masterPage.ImageStream = masterStream;

            var masterKeyboard = _keyboardFactory.CreateInlineKeyboard(Constants.ShowCardForMasterPageButtons);

            await SendMessageAndUpdateState(options, clientPage, masterPage, masterKeyboard, card);
        }

        private async Task ShowAdditionalCard(CreatePageOptions options)
        {
            try
            {
                var shownCards = await GetShownCards(options.Session.ChoosenCards);
                var deck = await _deckService.GetDeckAsync((Guid)shownCards.First().DeckId!);
                var allCards = await _cardRepository.GetAllByDeckNameAsync(deck.Name);
                var remainingCards = allCards.Except(shownCards).ToList();

                var random = new Random();
                var additionalCard = remainingCards[random.Next(remainingCards.Count)];

                var sessionCard = new SessionCard { Session = options.Session, CardId = additionalCard.Id };
                await _sessionCardRepository.AddAsync(sessionCard);

                var clientPage = (ImagePage)_pageManager.CreatePage(PageName.ShowCardForClientPage);
                var masterPage = (ImagePage)_pageManager.CreatePage(PageName.ShowCardForMasterPage);

                using var masterStream = await _objectStorageService.GetFileAsync(deck.Name, additionalCard.Link);
                using var clientStream = await _objectStorageService.GetFileAsync(deck.Name, additionalCard.Link);

                clientPage.ImageStream = clientStream;
                masterPage.ImageStream = masterStream;

                var masterKeyboard = _keyboardFactory.CreateInlineKeyboard(Constants.ShowCardForMasterPageButtons);

                await SendMessageAndUpdateState(options, clientPage, masterPage, masterKeyboard, additionalCard);

                _logger.Info($"Была показана дополнительная карта в чате id={options.ChatId}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Возникла ошибка при отправке дополнительной карты в чат id= {options.ChatId}", ex);
            }
        }

        private async Task<List<Card>> GetShownCards(IEnumerable<SessionCard> choosenCards)
        {
            var shownCards = new List<Card>();
            foreach (var sc in choosenCards)
            {
                shownCards.Add(await _cardRepository.TryGetAsync(sc.CardId));
            }
            return shownCards;
        }

        private async Task SendMessageAndUpdateState(CreatePageOptions options, ImagePage clientPage, ImagePage masterPage, IReplyMarkup masterKeyboard, Card card)
        {
            if (options.Message.MessageType == BotMessageType.Image)
            {
                await _handlerService.RemoveImagePageKeyboard(options.Client, options.ChatId, options.Message.MessageId);
            }

            var clientMessage = await clientPage.ShowStream(options.Client, options.SessionClient.ChatId);
            _logger.Info($"Bot sent a message with id= {clientMessage.MessageId} to chat with id= {options.SessionClient.ChatId}");

            var masterMessage = await masterPage.ShowStream(options.Client, options.ChatId, masterKeyboard);
            _logger.Info($"Bot sent a message with id= {masterMessage.MessageId} to chat with id= {options.ChatId}");

            if (options.Message.MessageType != BotMessageType.Image)
            {
                await _pageManager.DeletePage(options.Client, options.ChatId, options.Message.MessageId);
                _logger.Info($"Bot DELETED a message with id= {options.Message.MessageId} at chat with id= {options.ChatId}");
            }

            if (options.ClientMessage.MessageType != BotMessageType.Image)
            {
                await _pageManager.DeletePage(options.Client, options.SessionClient.ChatId, options.ClientMessage.MessageId);
                _logger.Info($"Bot DELETED a message with id= {clientMessage.MessageId} at chat with id= {options.SessionClient.ChatId}");
            }

            options.Message.IsLast = false;
            options.ClientMessage.IsLast = false;

            await _botMessagesRepository.UpdateAsync(options.ClientMessage);
            await _botMessagesRepository.UpdateAsync(options.Message);

            var masterLastMessage = new BotMessage
            {
                MessageId = masterMessage.MessageId,
                MessageType = BotMessageType.Image,
                ChatId = options.ChatId
            };
            await _handlerService.SetLastMessage(masterLastMessage);

            var clientLastMessage = new BotMessage
            {
                MessageId = clientMessage.MessageId,
                ChatId = options.SessionClient.ChatId,
                MessageType = BotMessageType.Image
            };
            await _handlerService.SetLastMessage(clientLastMessage);

            var shownCard = await _sessionCardRepository.GetBySessionAndCard(options.Session.SessionId, card.Id);
            shownCard.IsShown = true;
            await _sessionCardRepository.Update(shownCard);
        }

        private async Task ShowFinishSessionPage(CreatePageOptions options)
        {
            try
            {
                options.Session.EndSession();
                await _sessionRepository.UpdateAsync(options.Session);

                var clientPage = CreateTextPage(PageName.SessionIsFinishedForClientPage, Constants.ClientPageButtons, PageName.ClientPage);
                var masterPage = CreateTextPage(PageName.SessionIsFinishedForMasterPage, Constants.MasterButtons, PageName.MasterPage);

                var clientMessage = await ShowPageWithKeyboard(clientPage, options.Client, options.SessionClient.ChatId);
                var masterMessage = await ShowPageWithKeyboard(masterPage, options.Client, options.ChatId);

                await DeleteImageMessages(options.Client, options.SessionClient.ChatId);
                await DeleteImageMessages(options.Client, options.ChatId);

                await UpdateLastMessages(masterMessage, clientMessage, options);
            }
            catch (Exception e)
            {
                _logger.Error($"Возникла ошибка при завершении сессии SessionId= {options.Session.SessionId}", e);
            }
        }

        private TextPage CreateTextPage(PageName pageName, List<string> buttons, PageName buttonPageName)
        {
            var page = (TextPage)_pageManager.CreatePage(pageName);
            page.Keyboard = _keyboardFactory.CreateInlineKeyboard(buttons, buttonPageName);
            return page;
        }

        private async Task<Message> ShowPageWithKeyboard(TextPage page, ITelegramBotClient client, long chatId)
        {
            return await page.Show(client: client, chatId: chatId, keyboard: page.Keyboard);
        }

        private async Task DeleteImageMessages(ITelegramBotClient client, long chatId)
        {
            var images = await _botMessagesRepository.GetAllByChatId(chatId);
            var imageMessages = images.Where(message => message.MessageType == BotMessageType.Image && !message.IsDeleted).ToList();
            await _handlerService.DeleteImageMessages(client, imageMessages);
        }

        private async Task UpdateLastMessages(Message masterMessage, Message clientMessage, CreatePageOptions options)
        {
            options.Message.MessageId = masterMessage.MessageId;
            await _handlerService.SetLastMessage(options.Message);

            options.ClientMessage.MessageId = clientMessage.MessageId;
            await _handlerService.SetLastMessage(options.ClientMessage);
        }


    }
}
