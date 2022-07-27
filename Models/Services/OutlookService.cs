using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MimeKit;
using PostClient.Models.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PostClient.Models.Services
{
    public sealed class OutlookService : PostService, IPostService
    {
        public Account Account { get; }

        private readonly SmtpClient _smtpClient;
        private readonly ImapClient _imapClient;

        private const string _smtpLink = "smtp.outlook.com";
        private const string _imapLink = "imap.outlook.com";

        private readonly Action<string> _exceptionHandler;
        private bool _isImapClientEngaged;

        public OutlookService(Account account, Action<string> exceptionHandler)
        {
            this.Account = account;
            _smtpClient = new SmtpClient();
            _imapClient = new ImapClient();
            _exceptionHandler = exceptionHandler;

            EstablishConnection(_imapClient, _smtpClient, account, _imapLink, _smtpLink, 587, _exceptionHandler);
        }

        public async Task SendMessage(MimeMessage message) => await SendMessage(_smtpClient, message, _exceptionHandler);

        public async Task DeleteMessage(MailMessage message, SpecialFolder specialFolder, string subFolder) => await DeleteMessage(_imapClient, message, specialFolder, subFolder);

        public async Task FlagMessage(MailMessage message, SpecialFolder specialFolder, string subFolder) => await FlagMessage(_imapClient, message, specialFolder, subFolder);

        public async Task<Dictionary<UniqueId, MimeMessage>> LoadMessages(SpecialFolder specialFolder, SearchQuery searchQuery, string subFolder = "")
        {
            var result = new Dictionary<UniqueId, MimeMessage>();

            if (!_isImapClientEngaged)
            {
                _isImapClientEngaged = true;
                result = await GetMessages(_imapClient, searchQuery, subFolder: subFolder);
                _isImapClientEngaged = false;
            }
            else
                throw new System.Exception("Wait for end of previous action");

            return result;
        }

        public void CloseClients() => CloseClients(_smtpClient, _imapClient);
    }
}
