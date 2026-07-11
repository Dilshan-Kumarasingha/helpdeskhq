interface BadgeProps {
  text: string;
  variant: 'status' | 'priority' | 'sla';
}

const statusColors: Record<string, string> = {
  New: 'bg-gray-200 text-gray-800',
  Assigned: 'bg-blue-200 text-blue-800',
  InProgress: 'bg-yellow-200 text-yellow-800',
  OnHold: 'bg-orange-200 text-orange-800',
  Resolved: 'bg-green-200 text-green-800',
  Closed: 'bg-gray-300 text-gray-700',
  Reopened: 'bg-purple-200 text-purple-800',
  Escalated: 'bg-red-200 text-red-800',
};

const priorityColors: Record<string, string> = {
  Low: 'bg-gray-200 text-gray-800',
  Medium: 'bg-blue-200 text-blue-800',
  High: 'bg-orange-200 text-orange-800',
  Critical: 'bg-red-200 text-red-800',
};

const slaColors: Record<string, string> = {
  OnTrack: 'bg-green-200 text-green-800',
  AtRisk: 'bg-yellow-200 text-yellow-800',
  Breached: 'bg-red-200 text-red-800',
};

export default function Badge({ text, variant }: BadgeProps) {
  const colorMap = variant === 'status' ? statusColors : variant === 'priority' ? priorityColors : slaColors;
  const colorClass = colorMap[text] || 'bg-gray-200 text-gray-800';

  return (
    <span className={`px-2 py-1 rounded text-xs font-medium ${colorClass}`}>
      {text}
    </span>
  );
}