using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MimeKit;
using PostClient.Models.Infrastructure;
using System;
using System.Collections.Generic;

namespace PostClient.Models.Services
{
    internal sealed class GmailService : PostService, IService
    {
        public Account Account { get; }

        public GmailService(Account account)
        {
            this.Account = account;
        }

        public void SendMessage(MimeMessage message)
        {
            SmtpClient client = new SmtpClient();

            try
            {
                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate(Account.Email, Account.Password);
                client.Send(message);
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }

        public void DeleteMessage(MailMessage message)
        {
            ImapClient client = new ImapClient();

            try
            {
                EstablishConnection(client, Account, "imap.gmail.com");
                DeleteSpecificMessage(client, message.Uid);
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }

        public void FlagMessage(MailMessage message)
        {
            ImapClient client = new ImapClient();

            try
            {
                EstablishConnection(client, Account, "imap.gmail.com");
                FlagSpecificMessage(client, message.Uid, message.IsFlagged);
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }

        public Dictionary<UniqueId, MimeMessage> LoadMessages(SpecialFolder specialFolder, SearchQuery searchQuery)
        {
            ImapClient client = new ImapClient();
            Dictionary<UniqueId, MimeMessage> messages = new Dictionary<UniqueId, MimeMessage>();

            try
            {
                EstablishConnection(client, Account, "imap.gmail.com");
                GetMessages(client, messages, specialFolder, searchQuery);
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }

            return messages;
        }
    }
}
