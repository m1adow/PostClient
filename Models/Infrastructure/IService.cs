using MailKit;
using MailKit.Search;
using MimeKit;
using System;
using System.Collections.Generic;

namespace PostClient.Models.Infrastructure
{
    internal interface IService
    {
        Account Account { get; }
        void SendMessage(MimeMessage message, Action<string> exceptionHandler);
        void DeleteMessage(MailMessage message, Action<string> exceptionHandler);
        void FlagMessage(MailMessage message, Action<string> exceptionHandler);
        Dictionary<UniqueId, MimeMessage> LoadMessages(SpecialFolder specialFolder, SearchQuery searchQuery, Action<string> exceptionHandler);
    }
}
