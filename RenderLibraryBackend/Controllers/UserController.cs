using DataBaseService;
using DataBaseService.Models;
using Microsoft.AspNetCore.Mvc;
using RenderLibraryBackend.DTO;

namespace RenderLibraryBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private ApplicationContext database;

        public UserController(ApplicationContext db)
        {
            this.database = db;
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
    }
}
