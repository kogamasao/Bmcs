using System.Threading.Tasks;

namespace Bmcs.Function
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
