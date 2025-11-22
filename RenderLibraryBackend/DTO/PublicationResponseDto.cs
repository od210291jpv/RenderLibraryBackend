namespace RenderLibraryBackend.DTO
{
    public class PublicationResponseDto : PublicationDto
    {
        public int Id { get; set; }

        public string Url { get; set; } = string.Empty;
    }
}
