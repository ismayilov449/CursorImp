import { Link, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export const Layout = () => {
  const { isAuthenticated, user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="app-shell">
      <header className="app-header">
        <div className="brand">
          <span className="brand__logo">🔐</span>
          <span>AuthApp</span>
        </div>
        <nav className="nav-links">
          {isAuthenticated ? (
            <>
              <Link to="/" className={location.pathname === '/' ? 'active' : ''}>
                Dashboard
              </Link>
              <Link
                to="/permissions"
                className={location.pathname.startsWith('/permissions') ? 'active' : ''}
              >
                Permissions
              </Link>
            </>
          ) : (
            <>
              <Link to="/login" className={location.pathname === '/login' ? 'active' : ''}>
                Login
              </Link>
              <Link
                to="/register"
                className={location.pathname === '/register' ? 'active' : ''}
              >
                Register
              </Link>
            </>
          )}
        </nav>
        {isAuthenticated && (
          <div className="user-menu">
            <div className="user-menu__info">
              <span className="user-menu__name">
                {user?.firstName} {user?.lastName}
              </span>
              <span className="user-menu__email">{user?.email}</span>
            </div>
            <button type="button" onClick={handleLogout} className="user-menu__logout">
              Logout
            </button>
          </div>
        )}
      </header>
      <main className="app-main">
        <Outlet />
      </main>
    </div>
  );
};
