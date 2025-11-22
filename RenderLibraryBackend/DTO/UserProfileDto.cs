using DataBaseService.Models;

namespace RenderLibraryBackend.DTO
{
    public class UserProfileDto
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string? Email { get; set; }

        public bool IsAdmin { get; set; }

        public decimal Funds { get; set; }

        public int AuthoredPublications { get; set; }

    }
}
