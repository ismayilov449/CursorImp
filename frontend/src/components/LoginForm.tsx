import { useState } from 'react';
import type { FormEvent } from 'react';
import { isAxiosError } from 'axios';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import type { LoginRequest } from '../types/auth';

interface LocationState {
  from?: Location;
}

export const LoginForm = () => {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const state = location.state as LocationState | undefined;

  const [form, setForm] = useState<LoginRequest>({
    email: '',
    password: '',
  });
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      await login(form);
      const redirectTo = state?.from?.pathname ?? '/';
      navigate(redirectTo, { replace: true });
    } catch (err) {
      if (isAxiosError(err)) {
        const detail =
          (err.response?.data as { detail?: string } | undefined)?.detail ??
          err.response?.statusText;
        setError(detail ?? 'Unable to login. Please try again.');
      } else {
        setError('Unable to login. Please try again.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form className="card form-card" onSubmit={handleSubmit}>
      <h2>Sign In</h2>
      <label htmlFor="email">
        Email
        <input
          id="email"
          type="email"
          value={form.email}
          onChange={(event) => setForm((prev) => ({ ...prev, email: event.target.value }))}
          required
          autoComplete="email"
        />
      </label>
      <label htmlFor="password">
        Password
        <input
          id="password"
          type="password"
          value={form.password}
          onChange={(event) =>
            setForm((prev) => ({ ...prev, password: event.target.value }))
          }
          required
          autoComplete="current-password"
        />
      </label>
      {error && <p className="form-error">{error}</p>}
      <button type="submit" className="primary-button" disabled={isSubmitting}>
        {isSubmitting ? 'Signing in...' : 'Sign In'}
      </button>
    </form>
  );
};
