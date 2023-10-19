using backend.Services.API.Services;

namespace backend.Services.Interfaces
{
    public interface IEmailService
    {
        bool SendEmail(Message message);
    }
}
