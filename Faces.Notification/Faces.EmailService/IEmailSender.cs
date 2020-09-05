using System.Threading.Tasks;

namespace Faces.EmailService
{
    public interface IEmailSender
    {
        Task SendEmailAsync(Message message);
    }
}
