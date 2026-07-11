import apiClient from './client';
import type { Ticket, Comment } from '../types';

export interface CreateTicketRequest {
  title: string;
  description: string;
  ticketCategoryId: number;
  priority: number;
}

export async function getTickets(): Promise<Ticket[]> {
  const response = await apiClient.get<Ticket[]>('/tickets');
  return response.data;
}

export async function getTicketById(id: number): Promise<Ticket> {
  const response = await apiClient.get<Ticket>(`/tickets/${id}`);
  return response.data;
}

export async function createTicket(data: CreateTicketRequest): Promise<Ticket> {
  const response = await apiClient.post<Ticket>('/tickets', data);
  return response.data;
}

export async function getComments(ticketId: number): Promise<Comment[]> {
  const response = await apiClient.get<Comment[]>(`/tickets/${ticketId}/comments`);
  return response.data;
}

export async function addComment(ticketId: number, content: string): Promise<Comment> {
  const response = await apiClient.post<Comment>(`/tickets/${ticketId}/comments`, { content });
  return response.data;
}

export async function assignTicket(ticketId: number, agentUserId: number): Promise<Ticket> {
  const response = await apiClient.patch<Ticket>(`/tickets/${ticketId}/assign`, { agentUserId });
  return response.data;
}

export async function changeStatus(ticketId: number, newStatus: number, note?: string): Promise<Ticket> {
  const response = await apiClient.patch<Ticket>(`/tickets/${ticketId}/status`, { newStatus, note });
  return response.data;
}

export async function resolveTicket(ticketId: number, resolutionNotes: string): Promise<Ticket> {
  const response = await apiClient.patch<Ticket>(`/tickets/${ticketId}/resolve`, { resolutionNotes });
  return response.data;
}

export async function confirmResolution(ticketId: number): Promise<Ticket> {
  const response = await apiClient.patch<Ticket>(`/tickets/${ticketId}/confirm`);
  return response.data;
}

export async function reopenTicket(ticketId: number): Promise<Ticket> {
  const response = await apiClient.patch<Ticket>(`/tickets/${ticketId}/reopen`);
  return response.data;
}