namespace TaskManagmentSystem.Services
{
    public class EmailSettings
    {
        public string Host { get; set; }
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}


