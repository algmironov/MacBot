using System.Text.Json;

using Telegram.Bot.Types.ReplyMarkups;

namespace MacBot.ConsoleApp.Repository
{
    public static class KeyboardsStorage
    {
        private static string _folder = AppDomain.CurrentDomain.BaseDirectory;

        public static void SaveKeyboard(List<List<InlineKeyboardButton>> buttons, long userId)
        {
            var filename = Path.Combine(_folder, $"{userId}.json");

            try
            {
                var jsonString = JsonSerializer.Serialize(buttons);

                File.WriteAllText(filename, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Возникла ошибка при сохранении кнопок: {ex}");
            }
        }

        public static void SaveKeyboard(IReplyMarkup keyboard, long userId)
        {
            var filename = Path.Combine(_folder, $"{userId}.json");

            try
            {
                var jsonString = JsonSerializer.Serialize(keyboard);

                File.WriteAllText(filename, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Возникла ошибка при сохранении кнопок: {ex}");
            }
        }

        public async static Task<IReplyMarkup> GetKeyboard(long userId)
        {
            var filename = Path.Combine(_folder, $"{userId}.json");

            try
            {
                if (File.Exists(filename))
                {
                    var rawText = await File.ReadAllTextAsync(filename);
                    var keyboard = JsonSerializer.Deserialize<IReplyMarkup>(rawText);
                    return keyboard;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Возникла ошибка при получении кнопок: {ex}");
            }
            return null;
        }

        public async static Task<List<List<InlineKeyboardButton>>> GetButtons(long userId)
        {
            var filename = Path.Combine(_folder, $"{userId}.json");

            try
            {
                if (File.Exists(filename))
                {
                    var rawText = await File.ReadAllTextAsync(filename);
                    var buttons = JsonSerializer.Deserialize<List<List<InlineKeyboardButton>>>(rawText);
                    return buttons;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Возникла ошибка при получении кнопок: {ex}");
            }
            return new List<List<InlineKeyboardButton>> { };
        }

        public static void DeleteKeyboard(long userId)
        {
            var filename = Path.Combine(_folder, $"{userId}.json");


            try
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Возникла ошибка при удалении кнопок: {ex}");
            }
        }
    }
}
