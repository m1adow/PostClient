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
    public abstract class PostService
    {
        protected async Task SendMessage(SmtpClient client, MimeMessage message) => await client.SendAsync(message);

        protected void EstablishConnection(ImapClient imapClient, SmtpClient smtpClient, Account account, string imapServer, string smtpServer, int smtpPort)
        {
            imapClient.Connect(imapServer, 993, true);
            imapClient.Authenticate(account.Email, account.Password);
            smtpClient.Connect(smtpServer, smtpPort, SecureSocketOptions.Auto);
            smtpClient.Authenticate(account.Email, account.Password);
        }

        protected Dictionary<UniqueId, MimeMessage> GetMessages(ImapClient client, SearchQuery searchQuery, SpecialFolder specialFolder = SpecialFolder.All, string subFolder = "")
        {
            var messages = new Dictionary<UniqueId, MimeMessage>();

            var folder = GetFolder(client, specialFolder, subFolder);

            folder.Open(FolderAccess.ReadOnly);

            var uids = folder.Search(searchQuery);

            for (int i = uids.Count - 1; i >= (uids.Count > 100 ? uids.Count - 100 : 0); i--)
            {
                var messageMime = folder.GetMessage(uids[i]);
                messages.Add(uids[i], messageMime);
            }

            return messages;
        }

        protected async Task DeleteMessage(ImapClient client, MailMessage message, SpecialFolder specialFolder = SpecialFolder.All, string subFolder = "") => await DeleteSpecificMessage(client, specialFolder, message.Uid, subFolder);

        protected async Task DeleteSpecificMessage(ImapClient client, SpecialFolder specialFolder, uint uid, string subFolder)
        {
            await Task.Run(() =>
            {
                IList<UniqueId> uids = new List<UniqueId>()
                {
                    new UniqueId(uid)
                };

                var folder = GetFolder(client, specialFolder, subFolder);
                folder.Open(FolderAccess.ReadWrite);
                folder.AddFlags(uids, MessageFlags.Deleted, true);
                folder.Expunge(uids);
            });
        }

        protected async Task FlagMessage(ImapClient client, MailMessage message, SpecialFolder specialFolder = SpecialFolder.All, string subFolder = "") => await FlagSpecificMessage(client, specialFolder, message.Uid, message.IsFlagged, subFolder);

        protected async Task FlagSpecificMessage(ImapClient client, SpecialFolder specialFolder, uint uid, bool isFlagged, string subFolder)
        {
            await Task.Run(() =>
            {
                IList<UniqueId> uids = new List<UniqueId>()
                {
                    new UniqueId(uid)
                };

                var folder = GetFolder(client, specialFolder, subFolder);
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

        private IMailFolder GetFolder(ImapClient client, SpecialFolder specialFolder, string subFolder)
        {
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

            return folder;
        }
    }
}
