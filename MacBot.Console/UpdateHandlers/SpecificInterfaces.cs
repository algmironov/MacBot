namespace MacBot.ConsoleApp.UpdateHandlers
{
    public interface IMasterUpdateHandler : IUpdateHandler { }
    public interface IMasterActiveSessionUpdateHandler : IUpdateHandler { }
    public interface IClientUpdateHandler : IUpdateHandler { }
    public interface IClientActiveSessionUpdateHandler : IUpdateHandler { }
}
