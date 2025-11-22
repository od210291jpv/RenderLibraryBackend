using DataBaseService;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace RenderLibraryBackend.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class FundsController : BaseAuthController
    {
        private ApplicationContext database;
        private IConnectionMultiplexer redis;

        public FundsController(IConnectionMultiplexer redis, ApplicationContext db) : base(redis)
        {
            this.database = db;
            this.redis = redis;
        }

        [HttpPost("AddFunds")]
        public async Task<IActionResult> AddFunds(decimal amount)
        {
            if (!await IsTokenValid())
            {
                return Unauthorized("Invalid token");
            }

            var userId = await GetUserByToken();

            var user = this.database.Users.Find(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (!user.IsAdmin) 
            {
                return Forbid("Only admins can add funds");
            }

            user.Funds += amount;
             await this.database.SaveChangesAsync();

            // Logic to add funds to the user's account would go here
            return Ok("Funds added successfully");
        }
    }
}
