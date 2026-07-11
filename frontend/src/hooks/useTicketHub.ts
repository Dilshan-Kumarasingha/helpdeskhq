import { useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';

export function useTicketHub(ticketId: number | null, onUpdate: (payload: any) => void) {
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    if (!ticketId) return;

    const token = localStorage.getItem('token');

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(import.meta.env.VITE_SIGNALR_URL, {
        accessTokenFactory: () => token || '',
      })
      .withAutomaticReconnect()
      .build();

    connection.on('TicketUpdated', (payload) => {
      onUpdate(payload);
    });

    connection
      .start()
      .then(() => {
        connection.invoke('JoinTicketGroup', ticketId);
      })
      .catch((err) => console.error('SignalR connection error:', err));

    connectionRef.current = connection;

    return () => {
      if (connectionRef.current) {
        connectionRef.current.invoke('LeaveTicketGroup', ticketId).catch(() => {});
        connectionRef.current.stop();
      }
    };
  }, [ticketId]);
}