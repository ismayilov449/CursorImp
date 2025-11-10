import axios, {
  type AxiosError,
  type AxiosInstance,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from 'axios';
import { authStore } from '../store/authStore';
import type { AuthResponse } from '../types/auth';

interface AuthenticatedRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

const baseURL =
  (import.meta.env.VITE_API_BASE_URL as string | undefined) ??
  'https://localhost:5001';

const REFRESH_ENDPOINT = '/api/auth/refresh';

const refreshHttpClient = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json',
  },
});

let refreshPromise: Promise<string | null> | null = null;

const refreshAccessToken = async (): Promise<string | null> => {
  if (refreshPromise) {
    return refreshPromise;
  }

  const { refreshToken } = authStore.getState();
  if (!refreshToken) {
    return null;
  }

  refreshPromise = (async () => {
    try {
      const response = await refreshHttpClient.post<AuthResponse>(REFRESH_ENDPOINT, {
        refreshToken,
      });
      authStore.setFromAuthResponse(response.data);
      return response.data.accessToken;
    } catch (error) {
      authStore.clear();
      throw error;
    } finally {
      refreshPromise = null;
    }
  })();

  return refreshPromise;
};

const shouldSkipRefresh = (url?: string): boolean => {
  if (!url) {
    return false;
  }

  return (
    url.includes('/auth/login') ||
    url.includes('/auth/register') ||
    url.includes('/auth/refresh')
  );
};

const apiClient: AxiosInstance = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use((config) => {
  const state = authStore.getState();
  if (state.accessToken) {
    config.headers = config.headers ?? {};
    config.headers.Authorization = `Bearer ${state.accessToken}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response: AxiosResponse) => response,
  async (error: AxiosError) => {
    const responseStatus = error.response?.status;
    const originalRequest = error.config as AuthenticatedRequestConfig | undefined;

    if (
      responseStatus === 401 &&
      originalRequest &&
      !originalRequest._retry &&
      !shouldSkipRefresh(originalRequest.url)
    ) {
      originalRequest._retry = true;
      try {
        const newAccessToken = await refreshAccessToken();
        if (!newAccessToken) {
          return Promise.reject(error);
        }
        originalRequest.headers = originalRequest.headers ?? {};
        originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
        return apiClient(originalRequest);
      } catch (refreshError) {
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  },
);

export { apiClient, baseURL };
