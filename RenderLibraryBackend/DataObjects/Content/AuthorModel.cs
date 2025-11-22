namespace RenderLibraryBackend.DataObjects.Content
{
    public class AuthorModel
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string? Email { get; set; }

        public bool IsAdmin { get; set; }
    }
}