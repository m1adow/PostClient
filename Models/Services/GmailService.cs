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

        public void SendMessage(MimeMessage message, Action<string> exceptionHandler)
        {
            SmtpClient client = new SmtpClient();

            try
            {
                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate(Account.Email, Account.Password);
                client.Send(message);
            }
            catch (Exception exception)
            {
                exceptionHandler(exception.Message);
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }

        public void DeleteMessage(MailMessage message, Action<string> exceptionHandler)
        {
            ImapClient client = new ImapClient();

            try
            {
                EstablishConnection(client, Account, "imap.gmail.com");
                DeleteSpecificMessage(client, message.Uid);
            }
            catch (Exception exception)
            {
                exceptionHandler(exception.Message);
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }

        public void FlagMessage(MailMessage message, Action<string> exceptionHandler)
        {
            ImapClient client = new ImapClient();

            try
            {
                EstablishConnection(client, Account, "imap.gmail.com");
                FlagSpecificMessage(client, message.Uid, message.IsFlagged);
            }
            catch (Exception exception)
            {
                exceptionHandler(exception.Message);
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }

        public Dictionary<UniqueId, MimeMessage> LoadMessages(SpecialFolder specialFolder, SearchQuery searchQuery, Action<string> exceptionHandler)
        {
            ImapClient client = new ImapClient();
            Dictionary<UniqueId, MimeMessage> messages = new Dictionary<UniqueId, MimeMessage>();

            try
            {
                EstablishConnection(client, Account, "imap.gmail.com");
                GetMessages(client, messages, specialFolder, searchQuery);
            }
            catch (Exception exception)
            {
                exceptionHandler(exception.Message);
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
