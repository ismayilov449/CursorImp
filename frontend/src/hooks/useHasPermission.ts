import { useAuth } from '../context/AuthContext';

export const useHasPermission = (permission: string): boolean => {
  const { permissions } = useAuth();
  return permissions.includes(permission);
};
