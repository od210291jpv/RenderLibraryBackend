using DataBaseService.Models;

namespace RenderLibraryBackend.DataObjects.Content
{
    public class ContentResponseItem
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
        public AuthorModel Author { get; set; } = null!;
    }
}
