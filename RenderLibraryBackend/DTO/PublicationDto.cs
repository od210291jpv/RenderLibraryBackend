namespace RenderLibraryBackend.DTO
{
    public class PublicationDto
    {
        public IFormFile File { get; set; } = default!;

        public string Name { get; set; }

        public decimal Cost { get; set; }

        public bool Hidden { get; set; }

        public bool IsPremium { get; set; }

        public int AuthorId { get; set; }
    }
}
