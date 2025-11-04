namespace DataBaseService.Models
{
    public class PublicationModel
    {
        public int Id { get; set; }

        public string Url { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public short Rating { get; set; }

        public decimal Cost { get; set; }

        public int Likes { get; set; }

        public bool Hidden { get; set; }

        public bool IsPremium { get; set; }

        public int AuthorId { get; set; }
        public virtual UserModel Author { get; set; } = null!;

        public virtual ICollection<UserModel> FavoritedByUsers { get; set; } = new List<UserModel>();
    }
}
