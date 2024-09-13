using Microsoft.Extensions.DependencyInjection;

namespace MacBot.ConsoleApp.UpdateHandlers
{
    public interface ISpecificUpdateHandlerFactory
    {
        IUpdateHandler GetMasterActiveSessionUpdateHandler();
        IUpdateHandler GetMasterUpdateHandler();
        IUpdateHandler GetClientActiveSessionUpdateHandler();
        IUpdateHandler GetClientUpdateHandler();
    }
    public class SpecificUpdateHandlerFactory : ISpecificUpdateHandlerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public SpecificUpdateHandlerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IUpdateHandler GetClientUpdateHandler()
        {
            return _serviceProvider.GetRequiredService<IClientUpdateHandler>();
        }

        public IUpdateHandler GetClientActiveSessionUpdateHandler()
        {
            return _serviceProvider.GetRequiredService<IClientActiveSessionUpdateHandler>();
        }

        public IUpdateHandler GetMasterUpdateHandler()
        {
            return _serviceProvider.GetRequiredService<IMasterUpdateHandler>();
        }

        public IUpdateHandler GetMasterActiveSessionUpdateHandler()
        {
            return _serviceProvider.GetRequiredService<IMasterActiveSessionUpdateHandler>();
        }
    }
}
