using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
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

        protected async Task<Dictionary<IMessageSummary, MimeMessage>> GetMessages(ImapClient client, SearchQuery searchQuery, SpecialFolder specialFolder = SpecialFolder.All, string subFolder = "")
        {
            var messages = new Dictionary<IMessageSummary, MimeMessage>();

            var folder = GetFolder(client, specialFolder, subFolder);

            await folder.OpenAsync(FolderAccess.ReadOnly);

            var uids = (await folder.SearchAsync(searchQuery)).Reverse().ToList();

            var items = (await folder.FetchAsync(uids, MessageSummaryItems.All)).Reverse().ToList();

            for (int i = 0; i < (items.Count > 100 ? 100 : items.Count); i++)
                messages.Add(items[i], await folder.GetMessageAsync(uids[i]));

            return messages;
        }

        protected async Task AddFlagToMessage(ImapClient client, MailMessage message, MessageFlags messageFlags, SpecialFolder specialFolder, string subFolder)
        {
            IList<UniqueId> uids = new List<UniqueId>()
            {
                new UniqueId(message.Uid)
            };

            var folder = GetFolder(client, specialFolder, subFolder);
            await folder.OpenAsync(FolderAccess.ReadWrite);

            if (messageFlags == MessageFlags.Flagged)
                await AddOrRemoveFlags(folder, uids, messageFlags, message.IsFlagged);
            if (messageFlags == MessageFlags.Seen)
                await AddOrRemoveFlags(folder, uids, messageFlags, message.IsSeen);
            else
                await folder.AddFlagsAsync(uids, messageFlags, true);

            await folder.ExpungeAsync(uids);
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

        private async Task AddOrRemoveFlags(IMailFolder folder, IList<UniqueId> uids, MessageFlags messageFlags, bool isAdded)
        {
            if (isAdded)
                await folder.RemoveFlagsAsync(uids, messageFlags, true);
            else
                await folder.AddFlagsAsync(uids, messageFlags, true);
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
