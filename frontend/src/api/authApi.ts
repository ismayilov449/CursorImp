import { apiClient } from './client';
import type { AuthResponse, LoginRequest, RegisterRequest } from '../types/auth';

const register = async (payload: RegisterRequest): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>('/api/auth/register', payload);
  return response.data;
};

const login = async (payload: LoginRequest): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>('/api/auth/login', payload);
  return response.data;
};

const refresh = async (refreshToken: string): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>('/api/auth/refresh', {
    refreshToken,
  });
  return response.data;
};

export const authApi = {
  register,
  login,
  refresh,
};
