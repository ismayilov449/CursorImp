import { apiClient } from './client';
import type { PagedResult } from '../types/common';
import type { UserDto } from '../types/user';

const getAll = async (): Promise<UserDto[]> => {
  const response = await apiClient.get<UserDto[]>('/api/auth');
  return response.data;
};

const getPaged = async (pageNumber: number, pageSize: number): Promise<PagedResult<UserDto>> => {
  const response = await apiClient.get<PagedResult<UserDto>>('/api/auth/paged', {
    params: {
      pageNumber,
      pageSize,
    },
  });
  return response.data;
};

export const userApi = {
  getAll,
  getPaged,
};
