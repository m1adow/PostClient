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

        protected void GetMessages(ImapClient client, Dictionary<UniqueId, MimeMessage> messages, SpecialFolder specialFolder, SearchQuery searchQuery)
        {
            var folder = client.GetFolder(specialFolder);

            folder.Open(FolderAccess.ReadOnly);

            var uids = folder.Search(searchQuery);

            for (int i = uids.Count - 1; i >= (uids.Count > 100 ? uids.Count - 100 : 0); i--)
            {
                var messageMime = folder.GetMessage(uids[i]);
                messages.Add(uids[i], messageMime);
            }
        }

        protected void DeleteSpecificMessage(ImapClient client, uint uid)
        {
            IList<UniqueId> uids = new List<UniqueId>()
            {
                new UniqueId(uid)
            };

            client.Inbox.Open(FolderAccess.ReadWrite);
            client.Inbox.AddFlags(uids, MessageFlags.Deleted, true);
            client.Inbox.Expunge(uids);
        }

        protected void FlagSpecificMessage(ImapClient client, uint uid, bool isFlagged)
        {
            IList<UniqueId> uids = new List<UniqueId>()
            {
                new UniqueId(uid)
            };

            client.Inbox.Open(FolderAccess.ReadWrite);
            
            if (isFlagged)
                client.Inbox.RemoveFlags(uids, MessageFlags.Flagged, true);
            else
                client.Inbox.AddFlags(uids, MessageFlags.Flagged, true);

            client.Inbox.Expunge(uids);
        }
    }
}
