using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MimeKit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PostClient.Models.Services
{
    internal abstract class PostService
    {
        protected async Task SendMessage(SmtpClient client, Account account, string link, MimeMessage message)
        {
            try
            {
                await Task.Run(() =>
                {
                    client.ConnectAsync(link, 465, true);
                    client.AuthenticateAsync(account.Email, account.Password);
                    client.SendAsync(message);
                });
            }
            finally
            {
                await client.DisconnectAsync(true);
                client.Dispose();
            }
        }

        protected async Task EstablishConnectionAsync(ImapClient client, Account account, string imapServer)
        {
            await Task.Run(() =>
            {
                client.Connect(imapServer, 993, true);
                client.Authenticate(account.Email, account.Password);
            });
        }

        protected async Task<Dictionary<UniqueId, MimeMessage>> GetMessagesAsync(ImapClient client, Account account,string link, SpecialFolder specialFolder, SearchQuery searchQuery)
        {
            var messages = new Dictionary<UniqueId, MimeMessage>();

            try
            {
                await EstablishConnectionAsync(client, account, link);

                var folder = client.GetFolder(specialFolder);

                folder.Open(FolderAccess.ReadOnly);

                var uids = folder.Search(searchQuery);

                for (int i = uids.Count - 1; i >= (uids.Count > 100 ? uids.Count - 100 : 0); i--)
                {
                    var messageMime = folder.GetMessage(uids[i]);
                    messages.Add(uids[i], messageMime);
                }
            }
            finally
            {
                await client.DisconnectAsync(true);
                client.Dispose();
            }

            return messages;
        }

        protected async Task DeleteMessage(ImapClient client, Account account, string link, MailMessage message)
        {
            try
            {
                await EstablishConnectionAsync(client, account, link);
                await DeleteSpecificMessage(client, message.Uid);
            }
            finally
            {
                await client.DisconnectAsync(true);
                client.Dispose();
            }
        }

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

        protected async Task FlagMessage(ImapClient client, Account account, string link, MailMessage message)
        {
            try
            {
                await EstablishConnectionAsync(client, account, link);
                await FlagSpecificMessage(client, message.Uid, message.IsFlagged);
            }
            finally
            {
                await client.DisconnectAsync(true);
                client.Dispose();
            }
        }

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
    }
}
