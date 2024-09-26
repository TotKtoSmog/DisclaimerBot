using System.Data;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DisclaimerBot
{
    internal class Program
    {
        private static ITelegramBotClient _botClient;

        private static ReceiverOptions _receiverOptions;
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
                            await SendDesclaimer(_botClient, update.Message);
                            return;
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        
        private static async Task SendDesclaimer(ITelegramBotClient botClient, Message message, bool isShowLog = true)
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

                var chat = message.Chat;
                await botClient.SendTextMessageAsync(
                    chat.Id,
                    "***Не флудить!***",
                    replyToMessageId: message.MessageId,
                    parseMode: ParseMode.Markdown
                    );
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
