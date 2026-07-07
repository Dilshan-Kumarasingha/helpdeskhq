namespace HelpDeskHQ.Core.DTOs.Admin
{
    public class CreateTeamDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class TeamResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public int CategoryCount { get; set; }
    }
}