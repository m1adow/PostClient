namespace PostClient.Models
{
#nullable enable

    public sealed class Account
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? PostServiceName { get; set; }

        public Account()
        {
            Email = string.Empty;
            Password = string.Empty;
            PostServiceName = string.Empty;
        }
    }
}
