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

            var folder = client.GetFolder(SpecialFolder.All);
            folder.Open(FolderAccess.ReadWrite);
            folder.AddFlags(uids, MessageFlags.Deleted, true);
            folder.Expunge(uids);
        }

        protected void FlagSpecificMessage(ImapClient client, uint uid, bool isFlagged)
        {
            IList<UniqueId> uids = new List<UniqueId>()
            {
                new UniqueId(uid)
            };

            var folder = client.GetFolder(SpecialFolder.All);
            folder.Open(FolderAccess.ReadWrite);

            if (isFlagged)
                folder.RemoveFlags(uids, MessageFlags.Flagged, true);
            else
                folder.AddFlags(uids, MessageFlags.Flagged, true);

            folder.Expunge(uids);
        }
    }
}
