namespace RenderLibraryBackend.Services.Auth
{
    public class TokenService
    {
        public string GenerateToken(int userId, string username, bool isAdmin)
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + $"{username}:{isAdmin}:{userId}";
        }
    }
}
