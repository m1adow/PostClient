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
            await Task.Delay(4000);
            var messages = await _loadingService.LoadMessages(MailKit.SpecialFolder.All, MailKit.Search.SearchQuery.All);

            Assert.IsTrue(messages.Any(m => m.Value.Subject == subject));

            foreach (var message in messages)
                await _loadingService.FlagMessage(new MailMessage { Uid = message.Key.UniqueId.Id }, MailKit.MessageFlags.Deleted, MailKit.SpecialFolder.All, "");
        }

        [TestMethod]
        public async Task LoadMessagesTest()
        {
            await _sendingService.SendMessage(CreateMessage("LoadMessagesTest"));
            await Task.Delay(4000);
            var messages = await _loadingService.LoadMessages(MailKit.SpecialFolder.All, MailKit.Search.SearchQuery.All);

            Assert.IsTrue(messages.Count >= 1);

            foreach (var message in messages)
                await _loadingService.FlagMessage(new MailMessage { Uid = message.Key.UniqueId.Id }, MailKit.MessageFlags.Deleted, MailKit.SpecialFolder.All, "");
        }

        [TestMethod]
        public async Task FlagMessageTest()
        {
            await _sendingService.SendMessage(CreateMessage("FlagMessage"));
            await Task.Delay(4000);
            var messages = await _loadingService.LoadMessages(MailKit.SpecialFolder.All, MailKit.Search.SearchQuery.All);
            var mailMessage = new MailMessage { Uid = messages.FirstOrDefault(m => m.Value.Subject == "FlagMessage").Key.UniqueId.Id };

            await _loadingService.FlagMessage(mailMessage, MailKit.MessageFlags.Flagged, MailKit.SpecialFolder.All, "");
            var flaggedMessages = await _loadingService.LoadMessages(MailKit.SpecialFolder.All, MailKit.Search.SearchQuery.Flagged);

            Assert.IsTrue(flaggedMessages.Any(f => f.Key.UniqueId.Id == mailMessage.Uid));
            await _loadingService.FlagMessage(mailMessage, MailKit.MessageFlags.Deleted, MailKit.SpecialFolder.All, "");
        }

        [TestMethod]
        public async Task DeleteMessageTest()
        {
            await _sendingService.SendMessage(CreateMessage("DeleteMessage"));
            await Task.Delay(4000);
            var messages = await _loadingService.LoadMessages(MailKit.SpecialFolder.All, MailKit.Search.SearchQuery.All);
            var mailMessage = new MailMessage { Uid = messages.FirstOrDefault(m => m.Value.Subject == "DeleteMessage").Key.UniqueId.Id };

            await _loadingService.FlagMessage(mailMessage, MailKit.MessageFlags.Deleted, MailKit.SpecialFolder.All, "");
            await Task.Delay(4000);

            Assert.IsTrue((await _loadingService.LoadMessages(MailKit.SpecialFolder.All, MailKit.Search.SearchQuery.All)).Values.Count == 0);
        }

        private MimeMessage CreateMessage(string subject)
        {
            var from = new List<MailboxAddress>() { new MailboxAddress(_sendingAccount.Email, _sendingAccount.Email) };
            var to = new List<MailboxAddress>() { MailboxAddress.Parse(_receivingAccount.Email) };
            var body = (new BodyBuilder() { HtmlBody = "TestBody" }).ToMessageBody();

            return new MimeMessage(from, to, subject, body);
        }
    }
}
