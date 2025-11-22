using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace RenderLibraryBackend.Controllers
{
    public class BaseAuthController : Controller
    {
        private IConnectionMultiplexer redis;

        public BaseAuthController(IConnectionMultiplexer redis)
        {
            this.redis = redis;
        }

        internal string? GetToken()
        {
            if (Request.Headers.TryGetValue("Authorization", out var authHeaderValue))
            {
                string token = authHeaderValue.ToString();
                return token;
            }
            return null;
        }

        protected async Task<bool> IsTokenValid() 
        {
            var token = this.GetToken();
            if (token == null)
            {
                return false;
            }

            string? userId = token.Split(':').Last();

            if (userId is null) 
            {
                return false;
            }

            var db = redis.GetDatabase(7);
            return await db.StringGetAsync(userId) == token;
        }

        protected async Task<int> GetUserByToken() 
        {
            string token = this.GetToken() ?? string.Empty;
            string? userId = token.Split(':').Last();

            if (userId is null)
            {
                return -1;
            }

            var db = redis.GetDatabase(7);
            string? storedToken = await db.StringGetAsync(userId);

            if (storedToken == token)
            {
                return int.Parse(userId);
            }

            return -1;
        }
    }
}
