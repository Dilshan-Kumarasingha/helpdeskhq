import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

import NotificationBell from './NotificationBell';

export default function Navbar() {
  const { user, logout } = useAuth();

  if (!user) return null;

  return (
    <nav className="bg-white shadow px-6 py-3 flex justify-between items-center">
      <div className="flex items-center gap-6">
        <Link to="/dashboard" className="text-lg font-bold text-blue-600">
          HelpDeskHQ
        </Link>
        <Link to="/dashboard" className="text-gray-600 hover:text-blue-600">
          Dashboard
        </Link>
        <Link to="/tickets" className="text-gray-600 hover:text-blue-600">
          Tickets
        </Link>

        {user.role === 'Admin' && (
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