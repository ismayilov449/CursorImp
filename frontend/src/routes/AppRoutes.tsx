import { Navigate, Route, Routes } from 'react-router-dom';
import { Layout } from '../components/Layout';
import { ProtectedRoute } from '../components/ProtectedRoute';
import { DashboardPage } from '../pages/DashboardPage';
import { LoginPage } from '../pages/LoginPage';
import { PermissionsPage } from '../pages/PermissionsPage';
import { RegisterPage } from '../pages/RegisterPage';

export const AppRoutes = () => (
  <Routes>
    <Route path="/" element={<Layout />}>
      <Route element={<ProtectedRoute />}>
        <Route index element={<DashboardPage />} />
        <Route path="permissions" element={<PermissionsPage />} />
      </Route>
      <Route path="login" element={<LoginPage />} />
      <Route path="register" element={<RegisterPage />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Route>
  </Routes>
);
