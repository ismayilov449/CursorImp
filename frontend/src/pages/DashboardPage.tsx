import { useState } from 'react';
import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { userApi } from '../api/userApi';
import { UsersTable } from '../components/UsersTable';
import type { PagedResult } from '../types/common';
import type { UserDto } from '../types/user';

const PAGE_SIZE = 10;

export const DashboardPage = () => {
  const [pageNumber, setPageNumber] = useState(1);

  const { data, isPending, isError, refetch } = useQuery<PagedResult<UserDto>>({
    queryKey: ['users', pageNumber],
    queryFn: () => userApi.getPaged(pageNumber, PAGE_SIZE),
    placeholderData: keepPreviousData,
  });

  if (isPending) {
    return (
      <div className="page">
        <div className="card">Loading users...</div>
      </div>
    );
  }

  if (isError || !data) {
    return (
      <div className="page">
        <div className="card card--error">
          <h2>Unable to load users</h2>
          <p>Something went wrong while fetching user data.</p>
          <button type="button" onClick={() => refetch()}>
            Try Again
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="page">
      <UsersTable
        users={data.items}
        pageNumber={data.pageNumber}
        totalPages={data.totalPages}
        totalCount={data.totalCount}
        onPageChange={setPageNumber}
      />
    </div>
  );
};
