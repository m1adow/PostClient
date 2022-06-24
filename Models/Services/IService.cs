using MimeKit;

namespace PostClient.Models.Services
{
    public interface IService
    {
        void SendMessage(Account account, MimeMessage message);
    }
}
