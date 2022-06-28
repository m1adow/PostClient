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
        public void SendMessage(Account account, MimeMessage message, Action<string> exceptionHandler)
        {
            SmtpClient client = new SmtpClient();

            try
            {
                client.Connect("smtp.outlook.com", 587, SecureSocketOptions.StartTls);
                client.Authenticate(account.Email, account.Password);
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

        public void DeleteMessage(Account account, MailMessage message, Action<string> exceptionHandler)
        {
            throw new NotImplementedException();
        }

        public void FlagMessage(Account account, MailMessage message, Action<string> exceptionHandler)
        {
            throw new NotImplementedException();
        }

        public Dictionary<UniqueId, MimeMessage> LoadMessages(Account account, SearchQuery searchQuery, Action<string> exceptionHandler)
        {
            ImapClient client = new ImapClient();
            Dictionary<UniqueId, MimeMessage> messages = new Dictionary<UniqueId, MimeMessage>();

            try
            {
                EstablishConnection(client, account, "imap.outlook.com");
                GetMessages(client, messages, searchQuery);
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
