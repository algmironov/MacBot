using System.Resources;

namespace MacBot.ConsoleApp.Repository
{
    public class PageMessagesManager : IPageMessagesManager
    {
        private readonly ResourceManager _resources;

        public PageMessagesManager()
        {
            _resources = new ResourceManager("MacBot.ConsoleApp.Resources.PageMessages", typeof(Program).Assembly);

            var assembly = typeof(Program).Assembly;
            var resourceNames = assembly.GetManifestResourceNames();
            foreach (var resourceName in resourceNames)
            {
                Console.WriteLine(resourceName);
            }
        }

        public string GetPageText(PageName pageName)
        {
            return _resources.GetString(pageName.ToString());
        }

        public string GetPrefix(string prefix)
        {
            return _resources.GetString(prefix);
        }
    }
}
