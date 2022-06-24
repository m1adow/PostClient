using MailKit.Net.Smtp;
using MimeKit;
using System;

namespace PostClient.Models.Services
{
    public class GmailService : PostService, IService
    {
        private SmtpClient _client;

        public void SendMessage(Account account, MimeMessage message)
        {
            _client = new SmtpClient();

            try
            {
                _client.Connect("smtp.gmail.com", 465, true);
                _client.Authenticate(account.Email, account.Password);
                _client.Send(message);
            }
            catch (Exception exception)
            {
                ShowMessageDialogForException(exception);
            }
            finally
            {
                _client.Disconnect(true);
                _client.Dispose();
            }
        }
    }
}
