import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import {
  getTicketById, getComments, addComment,
  assignTicket, changeStatus, resolveTicket, confirmResolution, reopenTicket,
} from '../api/tickets';
import type { Ticket, Comment } from '../types';
import { TicketStatusEnum } from '../types';
import { useAuth } from '../context/AuthContext';
import { useTicketHub } from '../hooks/useTicketHub';
import Badge from '../components/Badge';

export default function TicketDetail() {
  const { id } = useParams<{ id: string }>();
  const ticketId = Number(id);
  const { user } = useAuth();

  const [ticket, setTicket] = useState<Ticket | null>(null);
  const [comments, setComments] = useState<Comment[]>([]);
  const [newComment, setNewComment] = useState('');
  const [resolutionNotes, setResolutionNotes] = useState('');
  const [loading, setLoading] = useState(true);
  const [posting, setPosting] = useState(false);
  const [actionError, setActionError] = useState('');

  useEffect(() => {
    loadData();
  }, [ticketId]);

  useTicketHub(ticketId, (payload) => {
    if (payload?.type === 'NewComment') {
      setComments((prev) => [...prev, payload.comment]);
    } else {
      setTicket(payload);
    }
  });

  async function loadData() {
    setLoading(true);
    try {
      const [ticketData, commentsData] = await Promise.all([
        getTicketById(ticketId),
        getComments(ticketId),
      ]);
      setTicket(ticketData);
      setComments(commentsData);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  }

  async function handleAddComment(e: React.FormEvent) {
    e.preventDefault();
    if (!newComment.trim()) return;

    setPosting(true);
    try {
      const comment = await addComment(ticketId, newComment);
      setComments([...comments, comment]);
      setNewComment('');
    } catch (err) {
      console.error(err);
    } finally {
      setPosting(false);
    }
  }

  async function handleAction(action: () => Promise<any>) {
    setActionError('');
    try {
      const updated = await action();
      setTicket(updated);
    } catch (err: any) {
      setActionError(err.response?.data?.message || 'Action failed.');
    }
  }

  if (loading) return <div className="p-8">Loading...</div>;
  if (!ticket) return <div className="p-8 text-red-600">Ticket not found.</div>;

  const isEmployee = user?.role === 'Employee';
  const isAgentOrLead = user?.role === 'SupportAgent' || user?.role === 'TeamLead';
  const isAdmin = user?.role === 'Admin';
  const canActLikeAgent = isAgentOrLead || isAdmin;

  return (
    <div className="p-8 max-w-4xl mx-auto">
      <div className="bg-white rounded-lg shadow p-6 mb-6">
        <div className="flex justify-between items-start mb-4">
          <div>
            <h1 className="text-2xl font-bold">{ticket.title}</h1>
            <p className="text-gray-500 text-sm">{ticket.ticketNumber}</p>
          </div>
          <div className="flex gap-2">
            <Badge text={ticket.status} variant="status" />
            <Badge text={ticket.priority} variant="priority" />
            <Badge text={ticket.slaBreachStatus} variant="sla" />
          </div>
        </div>

        <p className="text-gray-700 mb-4">{ticket.description}</p>

        <div className="grid grid-cols-2 gap-4 text-sm text-gray-600 border-t pt-4 mb-4">
          <div><strong>Raised by:</strong> {ticket.raisedByName}</div>
          <div><strong>Assigned to:</strong> {ticket.assignedAgentName || 'Unassigned'}</div>
          <div><strong>Category:</strong> {ticket.category}</div>
          <div><strong>Team:</strong> {ticket.teamName}</div>
          <div><strong>Created:</strong> {new Date(ticket.createdAt).toLocaleString()}</div>
          <div><strong>Resolution Due:</strong> {new Date(ticket.resolutionDueAt).toLocaleString()}</div>
        </div>

        {actionError && (
          <div className="bg-red-100 text-red-700 p-3 rounded mb-4 text-sm">{actionError}</div>
        )}

        {/* --- Agent / Lead / Admin actions --- */}
        {canActLikeAgent && (
          <div className="border-t pt-4 space-y-3">
            <h3 className="text-sm font-semibold text-gray-700">Agent Actions</h3>
            <div className="flex flex-wrap gap-2">
              {!ticket.assignedAgentName && (
                <button
                  onClick={() => handleAction(() => assignTicket(ticketId, user!.userId))}
                  className="bg-blue-600 text-white px-3 py-1.5 rounded text-sm hover:bg-blue-700"
                >
                  Assign to Me
                </button>
              )}

              {ticket.status === 'Assigned' && (
                <button
                  onClick={() => handleAction(() => changeStatus(ticketId, TicketStatusEnum.InProgress))}
                  className="bg-yellow-600 text-white px-3 py-1.5 rounded text-sm hover:bg-yellow-700"
                >
                  Start Work (In Progress)
                </button>
              )}

              {ticket.status === 'InProgress' && (
                <button
                  onClick={() => handleAction(() => changeStatus(ticketId, TicketStatusEnum.OnHold))}
                  className="bg-orange-600 text-white px-3 py-1.5 rounded text-sm hover:bg-orange-700"
                >
                  Put On Hold
                </button>
              )}

              {ticket.status === 'OnHold' && (
                <button
                  onClick={() => handleAction(() => changeStatus(ticketId, TicketStatusEnum.InProgress))}
                  className="bg-yellow-600 text-white px-3 py-1.5 rounded text-sm hover:bg-yellow-700"
                >
                  Resume Work
                </button>
              )}

              {ticket.status === 'Escalated' && (
                <button
                  onClick={() => handleAction(() => changeStatus(ticketId, TicketStatusEnum.InProgress))}
                  className="bg-yellow-600 text-white px-3 py-1.5 rounded text-sm hover:bg-yellow-700"
                >
                  Acknowledge & Work On It
                </button>
              )}
            </div>

            {ticket.status === 'InProgress' && (
              <div className="flex gap-2 items-start">
                <input
                  type="text"
                  value={resolutionNotes}
                  onChange={(e) => setResolutionNotes(e.target.value)}
                  placeholder="Resolution notes..."
                  className="flex-1 border rounded px-3 py-2 text-sm"
                />
                <button
                  onClick={() => {
                    if (!resolutionNotes.trim()) return;
                    handleAction(() => resolveTicket(ticketId, resolutionNotes));
                    setResolutionNotes('');
                  }}
                  className="bg-green-600 text-white px-3 py-2 rounded text-sm hover:bg-green-700"
                >
                  Mark Resolved
                </button>
              </div>
            )}
          </div>
        )}

        {/* --- Employee actions --- */}
        {isEmployee && ticket.status === 'Resolved' && (
          <div className="border-t pt-4">
            <h3 className="text-sm font-semibold text-gray-700 mb-2">Your Action Needed</h3>
            <p className="text-sm text-gray-600 mb-3">
              This ticket has been marked resolved. Does this fix your issue?
            </p>
            <div className="flex gap-2">
              <button
                onClick={() => handleAction(() => confirmResolution(ticketId))}
                className="bg-green-600 text-white px-4 py-2 rounded text-sm hover:bg-green-700"
              >
                ✅ Yes, Confirm & Close
              </button>
              <button
                onClick={() => handleAction(() => reopenTicket(ticketId))}
                className="bg-red-600 text-white px-4 py-2 rounded text-sm hover:bg-red-700"
              >
                ❌ No, Reopen It
              </button>
            </div>
          </div>
        )}
      </div>

      <div className="bg-white rounded-lg shadow p-6">
        <h2 className="text-lg font-semibold mb-4">Comments</h2>

        <div className="space-y-3 mb-4">
          {comments.length === 0 ? (
            <p className="text-gray-500 text-sm">No comments yet.</p>
          ) : (
            comments.map((comment) => (
              <div key={comment.id} className="border rounded p-3">
                <div className="flex justify-between text-sm text-gray-500 mb-1">
                  <span className="font-medium text-gray-800">{comment.authorName}</span>
                  <span>{new Date(comment.createdAt).toLocaleString()}</span>
                </div>
                <p className="text-gray-700">{comment.content}</p>
              </div>
            ))
          )}
        </div>

        <form onSubmit={handleAddComment} className="flex gap-2">
          <input
            type="text"
            value={newComment}
            onChange={(e) => setNewComment(e.target.value)}
            placeholder="Add a comment..."
            className="flex-1 border rounded px-3 py-2"
          />
          <button
            type="submit"
            disabled={posting}
            className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 disabled:opacity-50"
          >
            {posting ? 'Posting...' : 'Post'}
          </button>
        </form>
      </div>
    </div>
  );
}