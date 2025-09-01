using System.Threading.Tasks;

namespace TaskManagmentSystem.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}


