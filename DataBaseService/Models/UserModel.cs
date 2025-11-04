namespace DataBaseService.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string? Email { get; set; }

        public bool IsAdmin { get; set; }

        public decimal Funds { get; set; }

        public virtual ICollection<PublicationModel> AuthoredPublications { get; set; } = new List<PublicationModel>();

        public virtual ICollection<PublicationModel> FavoritePublications { get; set; } = new List<PublicationModel>();
    }
}
