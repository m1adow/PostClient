using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MimeKit;
using PostClient.Models.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PostClient.Models.Services
{
    #nullable enable

    internal sealed class GmailService : PostService, IService
    {
        public Account Account { get; }

        public GmailService(Account account)
        {
            this.Account = account;
        }

        public async Task SendMessage(MimeMessage message)
        {
            SmtpClient client = new SmtpClient();

            try
            {
                await Task.Run(() =>
                {
                    client.ConnectAsync("smtp.gmail.com", 465, true);
                    client.AuthenticateAsync(Account.Email, Account.Password);
                    client.SendAsync(message);
                });
            }
            finally
            {
                await client.DisconnectAsync(true);
                client.Dispose();
            }
        }

        public async void DeleteMessage(MailMessage message)
        {
            ImapClient client = new ImapClient();

            try
            {
                await EstablishConnectionAsync(client, Account, "imap.gmail.com");
                await DeleteSpecificMessage(client, message.Uid);
            }
            finally
            {
                await client.DisconnectAsync(true);
                client.Dispose();
            }
        }

        public async void FlagMessage(MailMessage message)
        {
            ImapClient client = new ImapClient();

            try
            {
                await EstablishConnectionAsync(client, Account, "imap.gmail.com");
                await FlagSpecificMessage(client, message.Uid, message.IsFlagged);
            }
            finally
            {
                await client.DisconnectAsync(true);
                client.Dispose();
            }
        }

        public async Task<Dictionary<UniqueId, MimeMessage>> LoadMessages(SpecialFolder specialFolder, SearchQuery searchQuery)
        {
            ImapClient client = new ImapClient();
            Dictionary<UniqueId, MimeMessage> messages = new Dictionary<UniqueId, MimeMessage>();

            try
            {
                await EstablishConnectionAsync(client, Account, "imap.gmail.com");
                await GetMessagesAsync(client, messages, specialFolder, searchQuery);
            }
            finally
            {
                await client.DisconnectAsync(true);
                client.Dispose();
            }

            return messages;
        }
    }
}
