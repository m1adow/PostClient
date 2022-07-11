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
        void SendMessage(MimeMessage message);
        void DeleteMessage(MailMessage message);
        void FlagMessage(MailMessage message);
        Dictionary<UniqueId, MimeMessage> LoadMessages(SpecialFolder specialFolder, SearchQuery searchQuery);
    }
}
