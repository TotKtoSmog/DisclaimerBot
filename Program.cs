using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DisclaimerBot
{
    internal class Program
    {
        private static ITelegramBotClient? _botClient;

        private static ReceiverOptions? _receiverOptions;
        private static readonly string _disclaimer = "***Не флудить!***";
        static async Task Main()
        {

            _botClient = new TelegramBotClient("---");

            _receiverOptions = new ReceiverOptions
            {
                AllowedUpdates =
                [
                    UpdateType.Message
                ],
                
                ThrowPendingUpdates = true,
            };
            
            await _botClient.SetMyCommandsAsync(
            [
                new BotCommand { Command = "start", Description = "Запустить бота" },
                new BotCommand { Command = "help", Description = "Получить справку" },
                new BotCommand { Command = "chats", Description = "Получаем информацию о всех моих каналах" }
            ]);
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
                                string text = message.Caption ?? "Текст публикации отсутствует";
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
            ChannelsTG XMLData = XMLHandler.ReadXML();
            if(XMLData.Channels.Count == 0)
            {
                ChannelTG channel = await CreateNewChannelTG(botClient, message.Chat);
                XMLData.Channels.Add(channel);
                XMLHandler.WriteXML(XMLData);
            }

            ChannelTG ch = XMLData.Channels.Where(c => c.ChatID == chat.Id).First();

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
                string command = message.Text.Split(' ')[0].ToLower(); 
                switch (command)
                {
                    case "/start":
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Привет я Дисклеймер бот, давай начнем совместную работу!");
                            break;
                        }
                    case "/chats":
                        {
                            ChannelsTG Data = XMLHandler.ReadXML();

                            List<ChannelTG> Channels = Data.Channels.Where(c => c.ChatAdmins.Any(a => a.UserId == message?.From?.Id)).ToList();
                            string chats = string.Join(' ', Channels.Select(c => "\n"+"***"+c.ChatName + "*** `" + c.ChatID + "`"));
                            await botClient.SendTextMessageAsync(message.Chat.Id,
                                $"Вот все доступные чаты для модерации=> : {chats}",
                                parseMode: ParseMode.Markdown
                                );
                           
                            
                            break;
                        }
                    case "/get_disclaimer":
                        {
                            ChannelTG? channel = await GetChannelId(botClient, message, message.Text.Split(' ').Skip(1).ToList());
                            if (channel != null)
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id,
                                $"Для чата {channel.ChatID} присвоен дисклеймер\n{channel.ChatDisclaimer}\nего статут активности: {channel.ChatDisclaimerState}",
                                parseMode: ParseMode.Markdown
                                );
                            }
                            break;
                        }
                    case "/disclaimer_on":
                        {
                            ChannelTG? channel = await GetChannelId(botClient, message, message.Text.Split(' ').Skip(1).ToList());
                            if (channel != null)
                            {
                                channel.ChatDisclaimerState = true;
                                XMLHandler.WriteXML(channel);

                                await botClient.SendTextMessageAsync(message.Chat.Id,
                                $"Для чата {channel.ChatID} ({channel.ChatName}) был включен дисклеймер, теперь его увидят все!!!",
                                parseMode: ParseMode.Markdown
                                );
                            }
                            break;
                        }
                    case "/disclaimer_off":
                        {
                            ChannelTG? channel = await GetChannelId(botClient, message, message.Text.Split(' ').Skip(1).ToList());
                            if (channel != null)
                            {
                                channel.ChatDisclaimerState = false;
                                XMLHandler.WriteXML(channel);

                                await botClient.SendTextMessageAsync(message.Chat.Id,
                                $"Для чата {channel.ChatID} ({channel.ChatName}) дисклеймер был выключен",
                                parseMode: ParseMode.Markdown
                                );
                            }
                            break;
                        }
                    case "/set_new_disclaimer":
                        {
                            ChannelTG? channel = await GetChannelId(botClient, message, message.Text.Split(' ').Skip(1).ToList());
                            if(channel != null)
                            {
                                List<string> parts = message.Text.Split(' ').Skip(2).ToList();
                                parts[0] = parts[0].Replace('\n', ' ');
                                string disclaimer = String.Join(" ", parts);
                                channel.ChatDisclaimer = disclaimer;
                                XMLHandler.WriteXML(channel);

                                await botClient.SendTextMessageAsync(message.Chat.Id,
                                    $"Для чата {channel.ChatID} ({channel.ChatName}) был изменен дисклеймер!!",
                                    parseMode: ParseMode.Markdown
                                    );
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"{channel.ChatDisclaimer}", parseMode: ParseMode.Markdown);
                            }
                            break;
                        }
                    default:
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Команда {message.Text} отсутствует(((", parseMode: ParseMode.Markdown);
                            break;
                        }
                }
                
            }
        }
        private static async Task<ChannelTG?> GetChannelId(ITelegramBotClient botClient, Message message, List<string> paramsCommand)
        {
            if (paramsCommand.Count == 0)
            {

                await botClient.SendTextMessageAsync(message.Chat.Id,
                    $"Неправильное количество параметров, ознакомьтесь с написанием данной команды через /help"
                    );
                return null;
            }
            else
            {
                ChannelsTG Data = XMLHandler.ReadXML();
                bool isCorrect = Int64.TryParse(paramsCommand[0], out long id);
                if (!isCorrect)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id,
                        $"Ошибка в написании команды проверьте правильность написания команды в /help",
                        parseMode: ParseMode.Markdown
                        );
                    return null;
                }
                ChannelTG? Channel = Data.Channels.Where(c => c.ChatID == id).FirstOrDefault();
                if (Channel == null)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id,
                        $"Я не знаю чат с таким ID {paramsCommand[0]}, еще раз добавьте бота в чат канала как администратора",
                        parseMode: ParseMode.Markdown
                        );
                    return null;
                }
                ChatMember[] members;
                try
                {
                    members = await botClient.GetChatAdministratorsAsync(id);

                    List<User> users = [];
                    foreach (var user in members)
                        users.Add(new User(user.User.Id, user.User.FirstName));
                    
                    ChannelsTG channelsTG = ChannelsTG.ChannelsAdminsInfo(XMLHandler.ReadXML(), id, users);
                    XMLHandler.WriteXML(channelsTG);

                    ChannelTG? channelTG = ChannelsTG.GetChannel(channelsTG, id);



                    if (!users.Any(u => u.UserId == message?.From?.Id))
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id,
                        $"Вы не являетесь администратором данного чата(",
                        parseMode: ParseMode.Markdown
                        );
                        return null;
                    }
                    else
                        return channelTG;
                }
                catch
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id,
                        $"Я не нашел такой чат с id ({id}) или же я в него не был добавлен(",
                        parseMode: ParseMode.Markdown
                        );
                }
                return null;
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
                        ChannelTG channelTG = await CreateNewChannelTG(botClient, message.Chat);
                        XMLHandler.WriteXML(channelTG);
                    }
                }
            }
        }
        private static async Task<ChannelTG> CreateNewChannelTG(ITelegramBotClient botClient, Chat chat)
        {
            ChatMember[] admins = await botClient.GetChatAdministratorsAsync(chat.Id);
            List<User> users = [];
            foreach (var user in admins)
                users.Add(new User(user.User.Id, user.User.FirstName));
            return new ChannelTG(chat.Title, chat.Id, users, _disclaimer, false);
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
