using MailKit;
using MailKit.Search;
using MimeKit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PostClient.Models.Infrastructure
{
    internal interface IPostService
    {
        Account Account { get; }
        Task SendMessage(MimeMessage message);
        Task DeleteMessage(MailMessage message, SpecialFolder specialFolder, string subFolder);
        Task FlagMessage(MailMessage message, SpecialFolder specialFolder, string subFolder);
        Dictionary<UniqueId, MimeMessage> LoadMessages(SpecialFolder specialFolder, SearchQuery searchQuery, string subFolder = "");
        void CloseClients();
    }
}
