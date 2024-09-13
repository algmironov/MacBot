using MacBot.ConsoleApp.Models;
using MacBot.ConsoleApp.Repository;

using Telegram.Bot.Types.ReplyMarkups;

namespace MacBot.ConsoleApp.Keyboards
{
    public interface IKeyboardFactory
    {
        IReplyMarkup CreateMarkupKeyboard(List<string> buttons);
        IReplyMarkup CreateInlineKeyboard(List<string> buttons);
        IReplyMarkup CreateInlineKeyboard(List<string> buttons, PageName? previousPageName);
        IReplyMarkup ChooseCardsKeyboard(List<string> cards, long chatId);
        Task<IReplyMarkup> UpdateInlineKeyboard(int button, long chatId);
        IReplyMarkup CreateCloudDecksKeyboard(List<string> decks, string backButtonPrefix);
        IReplyMarkup CreateCloudCardsKeyboard(List<string> cards, string backButtonPrefix);
        IReplyMarkup CreateCloudCardsKeyboard(List<Card> cards, string backButtonPrefix);
        IReplyMarkup CreateChooseCardsAmountKeyboard(List<string> buttons);
        IReplyMarkup CreateAllDecksKeyboard(Dictionary<string, Guid> decks);
        IReplyMarkup CreateKeyboardWithCallbacks(Dictionary<string, string> buttons);
        IReplyMarkup CreateClientsFromHistoryKeyboard(Dictionary<Guid, string> clients);
        IReplyMarkup CreateAllSessionsKeyboard(Dictionary<Guid, string> sessions);
        IReplyMarkup CreateBackButtonKeyboard(List<string> button, PageName previousPage);
        IReplyMarkup CreateBackButtonKeyboard(List<string> button, string prefix);
        IReplyMarkup CreateBackButtonKeyboard(List<string> button, string prefix, string data);
        IReplyMarkup CreateForceReplyKeyboard(string placeholder);
        IReplyMarkup CreateUnreadFeedbacksKeyboard(List<Feedback> feedbacks);
        IReplyMarkup CreateFeedbackWebAppKeyboard(); 
    }
}
