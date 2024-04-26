namespace backend.Services.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(string user);
    }
}
