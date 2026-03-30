import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { getUsers, createUser, deactivateUser, activateUser, deleteUser } from '../services/api';

export default function UsersPage() {
  const navigate = useNavigate();
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState('all'); // all | active | inactive
  const [search, setSearch] = useState('');

  // Create user form
  const [showCreate, setShowCreate] = useState(false);
  const [newUsername, setNewUsername] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [createError, setCreateError] = useState('');
  const [createLoading, setCreateLoading] = useState(false);
  const [successMsg, setSuccessMsg] = useState('');

  const fetchUsers = useCallback(async () => {
    try {
      const res = await getUsers();
      setUsers(res.data);
    } catch {
      // silently fail; will show empty state
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchUsers(); }, [fetchUsers]);

  const handleCreate = async (e) => {
    e.preventDefault();
    setCreateError('');
    setCreateLoading(true);
    try {
      await createUser(newUsername, newPassword);
      setNewUsername('');
      setNewPassword('');
      setShowCreate(false);
      setSuccessMsg(`User "${newUsername}" created successfully.`);
      setTimeout(() => setSuccessMsg(''), 3000);
      fetchUsers();
    } catch (err) {
      setCreateError(err.response?.data?.message || 'Failed to create user.');
    } finally {
      setCreateLoading(false);
    }
  };

  const handleToggleActive = async (user) => {
    try {
      if (user.isActive) {
        await deactivateUser(user.id);
      } else {
        await activateUser(user.id);
      }
      fetchUsers();
    } catch (err) {
      alert(err.response?.data?.message || 'Action failed.');
    }
  };

  const handleDelete = async (user) => {
    if (!window.confirm(`Delete user "${user.username}"? This cannot be undone.`)) return;
    try {
      await deleteUser(user.id);
      fetchUsers();
    } catch (err) {
      alert(err.response?.data?.message || 'Delete failed.');
    }
  };

  const filtered = users.filter((u) => {
    const matchesFilter =
      filter === 'all' ||
      (filter === 'active' && u.isActive) ||
      (filter === 'inactive' && !u.isActive);
    const matchesSearch = u.username.toLowerCase().includes(search.toLowerCase());
    return matchesFilter && matchesSearch;
  });

  return (
    <div>
      {/* Page header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '24px' }}>
        <div>
          <h1 style={{ fontSize: '20px', fontWeight: 700 }}>Users</h1>
          <p style={{ color: 'var(--text-muted)', marginTop: '2px' }}>
            {users.length} total · {users.filter(u => u.isActive).length} active
          </p>
        </div>
        <button className="btn-primary" onClick={() => { setShowCreate(!showCreate); setCreateError(''); }}>
          {showCreate ? 'Cancel' : '+ New User'}
        </button>
      </div>

      {/* Success */}
      {successMsg && <div className="alert alert-success">{successMsg}</div>}

      {/* Create form */}
      {showCreate && (
        <div className="card" style={{ marginBottom: '24px' }}>
          <h2 style={{ fontSize: '15px', fontWeight: 600, marginBottom: '16px' }}>Create New User</h2>
          {createError && <div className="alert alert-error">{createError}</div>}
          <form onSubmit={handleCreate} style={{ display: 'flex', gap: '12px', alignItems: 'flex-end', flexWrap: 'wrap' }}>
            <div className="field" style={{ flex: 1, minWidth: '160px', marginBottom: 0 }}>
              <label>Username</label>
              <input
                value={newUsername}
                onChange={(e) => setNewUsername(e.target.value)}
                placeholder="e.g. jdoe"
                required
              />
            </div>
            <div className="field" style={{ flex: 1, minWidth: '160px', marginBottom: 0 }}>
              <label>Password</label>
              <input
                type="password"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                placeholder="Min. 6 characters"
                required
                minLength={6}
              />
            </div>
            <button type="submit" className="btn-primary" disabled={createLoading} style={{ flexShrink: 0 }}>
              {createLoading ? 'Creating…' : 'Create User'}
            </button>
          </form>
        </div>
      )}

      {/* Filters */}
      <div style={{ display: 'flex', gap: '12px', marginBottom: '16px', flexWrap: 'wrap', alignItems: 'center' }}>
        <input
          style={{ maxWidth: '260px' }}
          placeholder="Search by username…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <div style={{ display: 'flex', gap: '4px' }}>
          {['all', 'active', 'inactive'].map((f) => (
            <button
              key={f}
              onClick={() => setFilter(f)}
              style={{
                padding: '6px 14px',
                borderRadius: '6px',
                border: '1px solid var(--border)',
                background: filter === f ? 'var(--primary)' : 'var(--surface)',
                color: filter === f ? '#fff' : 'var(--text-muted)',
                fontWeight: 500,
                textTransform: 'capitalize',
                fontSize: '13px',
              }}
            >
              {f}
            </button>
          ))}
        </div>
      </div>

      {/* Table */}
      <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
        {loading ? (
          <div className="empty-state"><p>Loading users…</p></div>
        ) : filtered.length === 0 ? (
          <div className="empty-state">
            <h3>No users found</h3>
            <p>{search ? 'Try a different search term.' : 'Create your first user above.'}</p>
          </div>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Username</th>
                <th>Status</th>
                <th>Applications</th>
                <th>Created</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((u) => (
                <tr key={u.id}>
                  <td>
                    <button
                      onClick={() => navigate(`/users/${u.id}`)}
                      style={{ background: 'none', border: 'none', color: 'var(--primary)', fontWeight: 600, cursor: 'pointer', padding: 0, fontSize: '14px' }}
                    >
                      {u.username}
                    </button>
                  </td>
                  <td>
                    <span className={`badge ${u.isActive ? 'badge-active' : 'badge-inactive'}`}>
                      {u.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td>
                    {u.applications.length === 0 ? (
                      <span style={{ color: 'var(--text-muted)', fontSize: '13px' }}>None assigned</span>
                    ) : (
                      <div style={{ display: 'flex', gap: '4px', flexWrap: 'wrap' }}>
                        {u.applications.map((app) => (
                          <span key={app} className="badge badge-app">{app}</span>
                        ))}
                      </div>
                    )}
                  </td>
                  <td style={{ color: 'var(--text-muted)', fontSize: '13px' }}>
                    {new Date(u.createdAt).toLocaleDateString()}
                  </td>
                  <td>
                    <div className="actions">
                      <button
                        className="btn-ghost btn-sm"
                        onClick={() => navigate(`/users/${u.id}`)}
                      >
                        Manage
                      </button>
                      <button
                        className={`btn-sm ${u.isActive ? 'btn-danger' : 'btn-success'}`}
                        onClick={() => handleToggleActive(u)}
                      >
                        {u.isActive ? 'Deactivate' : 'Activate'}
                      </button>
                      <button
                        className="btn-ghost btn-sm"
                        style={{ color: 'var(--danger)' }}
                        onClick={() => handleDelete(u)}
                      >
                        Delete
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
