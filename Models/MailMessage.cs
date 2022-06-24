using System;

namespace PostClient.Models
{
    internal sealed class MailMessage
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string From { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}
