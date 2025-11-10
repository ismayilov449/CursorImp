import { apiClient } from './client';
import type { PermissionDto, UpdateUserPermissionsRequest } from '../types/permissions';

const getAll = async (): Promise<PermissionDto[]> => {
  const response = await apiClient.get<PermissionDto[]>('/api/permissions');
  return response.data;
};

const create = async (payload: { key: string; name: string; description?: string }): Promise<PermissionDto> => {
  const response = await apiClient.post<PermissionDto>('/api/permissions', payload);
  return response.data;
};

const getUserPermissions = async (userId: string): Promise<string[]> => {
  const response = await apiClient.get<string[]>(`/api/users/${userId}/permissions`);
  return response.data;
};

const assignPermissions = async (userId: string, payload: UpdateUserPermissionsRequest): Promise<string[]> => {
  const response = await apiClient.post<string[]>(`/api/users/${userId}/permissions`, payload);
  return response.data;
};

const removePermissions = async (userId: string, payload: UpdateUserPermissionsRequest): Promise<string[]> => {
  const response = await apiClient.delete<string[]>(`/api/users/${userId}/permissions`, {
    data: payload,
  });
  return response.data;
};

export const permissionApi = {
  getAll,
  create,
  getUserPermissions,
  assignPermissions,
  removePermissions,
};
