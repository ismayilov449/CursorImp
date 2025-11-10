export interface AuthUserSummary {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
}

export interface AuthResponse {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
  user: AuthUserSummary;
  permissions: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest extends LoginRequest {
  firstName: string;
  lastName: string;
}
