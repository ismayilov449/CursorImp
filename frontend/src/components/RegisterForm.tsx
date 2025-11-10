import { useState } from 'react';
import type { FormEvent } from 'react';
import { isAxiosError } from 'axios';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import type { RegisterRequest } from '../types/auth';

export const RegisterForm = () => {
  const { register } = useAuth();
  const navigate = useNavigate();

  const [form, setForm] = useState<RegisterRequest>({
    email: '',
    password: '',
    firstName: '',
    lastName: '',
  });
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      await register(form);
      navigate('/', { replace: true });
    } catch (err) {
      if (isAxiosError(err)) {
        const detail =
          (err.response?.data as { detail?: string } | undefined)?.detail ??
          err.response?.statusText;
        setError(detail ?? 'Unable to register. Please try again.');
      } else {
        setError('Unable to register. Please try again.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form className="card form-card" onSubmit={handleSubmit}>
      <h2>Create Account</h2>
      <label htmlFor="firstName">
        First Name
        <input
          id="firstName"
          value={form.firstName}
          onChange={(event) =>
            setForm((prev) => ({ ...prev, firstName: event.target.value }))
          }
          required
          autoComplete="given-name"
        />
      </label>
      <label htmlFor="lastName">
        Last Name
        <input
          id="lastName"
          value={form.lastName}
          onChange={(event) => setForm((prev) => ({ ...prev, lastName: event.target.value }))}
          required
          autoComplete="family-name"
        />
      </label>
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
          autoComplete="new-password"
        />
      </label>
      {error && <p className="form-error">{error}</p>}
      <button type="submit" className="primary-button" disabled={isSubmitting}>
        {isSubmitting ? 'Creating...' : 'Create Account'}
      </button>
    </form>
  );
};
