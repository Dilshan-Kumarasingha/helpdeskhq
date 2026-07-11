import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import NotificationBell from './NotificationBell';

export default function Navbar() {
  const { user, logout } = useAuth();

  if (!user) return null;

  const isEmployee = user.role === 'Employee';
  const isAgent = user.role === 'SupportAgent';
  const isLead = user.role === 'TeamLead';
  const isAdmin = user.role === 'Admin';

  return (
    <nav className="bg-white shadow px-6 py-3 flex justify-between items-center">
      <div className="flex items-center gap-6">
        <Link to="/dashboard" className="text-lg font-bold text-blue-600">
          HelpDeskHQ
        </Link>
        <Link to="/dashboard" className="text-gray-600 hover:text-blue-600">
          Dashboard
        </Link>

        {/* Everyone can see tickets, but what they see inside differs */}
        <Link to="/tickets" className="text-gray-600 hover:text-blue-600">
          {isEmployee ? 'My Tickets' : 'Tickets'}
        </Link>

        {/* Only Employees raise new tickets */}
        {isEmployee && (
          <Link to="/tickets/new" className="text-gray-600 hover:text-blue-600">
            New Ticket
          </Link>
        )}

        {/* Agents and Leads get a queue view */}
        {(isAgent || isLead) && (
          <Link to="/queue" className="text-gray-600 hover:text-blue-600">
            My Queue
          </Link>
        )}

        {/* Leads get a team performance view */}
        {isLead && (
          <Link to="/team" className="text-gray-600 hover:text-blue-600">
            Team Overview
          </Link>
        )}

        {isAdmin && (
          <Link to="/admin" className="text-gray-600 hover:text-blue-600">
            Admin
          </Link>
        )}
      </div>
      <div className="flex items-center gap-4">
        <NotificationBell />
        <span className="text-sm text-gray-500">{user.fullName} ({user.role})</span>
        <button
          onClick={logout}
          className="text-sm bg-red-600 text-white px-3 py-1.5 rounded hover:bg-red-700"
        >
          Logout
        </button>
      </div>
    </nav>
  );
}