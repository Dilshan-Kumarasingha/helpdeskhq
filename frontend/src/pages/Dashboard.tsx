import { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { getDashboardSummary } from '../api/dashboard';
import type { DashboardSummary } from '../types';

export default function Dashboard() {
  const { user } = useAuth();
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function load() {
      try {
        const data = await getDashboardSummary();
        setSummary(data);
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

      {summary && (
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
      )}

      {summary && (
        <div className="bg-white rounded-lg shadow p-6">
          <p className="text-gray-500 text-sm mb-1">SLA Compliance (30-day)</p>
          <p className="text-4xl font-bold text-green-600">{summary.slaComplianceRatePercent}%</p>
        </div>
      )}
    </div>
  );
}