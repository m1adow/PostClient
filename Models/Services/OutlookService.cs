using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using PostClient.Models.Infrastructure;
using System;
using System.Collections.ObjectModel;

namespace PostClient.Models.Services
{
    internal sealed class OutlookService : PostService, IService
    {
        private SmtpClient _client;

        public ObservableCollection<MailMessage> LoadMessages(Account account, int[] count)
        {
            ImapClient client = new ImapClient();
            ObservableCollection<MailMessage> messages = new ObservableCollection<MailMessage>();

            try
            {
                EstablishConnection(client, account);
                GetMessages(client, count, messages);
            }
            catch (Exception exception)
            {
                ShowMessageDialogForException(exception);
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }

            return messages;
        }

        private void EstablishConnection(ImapClient client, Account account)
        {
            client.Connect("imap.outlook.com", 993, true);
            client.Authenticate(account.Email, account.Password);
        }

        private void GetMessages(ImapClient client, int[] count, ObservableCollection<MailMessage> messages)
        {
            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadOnly);

            int indexOfLastMessage = inbox.Count - count[0];
            int indexOfFirstMessage = inbox.Count < count[1] ? 0 : inbox.Count - count[1];

            CheckForOutOfBounds(indexOfLastMessage, inbox.Count, indexOfFirstMessage);

            for (int i = indexOfLastMessage - 1; i > indexOfFirstMessage - 1; i--)
            {
                var messageMime = inbox.GetMessage(i);

                MailMessage message = new MailMessage()
                {
                    Subject = messageMime.Subject,
                    Body = messageMime.HtmlBody ?? messageMime.TextBody,
                    From = messageMime.From[0].Name,
                    Date = messageMime.Date
                };

                messages.Add(message);
            }
        }

        public void SendMessage(Account account, MimeMessage message)
        {
            _client = new SmtpClient();

            try
            {
                _client.Connect("smtp.outlook.com", 587, SecureSocketOptions.StartTls);
                _client.Authenticate(account.Email, account.Password);
                _client.Send(message);
            }
            catch (Exception exception)
            {
                ShowMessageDialogForException(exception);
            }
            finally
            {
                _client.Disconnect(true);
                _client.Dispose();
            }
        }
    }
}
