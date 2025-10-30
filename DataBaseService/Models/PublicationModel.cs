namespace DataBaseService.Models
{
    public class PublicationModel
    {
        public int Id { get; set; }

        public string Url { get; set; }

        public string Name { get; set; }

        public short Rating { get; set; }

        public decimal Cost { get; set; }

        public int Likes { get; set; }

        public bool Hidden { get; set; }

        public bool IsPremium { get; set; }

        public int AuthorId { get; set; } // Foreign Key
        public virtual UserModel Author { get; set; }

        // 2. Many-to-Many: This publication can be favorited by many users.
        public virtual ICollection<UserModel> FavoritedByUsers { get; set; } = new List<UserModel>();
    }
}
