using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PostClient.Models.Services
{
    public abstract class PostService
    {
        protected void EstablishConnection(ImapClient imapClient, SmtpClient smtpClient, Account account, string imapServer, string smtpServer, int smtpPort, Action<string> excpetionHandler)
        {
            try
            {
                imapClient.Connect(imapServer, 993, true);
                imapClient.Authenticate(account.Email, account.Password);
                smtpClient.Connect(smtpServer, smtpPort, SecureSocketOptions.Auto);
                smtpClient.Authenticate(account.Email, account.Password);
            }
            catch (Exception exception)
            {
                excpetionHandler(exception.Message);
            }
        }

        protected async Task SendMessage(SmtpClient client, MimeMessage message, Action<string> exceptionHandler)
        {
            try
            {
                await client.SendAsync(message);
            }
            catch (Exception exception)
            {
                exceptionHandler(exception.Message);
            }
        }

        protected async Task<Dictionary<UniqueId, MimeMessage>> GetMessages(ImapClient client, SearchQuery searchQuery, SpecialFolder specialFolder = SpecialFolder.All, string subFolder = "")
        {
            var messages = new Dictionary<UniqueId, MimeMessage>();

            var folder = GetFolder(client, specialFolder, subFolder);

            await folder.OpenAsync(FolderAccess.ReadOnly);

            var uids = await folder.SearchAsync(searchQuery);

            for (int i = uids.Count - 1; i >= (uids.Count > 100 ? uids.Count - 100 : 0); i--)
                messages.Add(uids[i], await folder.GetMessageAsync(uids[i]));

            return messages;
        }

        protected async Task DeleteMessage(ImapClient client, MailMessage message, SpecialFolder specialFolder = SpecialFolder.All, string subFolder = "") => await DeleteSpecificMessage(client, specialFolder, message.Uid, subFolder);

        protected async Task DeleteSpecificMessage(ImapClient client, SpecialFolder specialFolder, uint uid, string subFolder)
        {
            IList<UniqueId> uids = new List<UniqueId>()
            {
                    new UniqueId(uid)
            };

            var folder = GetFolder(client, specialFolder, subFolder);
            await folder.OpenAsync(FolderAccess.ReadWrite);
            await folder.AddFlagsAsync(uids, MessageFlags.Deleted, true);
            await folder.ExpungeAsync(uids);
        }

        protected async Task FlagMessage(ImapClient client, MailMessage message, SpecialFolder specialFolder = SpecialFolder.All, string subFolder = "") => await FlagSpecificMessage(client, specialFolder, message.Uid, message.IsFlagged, subFolder);

        protected async Task FlagSpecificMessage(ImapClient client, SpecialFolder specialFolder, uint uid, bool isFlagged, string subFolder)
        {
            IList<UniqueId> uids = new List<UniqueId>()
            {
                new UniqueId(uid)
            };

            var folder = GetFolder(client, specialFolder, subFolder);
            await folder.OpenAsync(FolderAccess.ReadWrite);

            if (isFlagged)
                await folder.RemoveFlagsAsync(uids, MessageFlags.Flagged, true);
            else
                await folder.AddFlagsAsync(uids, MessageFlags.Flagged, true);

            await folder.ExpungeAsync(uids);
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
