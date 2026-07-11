import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { getDashboardSummary } from '../api/dashboard';
import { getTickets } from '../api/tickets';
import type { DashboardSummary, Ticket } from '../types';
import Badge from '../components/Badge';

export default function Dashboard() {
  const { user } = useAuth();
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [myTickets, setMyTickets] = useState<Ticket[]>([]);
  const [loading, setLoading] = useState(true);

  const isEmployee = user?.role === 'Employee';
  const isAgentOrLead = user?.role === 'SupportAgent' || user?.role === 'TeamLead';
  const isAdmin = user?.role === 'Admin';

  useEffect(() => {
    async function load() {
      try {
        const tickets = await getTickets();
        setMyTickets(tickets);

        // Only Agents/Leads/Admins get the global summary — Employees just see their own tickets
        if (!isEmployee) {
          const data = await getDashboardSummary();
          setSummary(data);
        }
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    }
    load();
  }, []);

  if (loading) return <div className="p-8">Loading dashboard...</div>;

  return (
    <div className="p-8 max-w-6xl mx-auto">
      <h1 className="text-2xl font-bold mb-6">Welcome, {user?.fullName}</h1>

      {/* --- Employee view: personal ticket status --- */}
      {isEmployee && (
        <div>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
            <div className="bg-white rounded-lg shadow p-4">
              <p className="text-gray-500 text-sm">My Open Tickets</p>
              <p className="text-3xl font-bold">
                {myTickets.filter(t => t.status !== 'Closed').length}
              </p>
            </div>
            <div className="bg-white rounded-lg shadow p-4">
              <p className="text-gray-500 text-sm">Awaiting My Confirmation</p>
              <p className="text-3xl font-bold text-yellow-600">
                {myTickets.filter(t => t.status === 'Resolved').length}
              </p>
            </div>
            <div className="bg-white rounded-lg shadow p-4">
              <p className="text-gray-500 text-sm">Closed</p>
              <p className="text-3xl font-bold text-green-600">
                {myTickets.filter(t => t.status === 'Closed').length}
              </p>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex justify-between items-center mb-4">
              <h2 className="text-lg font-semibold">Recent Tickets</h2>
              <Link to="/tickets/new" className="text-blue-600 text-sm hover:underline">
                + New Ticket
              </Link>
            </div>
            {myTickets.slice(0, 5).map((t) => (
              <Link
                key={t.id}
                to={`/tickets/${t.id}`}
                className="flex justify-between items-center py-2 border-b last:border-0 hover:bg-gray-50"
              >
                <span>{t.title}</span>
                <Badge text={t.status} variant="status" />
              </Link>
            ))}
          </div>
        </div>
      )}

      {/* --- Agent / Lead / Admin view: global operational summary --- */}
      {!isEmployee && summary && (
        <div>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
            <div className="bg-white rounded-lg shadow p-4">
              <p className="text-gray-500 text-sm">Open Tickets</p>
              <p className="text-3xl font-bold">{summary.totalOpenTickets}</p>
            </div>
            <div className="bg-white rounded-lg shadow p-4">
              <p className="text-gray-500 text-sm">Closed Tickets</p>
              <p className="text-3xl font-bold">{summary.totalClosedTickets}</p>
            </div>
            <div className="bg-white rounded-lg shadow p-4">
              <p className="text-gray-500 text-sm">At Risk</p>
              <p className="text-3xl font-bold text-yellow-600">{summary.atRiskCount}</p>
            </div>
            <div className="bg-white rounded-lg shadow p-4">
              <p className="text-gray-500 text-sm">Breached</p>
              <p className="text-3xl font-bold text-red-600">{summary.breachedCount}</p>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow p-6 mb-6">
            <p className="text-gray-500 text-sm mb-1">SLA Compliance (30-day)</p>
            <p className="text-4xl font-bold text-green-600">{summary.slaComplianceRatePercent}%</p>
          </div>

          {/* Unassigned tickets needing attention — relevant to Agents/Leads */}
          {isAgentOrLead && (
            <div className="bg-white rounded-lg shadow p-6">
              <h2 className="text-lg font-semibold mb-4">Unassigned Tickets</h2>
              {myTickets.filter(t => !t.assignedAgentName && t.status !== 'Closed').length === 0 ? (
                <p className="text-gray-500 text-sm">No unassigned tickets.</p>
              ) : (
                myTickets
                  .filter(t => !t.assignedAgentName && t.status !== 'Closed')
                  .map((t) => (
                    <Link
                      key={t.id}
                      to={`/tickets/${t.id}`}
                      className="flex justify-between items-center py-2 border-b last:border-0 hover:bg-gray-50"
                    >
                      <span>{t.title}</span>
                      <Badge text={t.priority} variant="priority" />
                    </Link>
                  ))
              )}
            </div>
          )}

          {isAdmin && (
            <div className="mt-6 bg-white rounded-lg shadow p-6">
              <h2 className="text-lg font-semibold mb-2">Admin Shortcuts</h2>
              <Link to="/admin" className="text-blue-600 hover:underline text-sm">
                Manage Teams, Categories & SLA Policies →
              </Link>
            </div>
          )}
        </div>
      )}
    </div>
  );
}