using System.Threading.Tasks;

namespace TaskManagmentSystem.Services
{
    public class ConsoleEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string toEmail, string subject, string body)
        {
            Console.WriteLine("=== EMAIL SENT ===");
            Console.WriteLine($"To: {toEmail}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Body: {body}");
            Console.WriteLine("==================");
            return Task.CompletedTask;
        }
    }
}
