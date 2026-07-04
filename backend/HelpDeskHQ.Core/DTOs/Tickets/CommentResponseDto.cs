namespace HelpDeskHQ.Core.DTOs.Tickets
{
    public class CommentResponseDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}