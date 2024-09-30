using System.Collections.Generic;
using System.Threading.Channels;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DisclaimerBot
{
    internal class Program
    {
        private static ITelegramBotClient _botClient;

        private static ReceiverOptions _receiverOptions;
        private static string _disclaimer = "***Не флудить!***";
        static async Task Main(string[] args)
        {

            _botClient = new TelegramBotClient("---");

            _receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] 
                {
                    UpdateType.Message
                },
                
                ThrowPendingUpdates = true,
            };
            await _botClient.SetMyCommandsAsync(new[]
            {
                new BotCommand { Command = "start", Description = "Запустить бота" },
                new BotCommand { Command = "help", Description = "Получить справку" },
                new BotCommand { Command = "chats", Description = "Получаем информацию о всех моих каналах" },
                new BotCommand { Command = "settings", Description = "Настройки" }
            });
            using var cts = new CancellationTokenSource();

            _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);

            var me = await _botClient.GetMeAsync();
            Console.WriteLine($"{me.FirstName} запущен!");

            await Task.Delay(-1);
        }
        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {

                switch (update.Type)
                {
                    case UpdateType.Message:
                        {

                            await AddNewChannel(botClient, update.Message);
                            await SendMessage(botClient, update.Message);
                            return;
                        }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        
        private static async Task SendMessage(ITelegramBotClient botClient, Message message, bool isShowLog = true)
        {
            if (message.SenderChat != null && message.SenderChat.Type == ChatType.Channel)
            {
                if (isShowLog)
                {
                    string log = $"Канал: {message.SenderChat.Title} \nОтправитель: {message.ForwardSignature} \nВремя публикации: {message.Date.TimeOfDay} \nДата публикации: {message.Date.ToShortDateString()}\n";
                    switch (message.Type)
                    {
                        case MessageType.Text:
                            {
                                log += $"Текст публикации {message.Text} \n";
                                break;
                            }
                        default:
                            {
                                string text = message.Caption == null ? "Текст публикации отцуцтвует" : message.Caption;
                                log += $"Текст публикации {text} \n";
                                break;
                            }
                    }
                    await Console.Out.WriteLineAsync(log);
                }

                await SendDisclaimer(botClient, message);

            }
            if (message.Chat != null && message.Chat.Type == ChatType.Private)
            {
                await SendPrivateResponse(botClient, message);
            }
        }
        private static async Task SendDisclaimer(ITelegramBotClient botClient, Message message)
        {
            var chat = message.Chat;
            СhannelsTG XMLData = XMLHandler.ReadXML();

            СhannelTG ch = XMLData.Channels.Where(c => c.ChatID == chat.Id).First();

            if(ch != null && ch.ChatDisclaimerState == true)
            {
                await botClient.SendTextMessageAsync(
                                chat.Id,
                                ch.ChatDisclaimer,
                                replyToMessageId: message.MessageId,
                                parseMode: ParseMode.Markdown
                                );
            }
            
        }

        private static async Task SendPrivateResponse(ITelegramBotClient botClient, Message message)
        {
            if(message.Text != null)
            {
                string commqand = message.Text.Split(' ')[0].ToLower();
                switch (commqand)
                {
                    case "/start":
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Привет я Дисклеймер бот, давай начем совместную работу!");
                            break;
                        }
                    case "/chats":
                        {
                            СhannelsTG Data = XMLHandler.ReadXML();

                            List<СhannelTG> Сhannels = Data.Channels.Where(c => c.ChatAdmins.Count(a => a.UserId == message.From.Id) > 0).ToList();
                            string chats = string.Join(' ', Сhannels.Select(c => "\n"+"***"+c.ChatName + "*** `" + c.ChatID + "`"));
                            await botClient.SendTextMessageAsync(message.Chat.Id,
                                $"Вот все доступные чаты для модерации=> : {chats}",
                                parseMode: ParseMode.Markdown
                                );
                           
                            
                            break;
                        }
                    default:
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Команда отсутствует(((", replyMarkup: new ReplyKeyboardRemove());
                            break;
                        }
                }
                
            }
        }

       

        private static async Task AddNewChannel(ITelegramBotClient botClient, Message message)
        {
            if (message.NewChatMembers != null)
            {
                foreach (var newUser in message.NewChatMembers)
                {
                    if (newUser.IsBot && message.Chat.Type == ChatType.Supergroup && newUser.Id == botClient.BotId)
                    {
                        Console.WriteLine($"Бот {newUser.Username} был добавлен в группу!");

                        ChatMember[] admins = await botClient.GetChatAdministratorsAsync(chatId: message.Chat.Id);

                        List<User> users = new List<User>();
                        foreach (var user in admins)
                            users.Add(new User(user.User.Id, user.User.FirstName));

                        СhannelTG сhannelTG = new СhannelTG(message.Chat.Title, message.Chat.Id, users, _disclaimer, false);
                        XMLHandler.WriteXML(сhannelTG);
                    }
                }
            }
        }
        
        private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
           
            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
