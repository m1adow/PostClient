using MailKit;
using MailKit.Net.Imap;
using MimeKit;
using System.Collections.Generic;

namespace PostClient.Models.Services
{
    internal abstract class PostService
    {
        protected void EstablishConnection(ImapClient client, Account account, string imapServer)
        {
            client.Connect(imapServer, 993, true);
            client.Authenticate(account.Email, account.Password);
        }

        protected void GetMessages(ImapClient client, List<MimeMessage> messages)
        {
            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadOnly);

            for (int i = inbox.Count - 1; i > (inbox.Count > 100 ? inbox.Count - 100 : 0); i--)
            {
                var messageMime = inbox.GetMessage(i);
                messages.Add(messageMime);
            }
        }
    }
}
