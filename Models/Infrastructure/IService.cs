using MailKit;
using MailKit.Search;
using MimeKit;
using System;
using System.Collections.Generic;

namespace PostClient.Models.Infrastructure
{
    internal interface IService
    {
        void SendMessage(Account account, MimeMessage message, Action<string> exceptionHandler);
        void DeleteMessage(Account account, MailMessage message, Action<string> exceptionHandler);
        void FlagMessage(Account account, MailMessage message, Action<string> exceptionHandler);
        Dictionary<UniqueId, MimeMessage> LoadMessages(Account account, SearchQuery searchQuery, Action<string> exceptionHandler);
    }
}
