using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using PostClient.Models.Infrastructure;
using System;
using System.Collections.Generic;

namespace PostClient.Models.Services
{
    internal sealed class OutlookService : PostService, IService
    {
        public Account Account { get; }

        public OutlookService(Account account)
        {
            this.Account = account;
        }

        public void SendMessage(MimeMessage message, Action<string> exceptionHandler)
        {
            SmtpClient client = new SmtpClient();

            try
            {
                client.Connect("smtp.outlook.com", 587, SecureSocketOptions.StartTls);
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
            throw new NotImplementedException();
        }

        public void FlagMessage(MailMessage message, Action<string> exceptionHandler)
        {
            throw new NotImplementedException();
        }

        public Dictionary<UniqueId, MimeMessage> LoadMessages(SpecialFolder specialFolder, SearchQuery searchQuery, Action<string> exceptionHandler)
        {
            ImapClient client = new ImapClient();
            Dictionary<UniqueId, MimeMessage> messages = new Dictionary<UniqueId, MimeMessage>();

            try
            {
                EstablishConnection(client, Account, "imap.outlook.com");
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
