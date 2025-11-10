import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { permissionApi } from '../api/permissionApi';
import { userApi } from '../api/userApi';
import { PermissionList } from '../components/PermissionList';
import { useHasPermission } from '../hooks/useHasPermission';
import { PermissionCatalog } from '../utils/permissionCatalog';

export const PermissionsPage = () => {
  const queryClient = useQueryClient();
  const canManagePermissions = useHasPermission(PermissionCatalog.ManagePermissions);

  const [selectedUserId, setSelectedUserId] = useState<string>('');
  const [permissionKey, setPermissionKey] = useState('');
  const isAssignmentEnabled = Boolean(selectedUserId) && canManagePermissions;

  const permissionsQuery = useQuery({
    queryKey: ['permissions'],
    queryFn: permissionApi.getAll,
  });

  const usersQuery = useQuery({
    queryKey: ['users-all'],
    queryFn: userApi.getAll,
  });

  const userPermissionsQuery = useQuery({
    queryKey: ['user-permissions', selectedUserId],
    queryFn: () => permissionApi.getUserPermissions(selectedUserId),
    enabled: Boolean(selectedUserId),
  });

  const createPermissionMutation = useMutation({
    mutationFn: permissionApi.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['permissions'] });
    },
  });

  const assignMutation = useMutation({
    mutationFn: (payload: string[]) =>
      permissionApi.assignPermissions(selectedUserId, { permissions: payload }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['user-permissions', selectedUserId] });
      queryClient.invalidateQueries({ queryKey: ['users'] });
      queryClient.invalidateQueries({ queryKey: ['users-all'] });
      setPermissionKey('');
    },
  });

  const removeMutation = useMutation({
    mutationFn: (payload: string[]) =>
      permissionApi.removePermissions(selectedUserId, { permissions: payload }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['user-permissions', selectedUserId] });
      queryClient.invalidateQueries({ queryKey: ['users'] });
      queryClient.invalidateQueries({ queryKey: ['users-all'] });
    },
  });

  const availablePermissionOptions = useMemo(() => {
    if (!permissionsQuery.data) {
      return [];
    }
    return permissionsQuery.data.map((permission) => ({
      value: permission.key,
      label: permission.key,
    }));
  }, [permissionsQuery.data]);

  const handleAssign = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!selectedUserId || !permissionKey) {
      return;
    }
    assignMutation.mutate([permissionKey]);
  };

  const handleRemove = (permission: string) => {
    if (!selectedUserId) {
      return;
    }
    removeMutation.mutate([permission]);
  };

  return (
    <div className="page">
      <h1>Permission Management</h1>
      {permissionsQuery.data && (
        <PermissionList
          permissions={permissionsQuery.data}
          canCreate={canManagePermissions}
          onCreate={async (payload) => {
            if (!canManagePermissions) {
              throw new Error('You do not have permission to create permissions.');
            }
            await createPermissionMutation.mutateAsync(payload);
          }}
        />
      )}
      <div className="card">
        <h2>Assign Permissions to Users</h2>
        <div className="assign-section">
          <label htmlFor="user-select">
            Select User
            <select
              id="user-select"
              value={selectedUserId}
              onChange={(event) => setSelectedUserId(event.target.value)}
              disabled={!canManagePermissions && !selectedUserId}
            >
              <option value="">Choose a user</option>
              {usersQuery.data?.map((user) => (
                <option key={user.id} value={user.id}>
                  {user.email}
                </option>
              ))}
            </select>
          </label>
          {selectedUserId && (
            <form className="assign-form" onSubmit={handleAssign}>
              <label htmlFor="permission-select">
                Permission
                <select
                  id="permission-select"
                  value={permissionKey}
                  onChange={(event) => setPermissionKey(event.target.value)}
                  required
                  disabled={!canManagePermissions}
                >
                  <option value="">Choose permission</option>
                  {availablePermissionOptions.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </label>
              <button
                type="submit"
                className="primary-button"
                disabled={assignMutation.isPending || !isAssignmentEnabled}
                title={
                  canManagePermissions ? undefined : 'You lack permission to manage user permissions.'
                }
              >
                {assignMutation.isPending ? 'Assigning...' : 'Assign Permission'}
              </button>
            </form>
          )}
        </div>
        {selectedUserId && userPermissionsQuery.data && (
          <div className="pill-group pill-group--interactive">
            {userPermissionsQuery.data.length === 0 ? (
              <span className="chip chip--muted">No permissions assigned.</span>
            ) : (
              userPermissionsQuery.data.map((permission) => (
                <button
                  key={permission}
                  type="button"
                  className="chip chip--action"
                  onClick={() => handleRemove(permission)}
                    disabled={removeMutation.isPending || !canManagePermissions}
                  title={
                    canManagePermissions ? undefined : 'You lack permission to remove permissions.'
                  }
                >
                  {permission} ✕
                </button>
              ))
            )}
          </div>
        )}
      </div>
    </div>
  );
};
