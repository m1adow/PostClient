using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
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

        protected void GetMessages(ImapClient client, List<MimeMessage> messages, SearchQuery searchQuery)
        {
            client.Inbox.Open(FolderAccess.ReadOnly);
            var uids = client.Inbox.Search(searchQuery);

            for (int i = uids.Count - 1; i >= (uids.Count > 100 ? uids.Count - 100 : 0); i--)
            {
                var messageMime = client.Inbox.GetMessage(uids[i]);
                messages.Add(messageMime);
            }
        }
    }
}
