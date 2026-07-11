import apiClient from './client';

export interface Team {
  id: number;
  name: string;
  memberCount: number;
  categoryCount: number;
}

export interface Category {
  id: number;
  name: string;
  teamId: number;
  teamName: string;
}

export interface SlaPolicy {
  id: number;
  ticketCategoryId: number;
  categoryName: string;
  priority: string;
  responseTargetMinutes: number;
  resolutionTargetMinutes: number;
}

export async function getTeams(): Promise<Team[]> {
  const res = await apiClient.get<Team[]>('/admin/teams');
  return res.data;
}

export async function createTeam(name: string): Promise<Team> {
  const res = await apiClient.post<Team>('/admin/teams', { name });
  return res.data;
}

export async function deleteTeam(id: number): Promise<void> {
  await apiClient.delete(`/admin/teams/${id}`);
}

export async function getCategories(): Promise<Category[]> {
  const res = await apiClient.get<Category[]>('/admin/categories');
  return res.data;
}

export async function createCategory(name: string, teamId: number): Promise<Category> {
  const res = await apiClient.post<Category>('/admin/categories', { name, teamId });
  return res.data;
}

export async function deleteCategory(id: number): Promise<void> {
  await apiClient.delete(`/admin/categories/${id}`);
}

export async function getSlaPolicies(): Promise<SlaPolicy[]> {
  const res = await apiClient.get<SlaPolicy[]>('/admin/sla-policies');
  return res.data;
}

export async function createSlaPolicy(data: {
  ticketCategoryId: number;
  priority: number;
  responseTargetMinutes: number;
  resolutionTargetMinutes: number;
}): Promise<SlaPolicy> {
  const res = await apiClient.post<SlaPolicy>('/admin/sla-policies', data);
  return res.data;
}

export async function deleteSlaPolicy(id: number): Promise<void> {
  await apiClient.delete(`/admin/sla-policies/${id}`);
}