namespace MacBot.ConsoleApp.Exceptions
{
    public class InvalidUpdateTypeException(string message) : ArgumentException(message)
    {
    }
}
