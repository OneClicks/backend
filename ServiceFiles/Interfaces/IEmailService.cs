using backend.ServiceFiles.API.Services;

namespace backend.ServiceFiles.Interfaces
{
    public interface IEmailService
    {
        bool SendEmail(Message message);
    }
}
