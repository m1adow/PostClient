using MailKit;
using MailKit.Search;
using MimeKit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PostClient.Models.Infrastructure
{
    internal interface IService
    {
        Account Account { get; }
        Task SendMessage(MimeMessage message);
        Task DeleteMessage(MailMessage message);
        Task FlagMessage(MailMessage message);
        Task<Dictionary<UniqueId, MimeMessage>> LoadMessages(SpecialFolder specialFolder, SearchQuery searchQuery);
    }
}
