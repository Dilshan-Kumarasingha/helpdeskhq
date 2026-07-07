using HelpDeskHQ.Core.Entities;
using HelpDeskHQ.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHQ.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(HelpDeskHQDbContext context)
        {
            // Only seed if no teams exist yet — safe to run every startup
            if (await context.Teams.AnyAsync())
            {
                return;
            }

            // --- Teams ---
            var itSupport = new Team { Name = "IT Support" };
            var networkTeam = new Team { Name = "Network Team" };
            var facilitiesTeam = new Team { Name = "Facilities Team" };

            context.Teams.AddRange(itSupport, networkTeam, facilitiesTeam);
            await context.SaveChangesAsync();

            // --- Categories (linked to teams) ---
            var hardware = new TicketCategory { Name = "Hardware", TeamId = itSupport.Id };
            var software = new TicketCategory { Name = "Software", TeamId = itSupport.Id };
            var accessRequest = new TicketCategory { Name = "Access Request", TeamId = itSupport.Id };
            var network = new TicketCategory { Name = "Network", TeamId = networkTeam.Id };
            var facilities = new TicketCategory { Name = "Facilities", TeamId = facilitiesTeam.Id };

            context.TicketCategories.AddRange(hardware, software, accessRequest, network, facilities);
            await context.SaveChangesAsync();

            // --- SLA Policies (response/resolution targets per category x priority) ---
            var categories = new[] { hardware, software, accessRequest, network, facilities };

            var slaPolicies = new List<SlaPolicy>();

            foreach (var category in categories)
            {
                slaPolicies.Add(new SlaPolicy
                {
                    TicketCategoryId = category.Id,
                    Priority = TicketPriority.Critical,
                    ResponseTargetMinutes = 15,
                    ResolutionTargetMinutes = 240 // 4 hours
                });
                slaPolicies.Add(new SlaPolicy
                {
                    TicketCategoryId = category.Id,
                    Priority = TicketPriority.High,
                    ResponseTargetMinutes = 30,
                    ResolutionTargetMinutes = 480 // 8 hours
                });
                slaPolicies.Add(new SlaPolicy
                {
                    TicketCategoryId = category.Id,
                    Priority = TicketPriority.Medium,
                    ResponseTargetMinutes = 60,
                    ResolutionTargetMinutes = 1440 // 24 hours
                });
                slaPolicies.Add(new SlaPolicy
                {
                    TicketCategoryId = category.Id,
                    Priority = TicketPriority.Low,
                    ResponseTargetMinutes = 120,
                    ResolutionTargetMinutes = 2880 // 48 hours
                });
            }

            context.SlaPolicies.AddRange(slaPolicies);
            await context.SaveChangesAsync();
        }
    }
}