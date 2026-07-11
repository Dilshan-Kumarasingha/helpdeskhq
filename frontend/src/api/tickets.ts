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