using DataBaseService;
using DataBaseService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RenderLibraryBackend.DTO;
using RenderLibraryBackend.Services.Auth;
using StackExchange.Redis;

namespace RenderLibraryBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private ApplicationContext database;
        private readonly TokenService tokenService;
        private IConnectionMultiplexer redis;

        public UserController(ApplicationContext db, TokenService tokenService, IConnectionMultiplexer multiplexer)
        {
            this.database = db;
            this.tokenService = tokenService;
            this.redis = multiplexer;
        }

        [HttpPost("/signup")]
        public async Task<IActionResult> RegisterUser(RegisterUserDto requestModel)
        {
            var user = new UserModel
            {
                FirstName = requestModel.FirstName,
                LastName = requestModel.LastName,
                Username = requestModel.Username,
                Password = requestModel.Password,
                Email = requestModel.Email,
                IsAdmin = requestModel.IsAdmin ?? false,
                Funds = 0m,                
            };

            database.Users.Add(user);
            await database.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("/login")]
        public async Task<IActionResult> LoginUser(string userName, string password) 
        {
            UserModel? user = await database.Users
                .FirstOrDefaultAsync(u => u.Username == userName && u.Password == password);

            if (user == null)
            {
                return Unauthorized();
            }

            string token = tokenService.GenerateToken(user.Id, user.Username, user.IsAdmin);
            IDatabase db = redis.GetDatabase(7);


            await db.StringSetAsync(user.Id.ToString(), token);
            db.KeyExpire(user.Id.ToString(), TimeSpan.FromHours(1));

            return Ok(token);
        }
    }
}
