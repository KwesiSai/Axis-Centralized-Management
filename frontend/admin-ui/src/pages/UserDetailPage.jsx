import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  getUser,
  getApplications,
  assignApplication,
  revokeApplication,
  deactivateUser,
  activateUser,
} from '../services/api';

export default function UserDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [user, setUser] = useState(null);
  const [allApps, setAllApps] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [successMsg, setSuccessMsg] = useState('');
  const [actionLoading, setActionLoading] = useState(false);

  const flash = (msg) => {
    setSuccessMsg(msg);
    setTimeout(() => setSuccessMsg(''), 3000);
  };

  const fetchData = useCallback(async () => {
    try {
      const [userRes, appsRes] = await Promise.all([getUser(id), getApplications()]);
      setUser(userRes.data);
      setAllApps(appsRes.data);
    } catch {
      setError('Could not load user.');
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => { fetchData(); }, [fetchData]);

  const handleToggleActive = async () => {
    setActionLoading(true);
    try {
      if (user.isActive) {
        await deactivateUser(user.id);
        flash('User deactivated.');
      } else {
        await activateUser(user.id);
        flash('User activated.');
      }
      fetchData();
    } catch (err) {
      setError(err.response?.data?.message || 'Action failed.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleAssign = async (appId, appName) => {
    setActionLoading(true);
    try {
      await assignApplication(user.id, appId);
      flash(`Access to "${appName}" granted.`);
      fetchData();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to assign application.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleRevoke = async (appId, appName) => {
    if (!window.confirm(`Remove access to "${appName}" for ${user.username}?`)) return;
    setActionLoading(true);
    try {
      await revokeApplication(user.id, appId);
      flash(`Access to "${appName}" revoked.`);
      fetchData();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to revoke application.');
    } finally {
      setActionLoading(false);
    }
  };

  if (loading) return <div style={{ padding: '48px', textAlign: 'center', color: 'var(--text-muted)' }}>Loading…</div>;
  if (!user) return <div style={{ padding: '48px', textAlign: 'center', color: 'var(--danger)' }}>{error || 'User not found.'}</div>;

  const assignedAppNames = new Set(user.applications);

  return (
    <div style={{ maxWidth: '700px' }}>
      {/* Breadcrumb */}
      <div style={{ marginBottom: '20px' }}>
        <button
          onClick={() => navigate('/users')}
          style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer', padding: 0, fontSize: '14px' }}
        >
          ← Back to Users
        </button>
      </div>

      {/* Alerts */}
      {error && <div className="alert alert-error">{error}</div>}
      {successMsg && <div className="alert alert-success">{successMsg}</div>}

      {/* User Info Card */}
      <div className="card" style={{ marginBottom: '20px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: '12px' }}>
          <div>
            <h1 style={{ fontSize: '20px', fontWeight: 700, marginBottom: '6px' }}>
              {user.username}
            </h1>
            <div style={{ display: 'flex', gap: '8px', alignItems: 'center', flexWrap: 'wrap' }}>
              <span className={`badge ${user.isActive ? 'badge-active' : 'badge-inactive'}`}>
                {user.isActive ? 'Active' : 'Inactive'}
              </span>
              <span style={{ color: 'var(--text-muted)', fontSize: '13px' }}>
                User ID: {user.id}
              </span>
              <span style={{ color: 'var(--text-muted)', fontSize: '13px' }}>
                Created: {new Date(user.createdAt).toLocaleDateString('en-US', {
                  year: 'numeric', month: 'long', day: 'numeric'
                })}
              </span>
            </div>
          </div>

          <button
            className={user.isActive ? 'btn-danger' : 'btn-success'}
            onClick={handleToggleActive}
            disabled={actionLoading}
          >
            {user.isActive ? 'Deactivate User' : 'Activate User'}
          </button>
        </div>
      </div>

      {/* Application Access Card */}
      <div className="card">
        <h2 style={{ fontSize: '15px', fontWeight: 600, marginBottom: '16px' }}>
          Application Access
        </h2>

        {allApps.length === 0 ? (
          <p style={{ color: 'var(--text-muted)' }}>No applications found.</p>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
            {allApps.map((app) => {
              const hasAccess = assignedAppNames.has(app.name);
              return (
                <div
                  key={app.id}
                  style={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    padding: '14px 16px',
                    borderRadius: '6px',
                    border: '1px solid var(--border)',
                    background: hasAccess ? '#f0fff4' : 'var(--bg)',
                    transition: 'background 0.2s',
                  }}
                >
                  <div>
                    <div style={{ fontWeight: 600, fontSize: '14px' }}>{app.name}</div>
                    <div style={{ color: 'var(--text-muted)', fontSize: '12px', marginTop: '2px' }}>
                      {app.description}
                    </div>
                  </div>

                  <div style={{ display: 'flex', alignItems: 'center', gap: '10px', flexShrink: 0 }}>
                    {hasAccess ? (
                      <>
                        <span className="badge badge-active">✓ Access granted</span>
                        <button
                          className="btn-danger btn-sm"
                          onClick={() => handleRevoke(app.id, app.name)}
                          disabled={actionLoading}
                        >
                          Revoke
                        </button>
                      </>
                    ) : (
                      <>
                        <span style={{ color: 'var(--text-muted)', fontSize: '12px' }}>No access</span>
                        <button
                          className="btn-primary btn-sm"
                          onClick={() => handleAssign(app.id, app.name)}
                          disabled={actionLoading}
                        >
                          Grant Access
                        </button>
                      </>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}
