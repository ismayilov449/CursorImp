import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  type ReactNode,
} from 'react';
import { authStore, useAuthStore } from '../store/authStore';
import type { AuthResponse, LoginRequest, RegisterRequest } from '../types/auth';
import { authApi } from '../api/authApi';

interface AuthContextValue {
  isAuthenticated: boolean;
  user: AuthResponse['user'] | null;
  permissions: string[];
  login: (payload: LoginRequest) => Promise<void>;
  register: (payload: RegisterRequest) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const state = useAuthStore();

  const login = useCallback(async (payload: LoginRequest) => {
    const response = await authApi.login(payload);
    authStore.setFromAuthResponse(response);
  }, []);

  const register = useCallback(async (payload: RegisterRequest) => {
    const response = await authApi.register(payload);
    authStore.setFromAuthResponse(response);
  }, []);

  const logout = useCallback(() => {
    authStore.clear();
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      isAuthenticated: Boolean(state.accessToken),
      user: state.user,
      permissions: state.permissions,
      login,
      register,
      logout,
    }),
    [state.accessToken, state.permissions, state.user, login, register, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = (): AuthContextValue => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
