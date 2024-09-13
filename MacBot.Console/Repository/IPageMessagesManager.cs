namespace MacBot.ConsoleApp.Repository
{
    public interface IPageMessagesManager
    {
        string GetPageText(PageName pageName);
        string GetPrefix(string prefix);
    }
}