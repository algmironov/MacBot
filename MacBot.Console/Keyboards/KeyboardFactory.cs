using MacBot.ConsoleApp.Models;
using MacBot.ConsoleApp.Repository;

using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MacBot.ConsoleApp.Keyboards
{
    public class KeyboardFactory : IKeyboardFactory
    {
        private const int MaxSingleColumnButtons = 5;
        private const int LongKeyboardColumns = 2;
        private const int MaxColumnsInRaw = 8;
        private const int MaxButtonsCount = 89;

        public IReplyMarkup CreateMarkupKeyboard(List<string> buttons)
        {
            if (buttons == null || buttons.Count == 0)
            {
                return new ReplyKeyboardRemove();
            }

            return buttons.Count <= MaxSingleColumnButtons
                ? CreateSingleColumnKeyboard(buttons)
                : CreateMultiColumnKeyboard(buttons);
        }

        private ReplyKeyboardMarkup CreateSingleColumnKeyboard(List<string> buttons)
        {
            var keyboardButtons = buttons.Select(button => new List<KeyboardButton> { new KeyboardButton(button) }).ToList();
            return new ReplyKeyboardMarkup(keyboardButtons) { ResizeKeyboard = true };
        }

        private ReplyKeyboardMarkup CreateMultiColumnKeyboard(List<string> buttons)
        {
            var keyboardButtons = new List<List<KeyboardButton>>();
            for (int i = 0; i < buttons.Count; i += LongKeyboardColumns)
            {
                var row = buttons.Skip(i).Take(LongKeyboardColumns).Select(b => new KeyboardButton(b)).ToList();
                keyboardButtons.Add(row);
            }

            if (buttons.Count % 2 != 0)
            {
                keyboardButtons.Add([new KeyboardButton(buttons.Last())]);
            }

            return new ReplyKeyboardMarkup(keyboardButtons) { ResizeKeyboard = true };
        }

        public IReplyMarkup CreateInlineKeyboard(List<string> buttons)
        {
            if (buttons == null || buttons.Count == 0)
            {
                throw new ArgumentException("Buttons list cannot be null or empty");
            }

            var keyboardButtons = buttons.Count <= MaxSingleColumnButtons
                ? CreateSingleColumnInlineKeyboard(buttons)
                : CreateMultiColumnInlineKeyboard(buttons);

            return new InlineKeyboardMarkup(keyboardButtons);
        }

        private List<List<InlineKeyboardButton>> CreateSingleColumnInlineKeyboard(List<string> buttons)
        {
            return buttons.Select(button => new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData(button)
                }).ToList();
        }

        private List<List<InlineKeyboardButton>> CreateMultiColumnInlineKeyboard(List<string> buttons)
        {
            var keyboardButtons = new List<List<InlineKeyboardButton>>();
            for (int i = 0; i < buttons.Count; i += LongKeyboardColumns)
            {
                var row = buttons.Skip(i).Take(LongKeyboardColumns)
                    .Select(InlineKeyboardButton.WithCallbackData)
                    .ToList();
                keyboardButtons.Add(row);
            }

            if (buttons.Count % 2 != 0)
            {
                keyboardButtons.Add(
                [
                    InlineKeyboardButton.WithCallbackData(buttons.Last())
                ]);
            }
                return keyboardButtons;
            }

        public IReplyMarkup CreateInlineKeyboard(List<string> buttons, PageName? previousPageName)
        {
            var keyboardButtons = buttons.Select(button =>
            {
                var callbackData = button == "Назад"
                    ? $"/pageName {previousPageName}"
                    : button;
                return new List<InlineKeyboardButton>
                    {
                        InlineKeyboardButton.WithCallbackData(button, callbackData)
                    };
            }).ToList();

            return new InlineKeyboardMarkup(keyboardButtons);
        }

        public IReplyMarkup ChooseCardsKeyboard(List<string> cards, long chatId)
        {
            if (cards == null || cards.Count == 0)
            {
                throw new ArgumentException("Cards list cannot be null or empty");
            }

            var buttons = cards.Select((card, index) => new { Index = index + 1, Card = card })
                               .ToDictionary(x => x.Index, x => x.Card);

            return CreateChooseCardsKeyboard(buttons, chatId);
        }

        private IReplyMarkup CreateChooseCardsKeyboard(Dictionary<int, string> buttons, long chatId)
        {
            // Ограничиваем количество кнопок до 99 (98 карт + 1 кнопка "Завершить выбор")
            const int MaxButtons = 98;
            if (buttons.Count > MaxButtons)
            {
                buttons = buttons.OrderBy(kvp => kvp.Key)
                                 .Take(MaxButtons)
                                 .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            var inlineKeyboardButtons = buttons.Select(kvp =>
                InlineKeyboardButton.WithCallbackData(
                    kvp.Key.ToString(),
                    $"/chooseCard_{kvp.Key}_{kvp.Value}"[..Math.Min(64, $"/chooseCard_{kvp.Key}_{kvp.Value}".Length)]
                )
            ).ToList();

            var inlineKeyboard = new List<List<InlineKeyboardButton>>();

            // Разбиваем на ряды по 8 кнопок (максимум для Telegram)
            for (int i = 0; i < inlineKeyboardButtons.Count; i += MaxColumnsInRaw)
            {
                inlineKeyboard.Add(inlineKeyboardButtons.Skip(i).Take(MaxColumnsInRaw).ToList());
            }

            // Добавляем кнопку "Завершить выбор" в отдельный ряд
            inlineKeyboard.Add(
                [
                    InlineKeyboardButton.WithCallbackData("Завершить выбор", "/finishedCardSelection")
                ]);

            KeyboardsStorage.SaveKeyboard(inlineKeyboard, chatId);
            return new InlineKeyboardMarkup(inlineKeyboard);
        }

        public async Task<IReplyMarkup> UpdateInlineKeyboard(int button, long chatId)
        {
            var buttons = await KeyboardsStorage.GetButtons(chatId);
            var updatedButtons = buttons.Select(row => row.Select(keyboardButton =>
            {
                if (keyboardButton.Text == button.ToString())
                {
                    return UpdateButton(keyboardButton, $"{button} ✅", SetRemoveCardCallback);
                }
                if (keyboardButton.CallbackData.StartsWith("/removeCard") &&
                    int.Parse(keyboardButton.CallbackData.Split('_')[1].Split(' ')[0]) == button)
                {
                    return UpdateButton(keyboardButton, $"{button}", SetChooseCardCallback);
                }
                return keyboardButton;
            }).ToList()).ToList();

            KeyboardsStorage.SaveKeyboard(updatedButtons, chatId);
            return new InlineKeyboardMarkup(updatedButtons);
        }

        private InlineKeyboardButton UpdateButton(InlineKeyboardButton button, string newText, Func<string, string> callbackUpdater)
        {
            return InlineKeyboardButton.WithCallbackData(
                newText,
                callbackUpdater(button.CallbackData)
            );
        }

        private string SetRemoveCardCallback(string oldCallback) => oldCallback.Replace("/chooseCard", "/removeCard");
        private string SetChooseCardCallback(string oldCallback) => oldCallback.Replace("/removeCard", "/chooseCard");

        public IReplyMarkup CreateCloudDecksKeyboard(List<string> decks, string backButtonPrefix)
             => CreateGenericCloudKeyboard(decks, "cloudDeck", backButtonPrefix);

        public IReplyMarkup CreateCloudCardsKeyboard(List<string> cards, string backButtonPrefix)
        {
            var buttons = new List<List<InlineKeyboardButton>>();
            cards.RemoveAt(0);
            // Если карт больше 89, выбираем случайные 89
            if (cards.Count > MaxButtonsCount)
            {
                var random = new Random();
                cards = cards.OrderBy(x => random.Next()).Take(MaxButtonsCount).ToList();
            }

            // Сортируем карты по номеру в имени файла
            cards = [.. cards.OrderBy(c =>
            {
                var parts = Path.GetFileNameWithoutExtension(c).Split('_');
                return int.Parse(parts[parts.Length - 1]);
            })];

            // Разбиваем на ряды по 8 кнопок (максимум для Telegram)
            for (int i = 0; i < cards.Count; i += MaxColumnsInRaw)
            {
                var row = cards
                            .Skip(i)
                            .Take(MaxColumnsInRaw)
                            .Select(item =>
                            {
                                var fileName = Path.GetFileNameWithoutExtension(item);
                                var number = fileName.Split('_')[1];
                                return InlineKeyboardButton.WithCallbackData(
                                    text: number,
                                    callbackData: $"/cloudCard {item}"
                                );
                            })
                            .ToList();
                buttons.Add(row);
            }

            buttons.Add([CreateBackButton(backButtonPrefix)]);

            return new InlineKeyboardMarkup(buttons);
        }

        public IReplyMarkup CreateCloudCardsKeyboard(List<Card> cards, string backButtonPrefix)
        {
            var deckId = cards.First().DeckId;
            var cardsNums = cards.Select(x => Path.GetFileNameWithoutExtension (x.Link.Split("_").Last())).ToList();

            var buttons = new List<List<InlineKeyboardButton>>();

            // Если карт больше 89, выбираем случайные 89
            if (cardsNums.Count > MaxButtonsCount)
            {
                var random = new Random();
                cardsNums = cardsNums.OrderBy(x => random.Next()).Take(MaxButtonsCount).ToList();
            }

            // Сортируем карты по номеру в имени файла
            cardsNums = [.. cardsNums.OrderBy(int.Parse)];

            // Разбиваем на ряды по 8 кнопок (максимум для Telegram)
            for (int i = 0; i < cardsNums.Count; i += MaxColumnsInRaw)
            {
                var row = cardsNums.Skip(i).Take(MaxColumnsInRaw)
                               .Select(item =>
                               {
                                   return InlineKeyboardButton.WithCallbackData(
                                       text: item,
                                       callbackData: $"/cloudCard {deckId} {item}"
                                   );
                               })
                               .ToList();
                buttons.Add(row);
            }

            buttons.Add([CreateBackButton(backButtonPrefix)]);

            return new InlineKeyboardMarkup(buttons);
        }

        private IReplyMarkup CreateGenericCloudKeyboard(List<string> items, string callbackPrefix, string backButtonPrefix)
        {
            var buttons = items.Select(item => new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(item, $"/{callbackPrefix} {item}")
            }).ToList();

            buttons.Add([CreateBackButton(backButtonPrefix)]);

            return new InlineKeyboardMarkup(buttons);
        }

        public IReplyMarkup CreateChooseCardsAmountKeyboard(List<string> buttons)
        {
            var keyboard = buttons.Select(button =>
                InlineKeyboardButton.WithCallbackData(button, $"/numberOfCards {button}")
            ).ToList();
            
            return new InlineKeyboardMarkup(new[] { keyboard });
        }

        public IReplyMarkup CreateAllDecksKeyboard(Dictionary<string, Guid> decks)
        {
            var buttons = decks.Select(deck =>
                InlineKeyboardButton.WithCallbackData(deck.Key, $"/deckId {deck.Value}")
            ).ToList();

            return new InlineKeyboardMarkup(CreateMultiColumnInlineKeyboard(buttons, LongKeyboardColumns));
        }

        private List<List<InlineKeyboardButton>> CreateMultiColumnInlineKeyboard(List<InlineKeyboardButton> buttons, int columnsCount)
        {
            var rows = (buttons.Count + columnsCount - 1) / columnsCount;
            var keyboard = new List<List<InlineKeyboardButton>>();

            for (int i = 0; i < rows; i++)
            {
                keyboard.Add(buttons.Skip(i * columnsCount).Take(columnsCount).ToList());
            }

            return keyboard;
        }

        public IReplyMarkup CreateKeyboardWithCallbacks(Dictionary<string, string> buttons)
        {
            var inlineKeyboardButtons = buttons.Select(button =>
                InlineKeyboardButton.WithCallbackData(button.Key, button.Value)
            ).ToList();

            return CreateInlineKeyboard(inlineKeyboardButtons);
        }

        private IReplyMarkup CreateInlineKeyboard(List<InlineKeyboardButton> buttons)
        {
            var keyboard = buttons.Count <= MaxSingleColumnButtons
                ? buttons.Select(button => new List<InlineKeyboardButton> { button }).ToList()
                : CreateMultiColumnInlineKeyboard(buttons, LongKeyboardColumns);

            return new InlineKeyboardMarkup(keyboard);
        }

        public IReplyMarkup CreateClientsFromHistoryKeyboard(Dictionary<Guid, string> clients)
        {
            return CreateHistoryKeyboard(clients, "historyOfClient", PageName.SessionHistoryPage);
        }

        public IReplyMarkup CreateAllSessionsKeyboard(Dictionary<Guid, string> sessions)
        {
            return CreateHistoryKeyboard(sessions, "showHistory", PageName.SessionHistoryPage);
        }

        private IReplyMarkup CreateHistoryKeyboard(Dictionary<Guid, string> items, string callbackPrefix, PageName previousPage)
        {
            var keyboard = items.Select(item => new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(item.Value, $"/{callbackPrefix} {item.Key}")
            }).ToList();

            keyboard.Add(new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData("Назад", $"/previousPage {previousPage}")
            });

            return new InlineKeyboardMarkup(keyboard);
        }

        public IReplyMarkup CreateBackButtonKeyboard(List<string> button, PageName previousPage)
        {
            var keyboard = new List<List<InlineKeyboardButton>>
            {
                new() {
                    InlineKeyboardButton.WithCallbackData(button[0], $"/previousPage {previousPage}")
                }
            };

            return new InlineKeyboardMarkup(keyboard);
        }

        public IReplyMarkup CreateBackButtonKeyboard(List<string> button, string prefix)
        {
            var keyboard = new List<List<InlineKeyboardButton>>
            {
                new() {
                    InlineKeyboardButton.WithCallbackData(button[0], $"/{prefix}")
                }
            };
            return new InlineKeyboardMarkup(keyboard);
        }

        public IReplyMarkup CreateBackButtonKeyboard(List<string> button, string prefix, string data)
        {
            var keyboard = new List<List<InlineKeyboardButton>>
            {
                new() {
                    InlineKeyboardButton.WithCallbackData(button[0], $"/{prefix} {data}")
                }
            };
            return new InlineKeyboardMarkup(keyboard);
        }

        private InlineKeyboardButton CreateBackButton(string backButtonPrefix)
        {
            return InlineKeyboardButton.WithCallbackData("Назад", $"/{backButtonPrefix}");
        }

        public IReplyMarkup CreateForceReplyKeyboard(string placeholder)
        {
            var keyboard = new ForceReplyMarkup
            {
                InputFieldPlaceholder = placeholder
            };
            return keyboard;
        }

        public IReplyMarkup CreateUnreadFeedbacksKeyboard(List<Feedback> feedbacks)
        {
            var prefix = "/feedback";
            List<List<InlineKeyboardButton>> keyboard = [];

            foreach (var feedback in feedbacks) 
            {
                List<InlineKeyboardButton> button = [];
                string text = $"{feedback.Date} от: {feedback.Name}";
                button.Add(InlineKeyboardButton.WithCallbackData(text, $"{prefix} {feedback.Id}"));
                keyboard.Add(button);
            }
            return new InlineKeyboardMarkup(keyboard);
        }

        public IReplyMarkup CreateFeedbackWebAppKeyboard()
        {
            List<List<InlineKeyboardButton>> keyboard = [];

            var webAppInfo = new WebAppInfo
            {
                Url = "https://algmironov.github.io/MacBotFeedbackForm/"
            };

            var webAppButton = InlineKeyboardButton.WithWebApp("Открыть форму обратной связи", webAppInfo);
            var backButton = InlineKeyboardButton.WithCallbackData("Назад", $"/pageName ClientPage");

            keyboard.Add([webAppButton]);
            keyboard.Add([backButton]);

            return new InlineKeyboardMarkup(keyboard);
        }
    }
}
