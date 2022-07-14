namespace PostClient.Models
{
    #nullable enable

    internal sealed class Account
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? PostServiceName { get; set; }
    }
}
