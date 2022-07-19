using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PostClient.Models.Services
{
    internal abstract class PostService
    {
        protected async Task SendMessage(SmtpClient client, MimeMessage message) => await client.SendAsync(message);

        protected async void EstablishConnection(ImapClient imapClient, SmtpClient smtpClient, Account account, string imapServer, string smtpServer, int smtpPort)
        {
            await imapClient.ConnectAsync(imapServer, 993, true);
            await imapClient.AuthenticateAsync(account.Email, account.Password);
            await smtpClient.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.Auto);
            await smtpClient.AuthenticateAsync(account.Email, account.Password);
        }

        protected Dictionary<UniqueId, MimeMessage> GetMessages(ImapClient client, SearchQuery searchQuery, SpecialFolder specialFolder = SpecialFolder.All, string subFolder = "")
        {
            var messages = new Dictionary<UniqueId, MimeMessage>();

            IMailFolder folder;

            if (client.Capabilities.HasFlag(ImapCapabilities.SpecialUse))
            {
                folder = client.GetFolder(specialFolder);
            }
            else if (!client.Capabilities.HasFlag(ImapCapabilities.SpecialUse) && subFolder == "")
            {
                folder = client.Inbox;
            }
            else
            {
                var personal = client.GetFolder(client.PersonalNamespaces[0]);
                folder = personal.GetSubfolder(subFolder);
            }

            folder.Open(FolderAccess.ReadOnly);

            var uids = folder.Search(searchQuery);

            for (int i = uids.Count - 1; i >= (uids.Count > 100 ? uids.Count - 100 : 0); i--)
            {
                var messageMime = folder.GetMessage(uids[i]);
                messages.Add(uids[i], messageMime);
            }

            return messages;
        }

        protected async Task DeleteMessage(ImapClient client, MailMessage message, bool isOutlook = false) => await DeleteSpecificMessage(client, message.Uid, isOutlook);

        protected async Task DeleteSpecificMessage(ImapClient client, uint uid, bool isOutlook)
        {
            await Task.Run(() =>
            {
                IList<UniqueId> uids = new List<UniqueId>()
                {
                    new UniqueId(uid)
                };

                var folder = isOutlook ? client.Inbox : client.GetFolder(SpecialFolder.All);
                folder.Open(FolderAccess.ReadWrite);
                folder.AddFlags(uids, MessageFlags.Deleted, true);
                folder.Expunge(uids);
            });
        }

        protected async Task FlagMessage(ImapClient client, MailMessage message, bool isOutlook = false) => await FlagSpecificMessage(client, message.Uid, message.IsFlagged, isOutlook);

        protected async Task FlagSpecificMessage(ImapClient client, uint uid, bool isFlagged, bool isOutlook)
        {
            await Task.Run(() =>
            {
                IList<UniqueId> uids = new List<UniqueId>()
                {
                new UniqueId(uid)
                };

                var folder = isOutlook ? client.Inbox : client.GetFolder(SpecialFolder.All);
                folder.Open(FolderAccess.ReadWrite);

                if (isFlagged)
                    folder.RemoveFlags(uids, MessageFlags.Flagged, true);
                else
                    folder.AddFlags(uids, MessageFlags.Flagged, true);

                folder.Expunge(uids);
            });
        }

        protected async void CloseClients(SmtpClient smtpClient, ImapClient imapClient)
        {
            await smtpClient.DisconnectAsync(true);
            await imapClient.DisconnectAsync(true);
            smtpClient.Dispose();
            imapClient.Dispose();
        }
    }
}
