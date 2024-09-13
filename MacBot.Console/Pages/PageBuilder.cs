using MacBot.ConsoleApp.Repository;

namespace MacBot.ConsoleApp.Pages
{
    public abstract class PageBuilder
    {
        protected Page page;

        public PageBuilder()
        {
            page = CreatePage();
        }

        protected abstract Page CreatePage();

        public PageBuilder SetHasPreviousPage(bool hasPreviousPage)
        {
            page.HasPreviousPage = hasPreviousPage;
            return this;
        }

        public PageBuilder SetPreviousPage(PageName? previousPage)
        {
            page.PreviousPage = previousPage;
            return this;
        }

        public PageBuilder SetText(string text)
        {
            page.Text = text;
            return this;
        }

        public PageBuilder SetButtons(List<string> buttons)
        {
            page.Buttons = buttons;
            return this;
        }

        public Page Build()
        {
            return page;
        }
    }
}
