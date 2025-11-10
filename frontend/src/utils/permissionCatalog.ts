export const PermissionCatalog = {
  ManageUsers: 'permissions.manage-users',
  ViewUsers: 'permissions.view-users',
  ManagePermissions: 'permissions.manage-permissions',
  ViewPermissions: 'permissions.view-permissions',
} as const;

export type PermissionKey = (typeof PermissionCatalog)[keyof typeof PermissionCatalog];
