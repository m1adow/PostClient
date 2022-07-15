using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using PostClient.Models.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PostClient.Models.Services
{
    internal sealed class OutlookService : PostService, IService
    {
        public Account Account { get; }

        private SmtpClient _smtpClient;
        private ImapClient _imapClient;

        private const string _smtpLink = "smtp.outlook.com";
        private const string _imapLink = "imap.outlook.com";

        public OutlookService(Account account)
        {
            this.Account = account;
            _smtpClient = new SmtpClient();
            _imapClient = new ImapClient();
        }

        public async Task SendMessage(MimeMessage message) => await SendMessage(_smtpClient, Account, _smtpLink, message);

        public async Task DeleteMessage(MailMessage message) => await DeleteMessage(_imapClient, Account, _imapLink, message);

        public async Task FlagMessage(MailMessage message) => await FlagMessage(_imapClient, Account, _imapLink, message);

        public async Task<Dictionary<UniqueId, MimeMessage>> LoadMessages(SpecialFolder specialFolder, SearchQuery searchQuery) => await GetMessagesAsync(_imapClient, Account, _imapLink, specialFolder, searchQuery);
    }
}
