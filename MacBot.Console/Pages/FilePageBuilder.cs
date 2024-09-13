namespace MacBot.ConsoleApp.Pages
{
    public class FilePageBuilder : PageBuilder
    {
        protected override Page CreatePage()
        {
            return new FilePage();
        }
    }
}
