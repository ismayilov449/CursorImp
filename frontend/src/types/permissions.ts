export interface PermissionDto {
  id: string;
  key: string;
  name: string;
  description: string;
  createdAtUtc: string;
}

export interface UpdateUserPermissionsRequest {
  permissions: string[];
}
