using System.Text;

namespace MacBot.ConsoleApp.Services
{
    public class InviteLinkGenerator
    {
        public static string GenerateInviteCode(int length) 
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            var random = new Random();

            var stringBuilder = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(chars[random.Next(chars.Length)]);
            }
            return stringBuilder.ToString();
        }

        public static string GenerateInviteUrl(int length)
        {
            var code = GenerateInviteCode(length);

            return @$"https://t.me/mac_cards_psy_bot?start={code}";
        }
    }
}
