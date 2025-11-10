import { useState } from 'react';
import type { FormEvent } from 'react';
import type { PermissionDto } from '../types/permissions';

interface PermissionListProps {
  permissions: PermissionDto[];
  onCreate: (payload: { key: string; name: string; description?: string }) => Promise<void>;
  canCreate: boolean;
}

export const PermissionList = ({ permissions, onCreate, canCreate }: PermissionListProps) => {
  const [form, setForm] = useState({
    key: '',
    name: '',
    description: '',
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      const payload = {
        key: form.key.trim().toLowerCase(),
        name: form.name.trim(),
        description: form.description.trim(),
      };
      await onCreate(payload);
      setForm({ key: '', name: '', description: '' });
    } catch (err) {
      const message =
        err instanceof Error ? err.message : 'Unable to create permission.';
      setError(message);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="grid grid--two-columns">
      <div className="card">
        <div className="card__header">
          <h2>Existing Permissions</h2>
          <span className="card__meta">{permissions.length} total</span>
        </div>
        <ul className="permission-list">
          {permissions.map((permission) => (
            <li key={permission.id} className="permission-list__item">
              <div>
                <strong>{permission.key}</strong>
                <p>{permission.name}</p>
              </div>
              {permission.description && (
                <span className="permission-list__description">
                  {permission.description}
                </span>
              )}
            </li>
          ))}
          {permissions.length === 0 && <li>No permissions defined.</li>}
        </ul>
      </div>
      <form className="card form-card" onSubmit={handleSubmit}>
        <h2>Create Permission</h2>
        <label htmlFor="permission-key">
          Key
          <input
            id="permission-key"
            value={form.key}
            onChange={(event) => setForm((prev) => ({ ...prev, key: event.target.value }))}
            placeholder="permissions.manage-users"
            required
            disabled={!canCreate}
          />
        </label>
        <label htmlFor="permission-name">
          Name
          <input
            id="permission-name"
            value={form.name}
            onChange={(event) => setForm((prev) => ({ ...prev, name: event.target.value }))}
            placeholder="Manage Users"
            required
            disabled={!canCreate}
          />
        </label>
        <label htmlFor="permission-description">
          Description
          <textarea
            id="permission-description"
            value={form.description}
            onChange={(event) =>
              setForm((prev) => ({ ...prev, description: event.target.value }))
            }
            placeholder="Describe what this permission allows."
            rows={3}
            disabled={!canCreate}
          />
        </label>
        {error && <p className="form-error">{error}</p>}
        <button
          type="submit"
          className="primary-button"
          disabled={isSubmitting || !canCreate}
          title={!canCreate ? 'You lack permission to create permissions.' : undefined}
        >
          {isSubmitting ? 'Creating...' : 'Create Permission'}
        </button>
      </form>
    </div>
  );
};
