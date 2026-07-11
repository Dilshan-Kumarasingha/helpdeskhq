import { useEffect, useState } from 'react';
import {
  getTeams, createTeam, deleteTeam,
  getCategories, createCategory, deleteCategory,
  getSlaPolicies, createSlaPolicy, deleteSlaPolicy,
  type Team, type Category, type SlaPolicy,
} from '../api/admin';

const PRIORITIES = [
  { value: 0, name: 'Low' },
  { value: 1, name: 'Medium' },
  { value: 2, name: 'High' },
  { value: 3, name: 'Critical' },
];

export default function Admin() {
  const [teams, setTeams] = useState<Team[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [policies, setPolicies] = useState<SlaPolicy[]>([]);
  const [error, setError] = useState('');

  const [newTeamName, setNewTeamName] = useState('');
  const [newCategoryName, setNewCategoryName] = useState('');
  const [newCategoryTeamId, setNewCategoryTeamId] = useState<number | null>(null);
  const [newPolicyCategoryId, setNewPolicyCategoryId] = useState<number | null>(null);
  const [newPolicyPriority, setNewPolicyPriority] = useState(0);
  const [newPolicyResponse, setNewPolicyResponse] = useState(60);
  const [newPolicyResolution, setNewPolicyResolution] = useState(480);

  useEffect(() => {
    loadAll();
  }, []);

  async function loadAll() {
    try {
      const [t, c, p] = await Promise.all([getTeams(), getCategories(), getSlaPolicies()]);
      setTeams(t);
      setCategories(c);
      setPolicies(p);
      if (t.length > 0) setNewCategoryTeamId(t[0].id);
      if (c.length > 0) setNewPolicyCategoryId(c[0].id);
    } catch (err: any) {
      setError('Failed to load admin data. You may not have Admin access.');
    }
  }

  async function handleCreateTeam(e: React.FormEvent) {
    e.preventDefault();
    try {
      await createTeam(newTeamName);
      setNewTeamName('');
      loadAll();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create team.');
    }
  }

  async function handleDeleteTeam(id: number) {
    try {
      await deleteTeam(id);
      loadAll();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete team.');
    }
  }

  async function handleCreateCategory(e: React.FormEvent) {
    e.preventDefault();
    if (!newCategoryTeamId) return;
    try {
      await createCategory(newCategoryName, newCategoryTeamId);
      setNewCategoryName('');
      loadAll();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create category.');
    }
  }

  async function handleDeleteCategory(id: number) {
    try {
      await deleteCategory(id);
      loadAll();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete category.');
    }
  }

  async function handleCreatePolicy(e: React.FormEvent) {
    e.preventDefault();
    if (!newPolicyCategoryId) return;
    try {
      await createSlaPolicy({
        ticketCategoryId: newPolicyCategoryId,
        priority: newPolicyPriority,
        responseTargetMinutes: newPolicyResponse,
        resolutionTargetMinutes: newPolicyResolution,
      });
      loadAll();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create SLA policy.');
    }
  }

  async function handleDeletePolicy(id: number) {
    try {
      await deleteSlaPolicy(id);
      loadAll();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete policy.');
    }
  }

  return (
    <div className="p-8 max-w-6xl mx-auto space-y-8">
      <h1 className="text-2xl font-bold">Admin Settings</h1>

      {error && (
        <div className="bg-red-100 text-red-700 p-3 rounded text-sm">{error}</div>
      )}

      {/* Teams */}
      <section className="bg-white rounded-lg shadow p-6">
        <h2 className="text-lg font-semibold mb-4">Teams</h2>
        <form onSubmit={handleCreateTeam} className="flex gap-2 mb-4">
          <input
            type="text"
            value={newTeamName}
            onChange={(e) => setNewTeamName(e.target.value)}
            placeholder="New team name"
            required
            className="border rounded px-3 py-2 flex-1"
          />
          <button type="submit" className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700">
            Add
          </button>
        </form>
        <ul className="divide-y">
          {teams.map((t) => (
            <li key={t.id} className="flex justify-between items-center py-2">
              <span>{t.name} <span className="text-gray-400 text-sm">({t.memberCount} members, {t.categoryCount} categories)</span></span>
              <button onClick={() => handleDeleteTeam(t.id)} className="text-red-600 text-sm hover:underline">
                Delete
              </button>
            </li>
          ))}
        </ul>
      </section>

      {/* Categories */}
      <section className="bg-white rounded-lg shadow p-6">
        <h2 className="text-lg font-semibold mb-4">Categories</h2>
        <form onSubmit={handleCreateCategory} className="flex gap-2 mb-4">
          <input
            type="text"
            value={newCategoryName}
            onChange={(e) => setNewCategoryName(e.target.value)}
            placeholder="New category name"
            required
            className="border rounded px-3 py-2 flex-1"
          />
          <select
            value={newCategoryTeamId ?? ''}
            onChange={(e) => setNewCategoryTeamId(Number(e.target.value))}
            className="border rounded px-3 py-2"
          >
            {teams.map((t) => (
              <option key={t.id} value={t.id}>{t.name}</option>
            ))}
          </select>
          <button type="submit" className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700">
            Add
          </button>
        </form>
        <ul className="divide-y">
          {categories.map((c) => (
            <li key={c.id} className="flex justify-between items-center py-2">
              <span>{c.name} <span className="text-gray-400 text-sm">({c.teamName})</span></span>
              <button onClick={() => handleDeleteCategory(c.id)} className="text-red-600 text-sm hover:underline">
                Delete
              </button>
            </li>
          ))}
        </ul>
      </section>

      {/* SLA Policies */}
      <section className="bg-white rounded-lg shadow p-6">
        <h2 className="text-lg font-semibold mb-4">SLA Policies</h2>
        <form onSubmit={handleCreatePolicy} className="grid grid-cols-2 md:grid-cols-5 gap-2 mb-4">
          <select
            value={newPolicyCategoryId ?? ''}
            onChange={(e) => setNewPolicyCategoryId(Number(e.target.value))}
            className="border rounded px-3 py-2"
          >
            {categories.map((c) => (
              <option key={c.id} value={c.id}>{c.name}</option>
            ))}
          </select>
          <select
            value={newPolicyPriority}
            onChange={(e) => setNewPolicyPriority(Number(e.target.value))}
            className="border rounded px-3 py-2"
          >
            {PRIORITIES.map((p) => (
              <option key={p.value} value={p.value}>{p.name}</option>
            ))}
          </select>
          <input
            type="number"
            value={newPolicyResponse}
            onChange={(e) => setNewPolicyResponse(Number(e.target.value))}
            placeholder="Response (min)"
            className="border rounded px-3 py-2"
          />
          <input
            type="number"
            value={newPolicyResolution}
            onChange={(e) => setNewPolicyResolution(Number(e.target.value))}
            placeholder="Resolution (min)"
            className="border rounded px-3 py-2"
          />
          <button type="submit" className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700">
            Add
          </button>
        </form>
        <table className="w-full text-left text-sm">
          <thead className="border-b text-gray-500">
            <tr>
              <th className="py-2">Category</th>
              <th className="py-2">Priority</th>
              <th className="py-2">Response</th>
              <th className="py-2">Resolution</th>
              <th className="py-2"></th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {policies.map((p) => (
              <tr key={p.id}>
                <td className="py-2">{p.categoryName}</td>
                <td className="py-2">{p.priority}</td>
                <td className="py-2">{p.responseTargetMinutes} min</td>
                <td className="py-2">{p.resolutionTargetMinutes} min</td>
                <td className="py-2 text-right">
                  <button onClick={() => handleDeletePolicy(p.id)} className="text-red-600 hover:underline">
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </div>
  );
}