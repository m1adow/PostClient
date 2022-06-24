using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.ObjectModel;

namespace PostClient.Models.Services
{
    internal sealed class GmailService : PostService, IService
    {
        public ObservableCollection<MailMessage> LoadMessages(Account account)
        {
            ImapClient client = new ImapClient();
            ObservableCollection<MailMessage> messages = new ObservableCollection<MailMessage>();

            try
            {
                client.Connect("imap.gmail.com", 993, true);

                client.Authenticate(account.Email, account.Password);

                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadOnly);

                for (int i = inbox.Count - 1; i > inbox.Count - 50; i--)
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
