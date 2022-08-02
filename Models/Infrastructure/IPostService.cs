 using MailKit;
using MailKit.Search;
using MimeKit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PostClient.Models.Infrastructure
{
    public interface IPostService
    {
        Account Account { get; }
        Task SendMessage(MimeMessage message);
        Task FlagMessage(MailMessage message, MessageFlags messageFlags, SpecialFolder specialFolder, string subFolder);
        Task<Dictionary<IMessageSummary, MimeMessage>> LoadMessages(SpecialFolder specialFolder, SearchQuery searchQuery, string subFolder = "");
        void CloseClients();
    }
}
