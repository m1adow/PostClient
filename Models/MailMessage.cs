using System;

namespace PostClient.Models
{
    internal sealed class MailMessage
    {
        public uint Uid { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string From { get; set; }
        public bool IsFlagged { get; set; }
        public DateTimeOffset Date { get; set; }

        public MailMessage()
        {
            Uid = 0;
            Subject = String.Empty;
            Body = String.Empty;
            From = String.Empty;
            IsFlagged = false;  
            Date = DateTimeOffset.Now;
        }

        public override bool Equals(object obj) => this.Uid == (obj as MailMessage).Uid;

        public override int GetHashCode() => base.GetHashCode();
    }
}
