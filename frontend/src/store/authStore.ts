import { useSyncExternalStore } from 'react';
import type { AuthResponse, AuthUserSummary } from '../types/auth';

const STORAGE_KEY = 'authapp:auth-state';

export interface AuthState {
  user: AuthUserSummary | null;
  permissions: string[];
  accessToken: string | null;
  accessTokenExpiresAtUtc: string | null;
  refreshToken: string | null;
  refreshTokenExpiresAtUtc: string | null;
}

type Listener = () => void;

const listeners = new Set<Listener>();

const loadState = (): AuthState => {
  if (typeof window === 'undefined') {
    return getDefaultState();
  }

  const serialized = window.localStorage.getItem(STORAGE_KEY);
  if (!serialized) {
    return getDefaultState();
  }

  try {
    const parsed = JSON.parse(serialized) as AuthState;
    return {
      ...getDefaultState(),
      ...parsed,
    };
  } catch {
    return getDefaultState();
  }
};

const getDefaultState = (): AuthState => ({
  user: null,
  permissions: [],
  accessToken: null,
  accessTokenExpiresAtUtc: null,
  refreshToken: null,
  refreshTokenExpiresAtUtc: null,
});

let state: AuthState = loadState();

const persistState = (nextState: AuthState) => {
  if (typeof window === 'undefined') {
    return;
  }

  try {
    window.localStorage.setItem(STORAGE_KEY, JSON.stringify(nextState));
  } catch (error) {
    console.warn('Failed to persist auth state', error);
  }
};

const emit = () => {
  for (const listener of listeners) {
    listener();
  }
};

export const authStore = {
  getState: (): AuthState => state,
  subscribe: (listener: Listener) => {
    listeners.add(listener);
    return () => {
      listeners.delete(listener);
    };
  },
  setFromAuthResponse: (response: AuthResponse) => {
    state = {
      user: response.user,
      permissions: response.permissions,
      accessToken: response.accessToken,
      accessTokenExpiresAtUtc: response.accessTokenExpiresAtUtc,
      refreshToken: response.refreshToken,
      refreshTokenExpiresAtUtc: response.refreshTokenExpiresAtUtc,
    };
    persistState(state);
    emit();
  },
  updateTokens: (response: Pick<AuthResponse, 'accessToken' | 'accessTokenExpiresAtUtc' | 'refreshToken' | 'refreshTokenExpiresAtUtc'>) => {
    state = {
      ...state,
      accessToken: response.accessToken,
      accessTokenExpiresAtUtc: response.accessTokenExpiresAtUtc,
      refreshToken: response.refreshToken,
      refreshTokenExpiresAtUtc: response.refreshTokenExpiresAtUtc,
    };
    persistState(state);
    emit();
  },
  clear: () => {
    state = getDefaultState();
    persistState(state);
    emit();
  },
};

export const useAuthStore = (): AuthState =>
  useSyncExternalStore(authStore.subscribe, authStore.getState, authStore.getState);
