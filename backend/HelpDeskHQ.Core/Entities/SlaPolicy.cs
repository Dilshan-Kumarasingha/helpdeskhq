using HelpDeskHQ.Core.Enums;

namespace HelpDeskHQ.Core.Entities
{
    public class SlaPolicy
    {
        public int Id { get; set; }

        public int TicketCategoryId { get; set; }
        public TicketCategory TicketCategory { get; set; } = null!;

        public TicketPriority Priority { get; set; }

        // Targets stored in minutes - simplest unit to do math with
        public int ResponseTargetMinutes { get; set; }
        public int ResolutionTargetMinutes { get; set; }
    }
}