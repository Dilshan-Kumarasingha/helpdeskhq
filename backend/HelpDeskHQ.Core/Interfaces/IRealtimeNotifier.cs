namespace HelpDeskHQ.Core.Interfaces
{
    public interface IRealtimeNotifier
    {
        Task NotifyTicketUpdatedAsync(int ticketId, object payload);
    }
}