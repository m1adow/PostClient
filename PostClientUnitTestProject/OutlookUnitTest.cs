using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MimeKit;
using PostClient.Models;
using PostClient.Models.Helpers;
using PostClient.Models.Infrastructure;
using PostClient.Models.Services;
using PostClient.ViewModels;

namespace PostClientUnitTestProject
{
    [TestClass]
    public class OutlookTests
    {
        private static readonly Account _sendingAccount = new Account() { Email = "somethinggreat3@outlook.com", Password = "BRxeX_rrr82.kUp" };
        private static readonly Account _receivingAccount = new Account() { Email = "somethingmaynothavesense@outlook.com", Password = "heIv4Y%Zsw7!Q*%I51PE^R*SMnGBe8pdx2^c##$#" };

        private readonly OutlookService _sendingService = new OutlookService(_sendingAccount, MessageDialogShower.ShowMessageDialog);
        private readonly OutlookService _loadingService = new OutlookService(_receivingAccount, MessageDialogShower.ShowMessageDialog);

        [TestMethod]
        public async Task SendMessagesTest()
        {
            var sendingViewModel = new SendMessageViewModel(
                    new Func<IPostService>(() => _sendingService),
                    new Func<Account>(() =>
                        _sendingAccount),
                    null);

            string subject = "TestSubject";

            sendingViewModel.MessageReciever = _receivingAccount.Email;
            sendingViewModel.MessageName = "TestName";
            sendingViewModel.MessageSubject = subject;
            sendingViewModel.MessageBody = "TestBody";

            sendingViewModel.SendMessageCommand.Execute(new object());
            var messages = await _loadingService.LoadMessages(MailKit.SpecialFolder.All, MailKit.Search.SearchQuery.All);

            Assert.IsTrue(messages.Any(m => m.Value.Subject == subject));
        }

        [TestMethod]
        public async Task LoadMessagesTest()
        {
            var from = new List<MailboxAddress>() { new MailboxAddress(_sendingAccount.Email, _sendingAccount.Email) };
            var to = new List<MailboxAddress>() { MailboxAddress.Parse(_receivingAccount.Email) };
            var subject = "TestSubject";
            var body = (new BodyBuilder() { HtmlBody = "TestBody"}).ToMessageBody();

            for (int i = 0; i < 5; i++)
                await _sendingService.SendMessage(new MimeMessage(from, to, subject, body));

            var messages = await _loadingService.LoadMessages(MailKit.SpecialFolder.All, MailKit.Search.SearchQuery.All);

            Assert.IsTrue(messages.Count >= 5);
        }
    }
}
