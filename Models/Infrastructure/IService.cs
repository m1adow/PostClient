using MimeKit;
using System.Collections.Generic;

namespace PostClient.Models.Infrastructure
{
    internal interface IService
    {
        void SendMessage(Account account, MimeMessage message);
        List<MimeMessage> LoadMessages(Account account);
    }
}
