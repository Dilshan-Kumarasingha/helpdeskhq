import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { createTicket } from '../api/tickets';

const CATEGORIES = [
  { id: 1, name: 'Hardware' },
  { id: 2, name: 'Software' },
  { id: 3, name: 'Access Request' },
  { id: 4, name: 'Network' },
  { id: 5, name: 'Facilities' },
];

const PRIORITIES = [
  { value: 0, name: 'Low' },
  { value: 1, name: 'Medium' },
  { value: 2, name: 'High' },
  { value: 3, name: 'Critical' },
];

export default function NewTicket() {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [categoryId, setCategoryId] = useState(1);
  const [priority, setPriority] = useState(1);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const navigate = useNavigate();

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const ticket = await createTicket({
        title,
        description,
        ticketCategoryId: categoryId,
        priority,
      });
      navigate(`/tickets/${ticket.id}`);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create ticket.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="p-8 max-w-2xl mx-auto">
      <h1 className="text-2xl font-bold mb-6">Create New Ticket</h1>

      {error && (
        <div className="bg-red-100 text-red-700 p-3 rounded mb-4 text-sm">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit} className="bg-white p-6 rounded-lg shadow space-y-4">
        <div>
          <label className="block text-sm font-medium mb-1">Title</label>
          <input
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            required
            className="w-full border rounded px-3 py-2"
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Description</label>
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            required
            rows={4}
            className="w-full border rounded px-3 py-2"
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Category</label>
          <select
            value={categoryId}
            onChange={(e) => setCategoryId(Number(e.target.value))}
            className="w-full border rounded px-3 py-2"
          >
            {CATEGORIES.map((c) => (
              <option key={c.id} value={c.id}>{c.name}</option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Priority</label>
          <select
            value={priority}
            onChange={(e) => setPriority(Number(e.target.value))}
            className="w-full border rounded px-3 py-2"
          >
            {PRIORITIES.map((p) => (
              <option key={p.value} value={p.value}>{p.name}</option>
            ))}
          </select>
        </div>

        <button
          type="submit"
          disabled={loading}
          className="w-full bg-blue-600 text-white py-2 rounded hover:bg-blue-700 disabled:opacity-50"
        >
          {loading ? 'Creating...' : 'Create Ticket'}
        </button>
      </form>
    </div>
  );
}