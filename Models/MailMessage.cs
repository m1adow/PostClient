using System;
using System.Collections.Generic;

namespace PostClient.Models
{
#nullable enable

    public sealed class MailMessage
    {
        public uint Uid { get; set; }
        public string? Name { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public List<KeyValuePair<string, byte[]>>? Attachments { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
        public bool IsSeen { get; set; }
        public bool IsFlagged { get; set; }
        public bool IsDraft { get; set; }
        public string Folder { get; set; }
        public DateTimeOffset Date { get; set; }

        public MailMessage()
        {
            Uid = 0;
            Name = String.Empty;
            Subject = String.Empty;
            Body = String.Empty;
            Attachments = new List<KeyValuePair<string, byte[]>>();
            From = String.Empty;
            To = String.Empty;
            IsSeen = false;
            IsFlagged = false;  
            IsDraft = false;
            Folder = "";
            Date = DateTimeOffset.Now;
        }

        public override bool Equals(object obj) => this.Uid == (obj as MailMessage)?.Uid;

        public override int GetHashCode() => base.GetHashCode();
    }
}
