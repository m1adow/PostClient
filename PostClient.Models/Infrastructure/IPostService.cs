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
        Task SendMessageAsync(MimeMessage message);
        Task FlagMessageAsync(MailMessage message, MessageFlags messageFlags, SpecialFolder specialFolder, string subFolder);
        Task<Dictionary<IMessageSummary, MimeMessage>> LoadMessagesAsync(SpecialFolder specialFolder, SearchQuery searchQuery, string subFolder = "");
        Task DeletePopMessageAsync(int index);
        Task<Dictionary<int, MimeMessage>> LoadPopMessagesAsync();
        Task CloseClients();
    }
}
