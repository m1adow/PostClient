using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MimeKit;
using PostClient.Models.Infrastructure;
using System;
using System.Collections.ObjectModel;

namespace PostClient.Models.Services
{
    internal sealed class GmailService : PostService, IService
    {
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
            client.Connect("imap.gmail.com", 993, true);
            client.Authenticate(account.Email, account.Password);
        }

        private void GetMessages(ImapClient client, int[] count, ObservableCollection<MailMessage> messages)
        {
            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadOnly);

            int indexOfLastMessage = inbox.Count - count[0];
            int indexOfFirstMessage = inbox.Count - count[1];

            CheckForOutOfBounds(indexOfLastMessage, inbox.Count, indexOfFirstMessage);

            for (int i = indexOfLastMessage - 1; i > indexOfFirstMessage - 1; i--)
            {
                var messageMime = inbox.GetMessage(i);

                //string body = ((TextPart)messageMime.Body).Text is null ? ((MimeKit.MultipartAlternative)messageMime.Body).TextBody : ((TextPart)messageMime.Body).Text;

                MailMessage message = new MailMessage()
                {
                    Subject = messageMime.Subject,
                    //Body = ((TextPart)messageMime.Body).Text,
                    From = messageMime.From[0].Name,
                    Date = messageMime.Date
                };

                messages.Add(message);
            }
        }

        public void SendMessage(Account account, MimeMessage message)
        {
            SmtpClient client = new SmtpClient();

            try
            {
                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate(account.Email, account.Password);
                client.Send(message);
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
        }
    }
}
