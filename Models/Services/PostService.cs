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

        protected void EstablishConnection(ImapClient imapClient, SmtpClient smtpClient, Account account, string imapServer, string smtpServer, int smtpPort)
        {
            imapClient.Connect(imapServer, 993, true);
            imapClient.Authenticate(account.Email, account.Password);
            smtpClient.Connect(smtpServer, smtpPort, SecureSocketOptions.Auto);
            smtpClient.Authenticate(account.Email, account.Password);
        }

        protected Dictionary<UniqueId, MimeMessage> GetMessagesAsync(ImapClient client, SpecialFolder specialFolder, SearchQuery searchQuery)
        {
            var messages = new Dictionary<UniqueId, MimeMessage>();

            var folder = client.GetFolder(specialFolder);

            folder.Open(FolderAccess.ReadOnly);

            var uids = folder.Search(searchQuery);

            for (int i = uids.Count - 1; i >= (uids.Count > 100 ? uids.Count - 100 : 0); i--)
            {
                var messageMime = folder.GetMessage(uids[i]);
                messages.Add(uids[i], messageMime);
            }

            return messages;
        }

        protected async Task DeleteMessage(ImapClient client, MailMessage message) => await DeleteSpecificMessage(client, message.Uid);

        protected async Task DeleteSpecificMessage(ImapClient client, uint uid)
        {
            await Task.Run(() =>
            {
                IList<UniqueId> uids = new List<UniqueId>()
                {
                    new UniqueId(uid)
                };

                var folder = client.GetFolder(SpecialFolder.All);
                folder.Open(FolderAccess.ReadWrite);
                folder.AddFlags(uids, MessageFlags.Deleted, true);
                folder.Expunge(uids);
            });
        }

        protected async Task FlagMessage(ImapClient client, MailMessage message) => await FlagSpecificMessage(client, message.Uid, message.IsFlagged);

        protected async Task FlagSpecificMessage(ImapClient client, uint uid, bool isFlagged)
        {
            await Task.Run(() =>
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
