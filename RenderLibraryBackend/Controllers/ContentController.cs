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

        [HttpPost("UploadImage")]
        [ProducesResponseType(typeof(PublicationResponseDto), StatusCodes.Status200OK)]
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

            var user = this.database.Users.SingleOrDefault(u => u.Id == requestData.AuthorId);
            if (user != null)
            {
                user.Funds += requestData.Cost * 0.5m; // Add 50% of the cost to the author's funds
            }

            PublicationResponseDto result = new PublicationResponseDto
            {
                Id = model.Id,
                Url = model.Url,
                Name = model.Name,
                Cost = model.Cost,
                AuthorId = model.AuthorId,
                Hidden = model.Hidden,
                IsPremium = model.IsPremium,
            };

            if (await this.database.SaveChangesAsync() > 0)
            {
                return Ok(result);
            }

            return Accepted(result);
        }

        [HttpGet("GetAll")]
        [ProducesResponseType(typeof(PaginatedResult<ContentResponseItem>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllimages(bool showHidden = false, int page = 0, int pageSize = 10, string? query = null)
        {
            if (!await IsTokenValid())
            {
                return Unauthorized("Invalid token");
            }

            if (pageSize <= 0) pageSize = 10;
            if (page < 0) page = 0;

            var imageQuery = this.database.Publications.AsNoTracking();

            if (!string.IsNullOrEmpty(query))
            {
                imageQuery = imageQuery.Where(i => i.Name.Contains(query));
            }

            imageQuery = imageQuery.Where(i => i.Hidden == showHidden);

            var totalCount = await imageQuery.CountAsync();

            var items = await imageQuery
                .OrderByDescending(i => i.Id)
                .Skip(pageSize * page)
                .Take(pageSize)
                .Select(i => new ContentResponseItem
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
                    Author = new AuthorModel 
                    {
                        Id = i.Author.Id,
                        Email = i.Author.Email,
                        IsAdmin = i.Author.IsAdmin,
                        Username = i.Author.Username
                    },
                })
                .ToListAsync();

            PaginatedResult<ContentResponseItem> result = new PaginatedResult<ContentResponseItem>
            {
                Items = items,
                TotalCount = totalCount
            };

            return Ok(result);
        }

        [HttpPost("AddFavoriteImage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AddFavoriteImage(int imageId)
        {
            if (!await IsTokenValid())
            {
                return Unauthorized("Invalid token");
            }

            if (this.database.Publications.SingleOrDefault(i => i.Id == imageId) is not PublicationModel image)
            {
                return NotFound("Image not found");
            }
            else
            {
                int userId = await this.GetUserByToken();
                UserModel? user = this.database.Users.Include(u => u.FavoritePublications).SingleOrDefault(u => u.Id == userId);

                if (user is null)
                {
                    return NotFound("User not found");
                }



                if (user.FavoritePublications is null)
                {
                    user.FavoritePublications = new List<PublicationModel>();
                }

                if (!user.FavoritePublications.Contains(image))
                {
                    if (user.Funds < image.Cost)
                    {
                        return BadRequest("Insufficient funds to favorite this image.");
                    }

                    user.FavoritePublications.Add(image);
                    user.Funds -= image.Cost; // Deduct the cost from user's funds
                    await this.database.SaveChangesAsync();
                }
            }

            return Ok();
        }

        [HttpPost("Unfavorite")]
        public async Task<IActionResult> RemoveFavoriteImage(int imageId)
        {
            if (!await IsTokenValid())
            {
                return Unauthorized("Invalid token");
            }

            if (this.database.Publications.SingleOrDefault(i => i.Id == imageId) is not PublicationModel image)
            {
                return NotFound("Image not found");
            }
            else
            {
                int userId = await this.GetUserByToken();
                UserModel? user = this.database.Users.Include(u => u.FavoritePublications).SingleOrDefault(u => u.Id == userId);

                if (user is null)
                {
                    return NotFound("User not found");
                }
                if (user.FavoritePublications is null)
                {
                    user.FavoritePublications = new List<PublicationModel>();
                }
                if (user.FavoritePublications.Contains(image))
                {
                    user.FavoritePublications.Remove(image);
                    await this.database.SaveChangesAsync();
                }
            }
            return Ok();
        }

        [HttpGet("GetFavoriteImages")]
        [ProducesResponseType(typeof(List<ContentResponseItem>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFavoriteImages()
        {
            if (!await IsTokenValid())
            {
                return Unauthorized("Invalid token");
            }

            int userId = await this.GetUserByToken();

            UserModel? user = this.database.Users
                .Include(u => u.FavoritePublications)
                .ThenInclude(p => p.Author)
                .SingleOrDefault(u => u.Id == userId);

            if (user is null)
            {
                return NotFound("User not found");
            }

            List<ContentResponseItem> favoriteImages = user.FavoritePublications.Select(i => new ContentResponseItem
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
                Author = new AuthorModel 
                {
                    Id = i.Author.Id,
                    Email = i.Author.Email,
                    IsAdmin = i.Author.IsAdmin,
                    Username = i.Author.Username
                },
            }).ToList();

            return Ok(favoriteImages);
        }

        [HttpPost("LikeImage")]
        public async Task<IActionResult> LikeImage(int imageId)
        {
            if (!await IsTokenValid())
            {
                return Unauthorized("Invalid token");
            }
            var image = this.database.Publications.SingleOrDefault(i => i.Id == imageId);
            if (image == null)
            {
                return NotFound("Image not found");
            }
            image.Likes += 1;
            await this.database.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("UnlikeImage")]
        public async Task<IActionResult> UnlikeImage(int imageId)
        {
            if (!await IsTokenValid())
            {
                return Unauthorized("Invalid token");
            }
            var image = this.database.Publications.SingleOrDefault(i => i.Id == imageId);
            if (image == null)
            {
                return NotFound("Image not found");
            }
            image.Likes = Math.Max(0, image.Likes - 1);
            await this.database.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("RateImage")]
        public async Task<IActionResult> RateImage(int imageId, short rating)
        {
            if (!await IsTokenValid())
            {
                return Unauthorized("Invalid token");
            }
            var image = this.database.Publications.SingleOrDefault(i => i.Id == imageId);
            if (image == null)
            {
                return NotFound("Image not found");
            }
            image.Rating = rating;
            await this.database.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("HideImage")]
        public async Task<IActionResult> HideImage(int imageId, bool hide)
        {
            if (!await IsTokenValid())
            {
                return Unauthorized("Invalid token");
            }

            var userId = await this.GetUserByToken();
            var user = this.database.Users.SingleOrDefault(u => u.Id == userId);

            if (user == null || !user.IsAdmin)
            {
                return Forbid("Only admins can hide or unhide images.");
            }

            var image = this.database.Publications.SingleOrDefault(i => i.Id == imageId);
            if (image == null)
            {
                return NotFound("Image not found");
            }
            image.Hidden = hide;
            await this.database.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("GetImageRating")]
        [ProducesResponseType(typeof(short), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetImageRating(int imageId)
        {
            if (!await IsTokenValid())
            {
                return Unauthorized("Invalid token");
            }
            var image = this.database.Publications.SingleOrDefault(i => i.Id == imageId);
            if (image == null)
            {
                return NotFound("Image not found");
            }
            return Ok(image.Rating);
        }

        [HttpGet("GetImageById")]
        [ProducesResponseType(typeof(ContentResponseItem), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetImageById(int imageId)
        {
            if (!await IsTokenValid())
            {
                return Unauthorized("Invalid token");
            }

            var image = this.database.Publications.Include(p => p.Author).SingleOrDefault(i => i.Id == imageId);
            if (image == null)
            {
                return NotFound("Image not found");
            }

            return Ok(new ContentResponseItem
            {
                Id = image.Id,
                Url = image.Url,
                Name = image.Name,
                Rating = image.Rating,
                Cost = image.Cost,
                Likes = image.Likes,
                Hidden = image.Hidden,
                IsPremium = image.IsPremium,
                AuthorId = image.AuthorId,
                Author = new AuthorModel
                {
                    Id = image.Author.Id,
                    Email = image.Author.Email,
                    IsAdmin = image.Author.IsAdmin,
                    Username = image.Author.Username
                },
            });
        }
    }
}
