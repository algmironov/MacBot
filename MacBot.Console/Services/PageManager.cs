using System.Collections.Concurrent;

using MacBot.ConsoleApp.Pages;
using MacBot.ConsoleApp.Repository;

using Telegram.Bot;

namespace MacBot.ConsoleApp.Services
{
    public class PageManager : IPageManager
    {
        private readonly IPageMessagesManager _messagesManager;
        private readonly ConcurrentDictionary<PageName, Func<Page>> _createPageDictionary;

        public PageManager(IPageMessagesManager pageMessagesManager)
        {
            _messagesManager = pageMessagesManager;
            _createPageDictionary = new ConcurrentDictionary<PageName, Func<Page>>
            {
                [PageName.WelcomePage] = CreateWelcomePage,
                [PageName.MasterPage] = CreateMasterPage,
                [PageName.ClientPage] = CreateClientPage,

                [PageName.SessionHistoryPage] = CreateSessionHistoryPage,
                [PageName.SessionFromHistoryPage] = CreateSessionFromHistoryPage,
                [PageName.SessionsByClientPage] = CreateSessionsByClientPage,
                [PageName.AllSessionsPage] = CreateAllSessionsPage,

                [PageName.NewSessionPage] = CreateNewSessionPage,
                [PageName.CreateInviteLinkPage] = CreateInviteLinkPage,
                [PageName.ViewDecksPage] = CreateViewDecksPage,
                [PageName.ChooseDeckPage] = CreateChooseDeckPage,
                [PageName.SetSessionDurationPage] = CreateSetSessionDurationPage,
                [PageName.SetCardsAmountToShowPage] = CreateSetCardsAmountToShowPage,

                [PageName.AwaitClientToChooseCardsPage] = CreateAwaitClientToChooseCardsPage,
                [PageName.AwaitMasterToShowcardPage] = CreateAwaitMasterToShowcardPage,
                [PageName.SelectCardsToShowPage] = CreateSelectCardsToShowPage,

                [PageName.ClientHasJoinedSessionPage] = CreateClientHasJoinedSessionPage,
                [PageName.SendCardToShowPage] = CreateSendCardToShowPage,
                [PageName.FinalCardShownPage] = CreateFinalCardShownPage,

                [PageName.SessionIsFinishedForMasterPage] = CreateSessionIsFinishedForMasterPage,
                [PageName.SessionIsFinishedForClientPage] = CreateSessionIsFinishedForClientPage,

                [PageName.ShowCardForClientPage] = CreateShowCardForClientPage,
                [PageName.ShowCardForMasterPage] = CreateShowCardForMasterPage,
                [PageName.SessionWillStartSoonPage] = CreateSessionWillStartSoonPage,
                
                [PageName.DecksViewPage] = CreateDecksViewPage,
                [PageName.AddDeckPage] = CreateAddDeckPage,
                [PageName.GetNewDeckCardsPage] = CreateGetNewDeckCardsPage,
                [PageName.ShowAllCardsFromDeckPage] = CreateShowAllCardsFromDeckPage,
                [PageName.ShowCardFromDeckPage] = CreateShowCardFromDeckPage,
                [PageName.ExportHistoryPage] = CreateExportHistoryPage,

                [PageName.AboutPage] = CreateAboutPage,
                [PageName.SendMessagePage] = CreateSendMessagePage,
                [PageName.MessageSentPage] = CreateMessageSentPage,
                [PageName.SendCodePage] = CreateSendCodePage

            };
        }

        public PageName GetPreviousPageName(PageName pageName)
        {
            switch (pageName)
            {
                case PageName.MasterPage:
                    return PageName.WelcomePage;

                case PageName.NewSessionPage:
                    return PageName.MasterPage;

                case PageName.ClientPage:
                    return PageName.WelcomePage;

                case PageName.SessionFromHistoryPage:
                    return PageName.SessionHistoryPage;

                case PageName.SessionIsFinishedForClientPage:
                    return PageName.ClientPage;

                case PageName.SessionIsFinishedForMasterPage:
                    return PageName.MasterPage;

                case PageName.AboutPage:
                    return PageName.ClientPage;

                default: return PageName.WelcomePage;
            }
        }

        public async Task DeletePage(ITelegramBotClient client, long chatId, int messageId)
        {
            await client.DeleteMessageAsync(chatId, messageId);
        }

        public Page CreatePage(PageName pageName)
        {
            return _createPageDictionary[pageName].Invoke();
        }

        private Page CreateWelcomePage()
        {
            var page = new TextPageBuilder()
                .SetText(_messagesManager.GetPageText(PageName.WelcomePage))
                .SetHasPreviousPage(false)
                .SetButtons(Constants.WelcomeButtons)
                .Build();
            return page;
        }

        private Page CreateMasterPage()
        {
            var page = new TextPageBuilder()
                .SetText(_messagesManager.GetPageText(PageName.MasterPage))
                .SetHasPreviousPage (true)
                .SetPreviousPage(PageName.WelcomePage)
                .SetButtons (Constants.MasterButtons)
                .Build();
            return page;
        }

        private Page CreateClientPage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage(true)
                .SetPreviousPage(PageName.WelcomePage)
                .SetButtons(Constants.ClientPageButtons)
                .SetText(_messagesManager.GetPageText(PageName.ClientPage))
                .Build();
            return page;
        }

        private Page CreateNewSessionPage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage(true)
                .SetPreviousPage(PageName.MasterPage)
                .SetButtons(Constants.CreateSessionButtons)
                .SetText(_messagesManager.GetPageText(PageName.NewSessionPage))
                .Build();
            return page;
        }

        private Page CreateInviteLinkPage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage(true)
                .SetPreviousPage(PageName.MasterPage)
                .SetButtons(Constants.CreateSessionButtons)
                .Build();
            return page;
        }

        private Page CreateSessionHistoryPage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage(true)
                .SetPreviousPage (PageName.MasterPage)
                .SetButtons(Constants.SessionHistoryButtons)
                .SetText(_messagesManager.GetPageText(PageName.SessionHistoryPage))
                .Build();
            return page;
        }

        private Page CreateExportHistoryPage()
        {
            var page = new FilePageBuilder()
                .SetHasPreviousPage(true)
                .SetPreviousPage(PageName.MasterPage)
                .SetButtons(Constants.SessionHistoryButtons)
                .SetText(_messagesManager.GetPageText(PageName.ExportHistoryPage))
                .Build();
            return page;
        }

        private Page CreateSessionsByClientPage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage(true)
                .SetPreviousPage(PageName.SessionHistoryPage)
                .SetText(_messagesManager.GetPageText(PageName.SessionsByClientPage))
                .Build();
            return page;
        }

        private Page CreateAllSessionsPage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage(true)
                .SetPreviousPage(PageName.AllSessionsPage)
                .SetText(_messagesManager.GetPageText(PageName.SessionHistoryPage))
                .Build();
            return page;
        }

        private Page CreateSessionFromHistoryPage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage(true)
                .SetPreviousPage(PageName.SessionHistoryPage)
                .SetButtons(Constants.SessionFromHistoryPageButtons)
                .Build ();
            return page;
        }

        private Page CreateViewDecksPage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage(true)
                .SetPreviousPage(PageName.MasterPage)
                .SetText(_messagesManager.GetPageText(PageName.ViewDecksPage))
                .Build();
            return page;
        }

        private Page CreateChooseDeckPage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage (true)
                .SetText(_messagesManager.GetPageText (PageName.ChooseDeckPage))
                .SetPreviousPage (PageName.NewSessionPage)
                .Build();
            return page;
        }

        private Page CreateSetSessionDurationPage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage (true)
                .SetText(_messagesManager.GetPageText(PageName.SetSessionDurationPage))
                .SetPreviousPage(PageName.NewSessionPage)
                .SetButtons (Constants.SetSessionDurationPageButons)
                .Build();
            return page;
        }

        private Page CreateSetCardsAmountToShowPage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage(true)
                .SetPreviousPage(PageName.NewSessionPage)
                .SetText(_messagesManager.GetPageText(PageName.SetCardsAmountToShowPage))
                .SetButtons(Constants.SetCardsAmountToShowPageButtons)
                .Build();
            return page;
        }

        private Page CreateClientHasJoinedSessionPage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage(false)
                .SetButtons(Constants.ClientHasJoinedSessionPageButtons)
                .SetText(_messagesManager.GetPageText(PageName.ClientHasJoinedSessionPage))
                .Build();
            return page;
        }

        private Page CreateAwaitClientToChooseCardsPage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage(true)
                .SetPreviousPage(PageName.MasterPage)
                .SetButtons(Constants.AwaitClientToChooseCardsPageButtons)
                .SetText(_messagesManager.GetPageText(PageName.AwaitClientToChooseCardsPage))
                .Build();
            return page;
        }

        private Page CreateSelectCardsToShowPage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage(false)
                .SetText(_messagesManager.GetPageText(PageName.SelectCardsToShowPage))
                .Build();
            return page;
        }

        private Page CreateSessionWillStartSoonPage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage(false)
                .SetText(_messagesManager.GetPageText(PageName.SessionWillStartSoonPage))
                .Build();
            return page;
        }

        private Page CreateSendCardToShowPage() 
        {
            var page = new TextPageBuilder ()
                .SetHasPreviousPage(false)
                .SetText(_messagesManager.GetPageText(PageName.SendCardToShowPage))
                .SetButtons(Constants.SendCardToShowPageButtons)
                .Build();
            return page;        
        }

        private Page CreateFinalCardShownPage()
        {
            var page = new ImagePageBuilder ()
                .SetHasPreviousPage(false)
                .SetButtons(Constants.FinalCardShownPageButtons)
                .SetText(_messagesManager.GetPageText(PageName.FinalCardShownPage))
                .Build ();
            return page;
        }

        private Page CreateSessionIsFinishedForMasterPage()
        {
            var page = new TextPageBuilder ()
                .SetHasPreviousPage(false)
                .SetText(_messagesManager.GetPageText(PageName.SessionIsFinishedForMasterPage))
                .SetButtons(Constants.SessionIsFinishedForMasterPageButtons)
                .Build();
            return page;
        }

        private Page CreateShowCardForClientPage()
        {
            var page = new ImagePageBuilder ()
                .SetHasPreviousPage (false)
                .Build ();
            return page;
        }

        private Page CreateShowCardForMasterPage()
        {
            var page = new ImagePageBuilder ()
                .SetHasPreviousPage(false)
                .SetButtons(Constants.ShowCardForMasterPageButtons)
                .Build ();
            return page;
        }

        private Page CreateSessionIsFinishedForClientPage()
        {
            var page = new TextPageBuilder ()
                .SetHasPreviousPage (false)
                .SetText(_messagesManager.GetPageText(PageName.SessionIsFinishedForClientPage))
                .SetButtons(Constants .SessionIsFinishedForClientPageButtons)
                .Build ();
            return page;
        }

        private Page CreateAwaitMasterToShowcardPage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage(false)
                .SetText(_messagesManager.GetPageText(PageName.AwaitMasterToShowcardPage))
                .Build();
            return page;
        }

        private Page CreateGetNewDeckCardsPage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage(false)
                .SetText(_messagesManager.GetPageText(PageName.GetNewDeckCardsPage))
                .Build();
            return page;
        }

        private Page CreateAddDeckPage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage(true)
                .SetPreviousPage(PageName.DecksViewPage)
                .SetText(_messagesManager.GetPageText(PageName.AddDeckPage))
                .SetButtons(Constants.AddDeckPageButtons)
                .Build();
            return page;
        }

        private Page CreateDecksViewPage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage(true)
                .SetPreviousPage(PageName.MasterPage)
                .SetText(_messagesManager.GetPageText(PageName.DecksViewPage))
                .SetButtons(Constants.DecksViewPageButtons)
                .Build();
            return page;
        }

        private Page CreateShowAllCardsFromDeckPage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage(true)
                .SetPreviousPage(PageName.DecksViewPage)
                .SetText(_messagesManager.GetPageText(PageName.ShowAllCardsFromDeckPage))
                .Build();
            return page;
        }

        private Page CreateShowCardFromDeckPage()
        {
            var page = new ImagePageBuilder()
                .SetHasPreviousPage(true)
                .SetPreviousPage(PageName.ShowAllCardsFromDeckPage)
                .SetButtons(Constants.ShowCardFromDeckPageButtons)
                .Build();
            return page;
        }

        private Page CreateAboutPage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage(true)
                .SetPreviousPage(PageName.ClientPage)
                .SetButtons(Constants.AboutPageButtons)
                .SetText(_messagesManager.GetPageText(PageName.AboutPage))
                .Build();
            return page;
        }

        private Page CreateSendMessagePage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage(true)
                .SetPreviousPage(PageName.ClientPage)
                .SetButtons(Constants.SendMessagePageButtons)
                .SetText(_messagesManager.GetPageText(PageName.SendMessagePage))
                .Build();
            return page;
        }

        private Page CreateMessageSentPage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage(true)
                .SetPreviousPage(PageName.WelcomePage)
                .SetButtons(Constants.MessageSentPageButtons)
                .SetText(_messagesManager.GetPageText(PageName.MessageSentPage))
                .Build();
            return page;
        }

        private Page CreateSendCodePage()
        {
            var page = new TextPageBuilder()
                .SetHasPreviousPage(true)
                .SetPreviousPage(PageName.ClientPage)
                .SetButtons(Constants.SendCodePageButtons)
                .SetText(_messagesManager.GetPageText(PageName.SendCodePage))
                .Build();
            return page;
        }
    }
}
