using System;
using System.Reflection.Metadata.Ecma335;
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
                    UpdateType.Message, 
                    UpdateType.ChannelPost
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
                            
                            var message = update.Message;

                            
                            var user = message.From;

                            Console.WriteLine($"{user.FirstName} ({user.Id}) написал сообщение: {message.Text}");

                            
                            var chat = message.Chat;
                            await botClient.SendTextMessageAsync(
                                chat.Id,
                                "Алоо",
                                replyToMessageId: message.MessageId 
                                );
                            await botClient.SendDiceAsync(chat.Id);
                            return;
                        }
                    case UpdateType.ChannelPost:
                        {

                            var ChannelPost = update.ChannelPost;

                            var ChannelID = ChannelPost.Chat.Id;
                            var MESSAGE_ID = ChannelPost.MessageId;

                            await botClient.SendTextMessageAsync(
                                ChannelID,
                                "Алоо", 
                                replyToMessageId: MESSAGE_ID 
                                );

                            return;

                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
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
