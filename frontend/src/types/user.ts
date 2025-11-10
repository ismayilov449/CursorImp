export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc?: string | null;
  permissions: string[];
}
