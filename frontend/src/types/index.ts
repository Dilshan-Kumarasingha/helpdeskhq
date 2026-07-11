export interface AuthResponse {
  userId: number;
  fullName: string;
  email: string;
  role: string;
  token: string;
}

export interface Ticket {
  id: number;
  ticketNumber: string;
  title: string;
  description: string;
  category: string;
  priority: string;
  status: string;
  raisedByName: string;
  assignedAgentName: string | null;
  teamName: string;
  createdAt: string;
  firstResponseDueAt: string;
  resolutionDueAt: string;
  firstRespondedAt: string | null;
  resolvedAt: string | null;
  slaBreachStatus: string;
  escalationLevel: number;
}

export interface Comment {
  id: number;
  content: string;
  authorName: string;
  createdAt: string;
}

export interface Notification {
  id: number;
  ticketId: number | null;
  message: string;
  isRead: boolean;
  createdAt: string;
}

export interface DashboardSummary {
  totalOpenTickets: number;
  totalClosedTickets: number;
  ticketsByStatus: Record<string, number>;
  ticketsByPriority: Record<string, number>;
  ticketsByTeam: Record<string, number>;
  atRiskCount: number;
  breachedCount: number;
  slaComplianceRatePercent: number;
}