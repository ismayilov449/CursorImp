import type { UserDto } from '../types/user';

interface UsersTableProps {
  users: UserDto[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  onPageChange: (pageNumber: number) => void;
}

export const UsersTable = ({
  users,
  pageNumber,
  totalPages,
  totalCount,
  onPageChange,
}: UsersTableProps) => {
  const handlePrevious = () => {
    if (pageNumber > 1) {
      onPageChange(pageNumber - 1);
    }
  };

  const handleNext = () => {
    if (pageNumber < totalPages) {
      onPageChange(pageNumber + 1);
    }
  };

  return (
    <div className="card">
      <div className="card__header">
        <h2>Users</h2>
        <span className="card__meta">
          Showing page {pageNumber} of {Math.max(totalPages, 1)} · {totalCount} total users
        </span>
      </div>
      <div className="table-wrapper">
        <table>
          <thead>
            <tr>
              <th>Email</th>
              <th>Name</th>
              <th>Status</th>
              <th>Permissions</th>
            </tr>
          </thead>
          <tbody>
            {users.length === 0 ? (
              <tr>
                <td colSpan={4} className="empty-state">
                  No users found.
                </td>
              </tr>
            ) : (
              users.map((user) => (
                <tr key={user.id}>
                  <td>{user.email}</td>
                  <td>
                    {user.firstName} {user.lastName}
                  </td>
                  <td>
                    <span className={`chip ${user.isActive ? 'chip--success' : 'chip--danger'}`}>
                      {user.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td>
                    <div className="pill-group">
                      {user.permissions.length === 0 ? (
                        <span className="chip chip--muted">None</span>
                      ) : (
                        user.permissions.map((permission) => (
                          <span key={permission} className="chip chip--muted">
                            {permission}
                          </span>
                        ))
                      )}
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
      <div className="pagination">
        <button type="button" onClick={handlePrevious} disabled={pageNumber <= 1}>
          Previous
        </button>
        <span>
          Page {pageNumber} / {Math.max(totalPages, 1)}
        </span>
        <button type="button" onClick={handleNext} disabled={pageNumber >= totalPages}>
          Next
        </button>
      </div>
    </div>
  );
};
