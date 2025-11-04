using DataBaseService;
using DataBaseService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RenderLibraryBackend.DataObjects;
using RenderLibraryBackend.DataObjects.Content;
using RenderLibraryBackend.DTO;
using StackExchange.Redis;

namespace RenderLibraryBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContentController : BaseAuthController
    {
        private ApplicationContext database;
        private IConnectionMultiplexer redis;

        public ContentController(ApplicationContext db, IConnectionMultiplexer multiplexer) : base(multiplexer)
        {
            this.database = db;
            this.redis = multiplexer;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(PublicationDto requestData)
        {
            if (!await IsTokenValid())
            {
                return Unauthorized("Invalid token");
            }

            var host = HttpContext.Request.Host.ToUriComponent();

            if (requestData.File == null || requestData.File.Length == 0)
                return NotFound("file not selected");

            if (this.database.Users.SingleOrDefault(u => u.Id == requestData.AuthorId) is null)
            {
                return NotFound($"{requestData.AuthorId} user not found");
            }

            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot/img",
                        requestData.File.FileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await requestData.File.CopyToAsync(stream);
            }

            string fileUrl = $"{HttpContext.Request.Scheme}://{host}/img/{requestData.File.FileName}";

            PublicationModel model = new PublicationModel
            {
                Url = fileUrl,
                Name = requestData.Name,
                Cost = requestData.Cost,
                Author = this.database.Users.SingleOrDefault(u => u.Id == requestData.AuthorId) ?? null,
                AuthorId = requestData.AuthorId,
                Hidden = requestData.Hidden,
                IsPremium = requestData.IsPremium,
                Likes = 0,
                Rating = 0
            };

            string serialized = JsonConvert.SerializeObject(model);
            await this.database.Publications.AddAsync(model);

            if (await this.database.SaveChangesAsync() > 0)
            {
                return Ok(serialized);
            }

            return Accepted(serialized);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllimages(bool showHidden = false, int page = 0, int pageSize = 0, string? query = null)
        {
            if (!await IsTokenValid())
            {
                return Unauthorized("Invalid token");
            }

            var imageQuery = this.database.Publications.AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                imageQuery = imageQuery.Where(i => i.Name.Contains(query));
            }

            var totalCount = await imageQuery.CountAsync();
            var items = await imageQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var result = new PaginatedResult<ContentResponseItem>
            {
                Items = items.Where(i => i.Hidden == showHidden).Select(i => new ContentResponseItem
                {
                    Id = i.Id,
                    Url = i.Url,
                    Name = i.Name,
                    Rating = i.Rating,
                    Cost = i.Cost,
                    Likes = i.Likes,
                    Hidden = i.Hidden,
                    IsPremium = i.IsPremium,
                    AuthorId = i.AuthorId,
                    Author = i.Author,
                    FavoritedByUsers = i.FavoritedByUsers
                }).ToList(),
                TotalCount = totalCount
            };
            return Ok(result);
        }
    }
}
