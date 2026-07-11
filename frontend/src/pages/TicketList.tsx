import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getTickets } from '../api/tickets';
import type { Ticket } from '../types';
import Badge from '../components/Badge';

export default function TicketList() {
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    async function fetchTickets() {
      try {
        const data = await getTickets();
        setTickets(data);
      } catch (err) {
        setError('Failed to load tickets.');
      } finally {
        setLoading(false);
      }
    }
    fetchTickets();
  }, []);

  if (loading) return <div className="p-8">Loading tickets...</div>;
  if (error) return <div className="p-8 text-red-600">{error}</div>;

  return (
    <div className="p-8 max-w-6xl mx-auto">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">Tickets</h1>
        <Link
          to="/tickets/new"
          className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700"
        >
          + New Ticket
        </Link>
      </div>

      {tickets.length === 0 ? (
        <p className="text-gray-500">No tickets yet.</p>
      ) : (
        <div className="bg-white rounded-lg shadow overflow-hidden">
          <table className="w-full text-left">
            <thead className="bg-gray-50 border-b">
              <tr>
                <th className="p-3 text-sm font-semibold">Ticket #</th>
                <th className="p-3 text-sm font-semibold">Title</th>
                <th className="p-3 text-sm font-semibold">Status</th>
                <th className="p-3 text-sm font-semibold">Priority</th>
                <th className="p-3 text-sm font-semibold">SLA</th>
                <th className="p-3 text-sm font-semibold">Team</th>
              </tr>
            </thead>
            <tbody>
              {tickets.map((ticket) => (
                <tr key={ticket.id} className="border-b hover:bg-gray-50">
                  <td className="p-3">
                    <Link to={`/tickets/${ticket.id}`} className="text-blue-600 hover:underline">
                      {ticket.ticketNumber}
                    </Link>
                  </td>
                  <td className="p-3">{ticket.title}</td>
                  <td className="p-3"><Badge text={ticket.status} variant="status" /></td>
                  <td className="p-3"><Badge text={ticket.priority} variant="priority" /></td>
                  <td className="p-3"><Badge text={ticket.slaBreachStatus} variant="sla" /></td>
                  <td className="p-3 text-sm text-gray-600">{ticket.teamName}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}