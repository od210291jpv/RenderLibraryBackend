namespace DataBaseService.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string? LastName { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string? Email { get; set; }

        public bool IsAdmin { get; set; }

        public decimal Funds { get; set; }

        public virtual ICollection<PublicationModel> AuthoredPublications { get; set; } = new List<PublicationModel>();

        // 2. Many-to-Many: A user can favorite many publications.
        public virtual ICollection<PublicationModel> FavoritePublications { get; set; } = new List<PublicationModel>();
    }
}
