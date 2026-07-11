using HelpDeskHQ.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHQ.Infrastructure.Data
{
    public class HelpDeskHQDbContext : DbContext
    {
        public HelpDeskHQDbContext(DbContextOptions<HelpDeskHQDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<Team> Teams { get; set; } = null!;
        public DbSet<TeamMember> TeamMembers { get; set; } = null!;
        public DbSet<TicketCategory> TicketCategories { get; set; } = null!;
        public DbSet<SlaPolicy> SlaPolicies { get; set; } = null!;
        public DbSet<Ticket> Tickets { get; set; } = null!;
        public DbSet<TicketComment> TicketComments { get; set; } = null!;
        public DbSet<TicketAttachment> TicketAttachments { get; set; } = null!;
        public DbSet<TicketStatusHistory> TicketStatusHistories { get; set; } = null!;
        public DbSet<TicketEscalation> TicketEscalations { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ----- Ticket relationships -----

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.RaisedByUser)
                .WithMany(u => u.RaisedTickets)
                .HasForeignKey(t => t.RaisedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.AssignedAgent)
                .WithMany(u => u.AssignedTickets)
                .HasForeignKey(t => t.AssignedAgentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.TicketCategory)
                .WithMany(c => c.Tickets)
                .HasForeignKey(t => t.TicketCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Team)
                .WithMany(team => team.Tickets)
                .HasForeignKey(t => t.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----- Ticket child records (cascade with the ticket) -----

            modelBuilder.Entity<TicketComment>()
                .HasOne(c => c.Ticket)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment author: restrict so deleting a user never cascades
            // into deleting ticket history/comments.
            modelBuilder.Entity<TicketComment>()
                .HasOne(c => c.AuthorUser)
                .WithMany()
                .HasForeignKey(c => c.AuthorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TicketAttachment>()
                .HasOne(a => a.Ticket)
                .WithMany(t => t.Attachments)
                .HasForeignKey(a => a.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TicketStatusHistory>()
                .HasOne(h => h.Ticket)
                .WithMany(t => t.StatusHistory)
                .HasForeignKey(h => h.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            // Who made the change: restrict, same reasoning as comment author.
            modelBuilder.Entity<TicketStatusHistory>()
                .HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TicketEscalation>()
                .HasOne(e => e.Ticket)
                .WithMany(t => t.Escalations)
                .HasForeignKey(e => e.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TicketEscalation>()
                .HasOne(e => e.EscalatedToUser)
                .WithMany()
                .HasForeignKey(e => e.EscalatedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----- Notifications -----

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Ticket)
                .WithMany()
                .HasForeignKey(n => n.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            // Recipient user: restrict, so deleting a user doesn't silently
            // wipe notification history tied to other users/tickets.
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----- Uniqueness constraints -----

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.TicketNumber)
                .IsUnique();

            // ----- Query performance indexes -----
            // These fields are filtered on constantly: ticket lists, the
            // dashboard, and the two recurring background jobs.

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.Status);

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.AssignedAgentId);

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.CreatedAt);
        }
    }
}