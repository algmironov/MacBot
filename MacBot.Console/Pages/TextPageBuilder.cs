namespace MacBot.ConsoleApp.Pages
{
    public class TextPageBuilder : PageBuilder
    {
        protected override Page CreatePage()
        {
            return new TextPage();
        }
    }
}
