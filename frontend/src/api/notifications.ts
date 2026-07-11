import apiClient from './client';
import type { Notification } from '../types';

export async function getNotifications(): Promise<Notification[]> {
  const response = await apiClient.get<Notification[]>('/notifications');
  return response.data;
}

export async function markNotificationAsRead(id: number): Promise<void> {
  await apiClient.patch(`/notifications/${id}/read`);
}