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
    public class ClientActiveSessionUpdatesHandler : IClientActiveSessionUpdateHandler
    {
        private Logger _logger;
        private IKeyboardFactory _keyboardFactory;
        private IPageMessagesManager _messagesManager;
        private IUserRepository _userRepository;
        private ISessionRepository _sessionRepository;
        private IPageManager _pageManager;
        private ICardRepository _cardRepository;
        private IBotMessagesRepository _botMessagesRepository;
        private ISessionParametersRepository _sessionParametersRepository;
        private ISessionCardRepository _sessionCardRepository;
        private IHandlerService _handlerService;

        public ClientActiveSessionUpdatesHandler(IKeyboardFactory keyboardFactory,
                                    IPageMessagesManager pageMessagesManager,
                                    IUserRepository userRepository,
                                    ISessionRepository sessionRepository,
                                    IPageManager pageManager,
                                    ICardRepository cardRepository,
                                    IBotMessagesRepository botMessagesRepository,
                                    ISessionParametersRepository sessionParametersRepository,
                                    ISessionCardRepository sessionCardRepository,
                                    IHandlerService handlerService,
                                    Logger logger)
        {
            _logger = logger;
            _keyboardFactory = keyboardFactory;
            _messagesManager = pageMessagesManager;
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;
            _pageManager = pageManager;
            _cardRepository = cardRepository;
            _botMessagesRepository = botMessagesRepository;
            _sessionParametersRepository = sessionParametersRepository;
            _sessionCardRepository = sessionCardRepository;
            _handlerService = handlerService;

        }
        public async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            await HandleClientActiveSession(client, update);
        }

        private async Task HandleClientActiveSession(ITelegramBotClient client, Update update)
        {
            var chatId = _handlerService.GetChatId(update);
            var user = await _handlerService.GetOrAddBotUser(update);
            var messageData = update.CallbackQuery.Data;
            var lastMessage = await _handlerService.GetLast(chatId);
            var session = await _sessionRepository.GetActiveByUser(user);
            var parameters = await _sessionParametersRepository.GetByMasterId(session.MasterId);

            if (messageData.StartsWith("/chooseCard"))
            {
                await HandleChooseCard(client, chatId, user, messageData, lastMessage, session, parameters);
            }
            else if (messageData.StartsWith("/removeCard"))
            {
                await HandleRemoveCard(client, chatId, user, messageData, lastMessage, session);
            }
            else if (messageData == "/finishedCardSelection")
            {
                await HandleFinishedCardSelection(client, chatId, lastMessage, session);
            }
        }

        private async Task HandleChooseCard(ITelegramBotClient client, long chatId, BotUser user, string messageData, BotMessage lastMessage, Session session, SessionParameters parameters)
        {
            try
            {
                var (cardNumber, cardId) = ParseCardData(messageData);
                var card = await _cardRepository.TryGetAsync(cardId);
                var sessionCard = new SessionCard { Session = session, Card = card };

                await _sessionCardRepository.AddAsync(sessionCard);
                await _sessionRepository.UpdateAsync(session);

                var selectCardsToShowPage = CreateSelectCardsToShowPage((int)(parameters.CardsToShowCount - session.ChoosenCards.Count));
                var keyboard = await _keyboardFactory.UpdateInlineKeyboard(cardNumber, user.ChatId);

                var message = await selectCardsToShowPage.Update(client, chatId, lastMessage.MessageId, keyboard);
                await UpdateLastMessage(lastMessage, message.MessageId);

                if (session.ChoosenCards.Count >= parameters.CardsToShowCount)
                {
                    selectCardsToShowPage.Text = _messagesManager.GetPrefix("ChoiceFinished");
                    message = await selectCardsToShowPage.Update(client, chatId, lastMessage.MessageId, keyboard);
                    await UpdateLastMessage(lastMessage, message.MessageId);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Возникла ошибка при обработке выбора карты", e);
            }
        }

        private async Task HandleRemoveCard(ITelegramBotClient client, long chatId, BotUser user, string messageData, BotMessage lastMessage, Session session)
        {
            try
            {
                var (cardNumber, cardId) = ParseCardData(messageData);
                var sessionCards = await _sessionCardRepository.GetSessionCardsAsync(session.SessionId);
                var sessionCard = sessionCards.First(sc => sc.CardId == cardId);

                session.ChoosenCards.Remove(sessionCard);
                await _sessionRepository.UpdateAsync(session);
                await _sessionCardRepository.RemoveAsync(session.SessionId, cardId);

                var selectCardsToShowPage = CreateSelectCardsToShowPage();
                selectCardsToShowPage.Text = _messagesManager.GetPrefix("CardChoiseAborted");
                var keyboard = await _keyboardFactory.UpdateInlineKeyboard(cardNumber, user.ChatId);

                var message = await selectCardsToShowPage.Update(client, chatId, lastMessage.MessageId, keyboard);
                await UpdateLastMessage(lastMessage, message.MessageId);
            }
            catch (Exception e)
            {
                _logger.Error("Возникла ошибка при удалении карты", e);
            }
        }

        private async Task HandleFinishedCardSelection(ITelegramBotClient client, long chatId, BotMessage lastMessage, Session session)
        {
            try
            {
                var master = await _userRepository.GetByIdAsync(session.MasterId);
                var masterLastMessage = await _botMessagesRepository.GetLast(master.ChatId);

                var sendCardToShowPage = (TextPage)_pageManager.CreatePage(PageName.SendCardToShowPage);
                var masterKeyboard = _keyboardFactory.CreateInlineKeyboard(sendCardToShowPage.Buttons);
                var masterMessage = await sendCardToShowPage.Update(client, master.ChatId, masterLastMessage.MessageId, masterKeyboard);

                var clientPage = (TextPage)_pageManager.CreatePage(PageName.AwaitMasterToShowcardPage);
                var clientMessage = await clientPage.Update(client, chatId, lastMessage.MessageId, null);

                await UpdateLastMessages(masterLastMessage, masterMessage.MessageId, lastMessage, clientMessage.MessageId);
            }
            catch (Exception e)
            {
                _logger.Error("Возникла ошибка при завершении выбора карт", e);
            }
        }

        private (int cardNumber, Guid cardId) ParseCardData(string messageData)
        {
            var callBack = messageData.Split('_');
            int.TryParse(callBack[1], out int cardNumber);
            var cardId = Guid.Parse(callBack[2]);
            return (cardNumber, cardId);
        }

        private TextPage CreateSelectCardsToShowPage(int remainingCards = 0)
        {
            var page = (TextPage)_pageManager.CreatePage(PageName.SelectCardsToShowPage);
            if (remainingCards > 0)
            {
                page.Text += $"\n Осталось выбрать {remainingCards}";
                page.Text = _messagesManager.GetPrefix("ChooseAnotherCard");
            }
            return page;
        }

        private async Task UpdateLastMessage(BotMessage lastMessage, int messageId)
        {
            lastMessage.MessageId = messageId;
            await _handlerService.SetLastMessage(lastMessage);
        }

        private async Task UpdateLastMessages(BotMessage masterLastMessage, int masterMessageId, BotMessage clientLastMessage, int clientMessageId)
        {
            masterLastMessage.MessageId = masterMessageId;
            await _handlerService.SetLastMessage(masterLastMessage);

            clientLastMessage.MessageId = clientMessageId;
            await _handlerService.SetLastMessage(clientLastMessage);
        }

    }
}
