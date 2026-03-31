import { useState, useEffect } from 'react';
import { getMe } from '../services/api';
import { useAuth } from '../context/AuthContext';

export default function ProfilePage() {
  const { user: authUser } = useAuth();
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    getMe()
      .then((res) => setProfile(res.data))
      .catch(() => setError('Could not load profile.'))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div style={{ padding: '48px', textAlign: 'center', color: 'var(--text-muted)' }}>Loading…</div>;
  if (error) return <div style={{ padding: '48px', textAlign: 'center', color: 'var(--danger)' }}>{error}</div>;

  return (
    <div style={{ maxWidth: '500px' }}>
      <div style={{ marginBottom: '24px' }}>
        <h1 style={{ fontSize: '20px', fontWeight: 700 }}>My Profile</h1>
        <p style={{ color: 'var(--text-muted)', marginTop: '2px' }}>
          Your account details and application access
        </p>
      </div>

      {/* Account Info */}
      <div className="card" style={{ marginBottom: '16px' }}>
        <h2 style={{ fontSize: '14px', fontWeight: 600, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.5px', marginBottom: '16px' }}>
          Account
        </h2>
        <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <span style={{ color: 'var(--text-muted)', fontSize: '13px' }}>Username</span>
            <span style={{ fontWeight: 600 }}>{profile.username}</span>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <span style={{ color: 'var(--text-muted)', fontSize: '13px' }}>Status</span>
            <span className={`badge ${profile.isActive ? 'badge-active' : 'badge-inactive'}`}>
              {profile.isActive ? 'Active' : 'Inactive'}
            </span>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <span style={{ color: 'var(--text-muted)', fontSize: '13px' }}>Member since</span>
            <span style={{ fontSize: '13px' }}>
              {new Date(profile.createdAt).toLocaleDateString('en-US', {
                year: 'numeric', month: 'long', day: 'numeric'
              })}
            </span>
          </div>
        </div>
      </div>

      {/* Application Access */}
      <div className="card">
        <h2 style={{ fontSize: '14px', fontWeight: 600, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.5px', marginBottom: '16px' }}>
          Application Access
        </h2>
        {profile.applications.length === 0 ? (
          <p style={{ color: 'var(--text-muted)', fontSize: '13px' }}>
            You have not been assigned to any applications yet. Contact your administrator.
          </p>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
            {profile.applications.map((app) => (
              <div
                key={app}
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  padding: '10px 14px',
                  borderRadius: '6px',
                  background: '#f0fff4',
                  border: '1px solid #c6f6d5'
                }}
              >
                <span style={{ fontWeight: 600, fontSize: '14px' }}>{app}</span>
                <span className="badge badge-active">✓ Access granted</span>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
