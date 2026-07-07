namespace HelpDeskHQ.Core.DTOs.Admin
{
    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public int TeamId { get; set; }
    }

    public class CategoryResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
    }
}