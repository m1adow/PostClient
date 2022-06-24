using MimeKit;
using System.Collections.ObjectModel;

namespace PostClient.Models.Infrastructure
{
    internal interface IService
    {
        void SendMessage(Account account, MimeMessage message);
        ObservableCollection<MailMessage> LoadMessages(Account account, int[] count);
    }
}
