using MimeKit;
using System.Collections.ObjectModel;

namespace PostClient.Models.Services
{
    internal interface IService
    {
        void SendMessage(Account account, MimeMessage message);
        ObservableCollection<MailMessage> LoadMessages(Account account);
    }
}
