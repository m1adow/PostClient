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
        public List<MimeMessage> LoadMessages(Account account, SearchQuery searchQuery, Action<string> exceptionHandler)
        {
            ImapClient client = new ImapClient();
            List<MimeMessage> messages = new List<MimeMessage>();

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
    }
}
