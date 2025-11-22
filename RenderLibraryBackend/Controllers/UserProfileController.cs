using DataBaseService;
using DataBaseService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RenderLibraryBackend.DTO;
using StackExchange.Redis;

namespace RenderLibraryBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserProfileController : BaseAuthController
    {
        private ApplicationContext db;

        public UserProfileController(IConnectionMultiplexer redis, ApplicationContext database) : base(redis)
        {
            this.db = database;
        }

        [HttpGet("GetUserProfile")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserProfile()
        {
            if (!await IsTokenValid())
            {
                return Unauthorized("Invalid token");
            }
            var userId = await GetUserByToken();

            UserModel? user = this.db.Users.Include(u => u.AuthoredPublications).FirstOrDefault(u => u.Id == userId);
            if (user is null) 
            {
                return NotFound("User not found");
            }
            else
            {
                return Ok(new UserProfileDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Username = user.Username,
                    Email = user.Email,
                    IsAdmin = user.IsAdmin,
                    Funds = user.Funds,
                    AuthoredPublications = user.AuthoredPublications.Count
                });
            }
        }
    }
}
