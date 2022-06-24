using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;

namespace PostClient.Models.Services
{
    public class OutlookService : PostService, IService
    {
        private SmtpClient _client;

        public void SendMessage(Account account, MimeMessage message)
        {
            _client = new SmtpClient();

            try
            {
                _client.Connect("smtp.outlook.com", 587, SecureSocketOptions.StartTls);
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
