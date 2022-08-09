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
#nullable enable

    public sealed class GmailService : PostService, IPostService
    {
        public Account Account { get; }

        private readonly SmtpClient _smtpClient;
        private readonly ImapClient _imapClient;

        private const string _smtpLink = "smtp.gmail.com";
        private const string _imapLink = "imap.gmail.com";

        private readonly Action<string, string> _exceptionHandler;
        private bool _isImapClientEngaged;

        public GmailService(Account account, Action<string, string> exceptionHandler)
        {
            this.Account = account;
            _smtpClient = new SmtpClient();
            _imapClient = new ImapClient();
            _exceptionHandler = exceptionHandler;

            EstablishConnection(_imapClient, _smtpClient, account, _imapLink, _smtpLink, 465, _exceptionHandler);
        }

        public async Task SendMessage(MimeMessage message) => await SendMessage(_smtpClient, message, _exceptionHandler);

        public async Task FlagMessage(MailMessage message, MessageFlags messageFlags, SpecialFolder specialFolder, string subFolder) => await AddFlagToMessage(_imapClient, message, messageFlags, specialFolder, subFolder);

        public async Task<Dictionary<IMessageSummary, MimeMessage>> LoadMessages(SpecialFolder specialFolder, SearchQuery searchQuery, string subFolder = "")
        {
            var result = new Dictionary<IMessageSummary, MimeMessage>();

            if (!_isImapClientEngaged)
            {
                _isImapClientEngaged = true;
                result = await GetMessages(_imapClient, searchQuery, specialFolder, subFolder); 
                _isImapClientEngaged = false;
            }
            else
                throw new Exception("Wait for end of previous action");

            return result;
        }

        public void CloseClients() => CloseClients(_smtpClient, _imapClient);
    }
}
