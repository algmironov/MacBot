namespace MacBot.ConsoleApp.Repository
{
    public enum PageName
    {
        WelcomePage,
        MasterPage,
        ClientPage,
        
        SessionHistoryPage,
        SessionsByClientPage,
        AllSessionsPage,
        SessionFromHistoryPage,

        NewSessionPage,
        CreateInviteLinkPage,
        ViewDecksPage,
        ChooseDeckPage,
        SetSessionDurationPage,
        SetCardsAmountToShowPage,

        ClientHasJoinedSessionPage,
        SessionWillStartSoonPage,
        AwaitClientToChooseCardsPage,
        AwaitMasterToShowcardPage,

        SelectCardsToShowPage,
        SendCardToShowPage,
        ShowCardForClientPage,
        ShowCardForMasterPage,
        FinalCardShownPage,

        SessionIsFinishedForMasterPage,
        SessionIsFinishedForClientPage,

        DecksViewPage,
        AddDeckPage,
        GetNewDeckCardsPage,
        ShowAllCardsFromDeckPage,
        ShowCardFromDeckPage,
        ExportHistoryPage,

        AboutPage,
        SendMessagePage,
        MessageSentPage,
        SendCodePage
    }
}
