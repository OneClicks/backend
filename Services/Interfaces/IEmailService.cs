using backend.Service.API.Services;

namespace backend.Service.Interfaces
{
    public interface IEmailService
    {
        bool SendEmail(Message message);
    }
}
