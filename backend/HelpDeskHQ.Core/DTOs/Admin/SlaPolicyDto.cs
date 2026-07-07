namespace HelpDeskHQ.Core.DTOs.Admin
{
    public class CreateSlaPolicyDto
    {
        public int TicketCategoryId { get; set; }
        public int Priority { get; set; } // 0=Low, 1=Medium, 2=High, 3=Critical
        public int ResponseTargetMinutes { get; set; }
        public int ResolutionTargetMinutes { get; set; }
    }

    public class SlaPolicyResponseDto
    {
        public int Id { get; set; }
        public int TicketCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public int ResponseTargetMinutes { get; set; }
        public int ResolutionTargetMinutes { get; set; }
    }
}