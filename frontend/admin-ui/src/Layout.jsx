import { Outlet, NavLink } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Layout() {
  const { user, logout } = useAuth();

  return (
    <div style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
      <header style={{
        background: '#1a202c',
        color: '#fff',
        padding: '0 24px',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        height: '56px',
        boxShadow: '0 2px 4px rgba(0,0,0,0.2)'
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '32px' }}>
          <span style={{ fontWeight: 700, fontSize: '16px', letterSpacing: '-0.3px' }}>
            ⚙️ UserAdmin
          </span>
          <nav style={{ display: 'flex', gap: '4px' }}>
            <NavLink
              to="/users"
              style={({ isActive }) => ({
                color: isActive ? '#fff' : '#a0aec0',
                padding: '6px 12px',
                borderRadius: '6px',
                fontWeight: 500,
                fontSize: '14px',
                background: isActive ? 'rgba(255,255,255,0.1)' : 'transparent',
              })}
            >
              Users
            </NavLink>
          </nav>
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
          <span style={{ color: '#a0aec0', fontSize: '13px' }}>
            Signed in as <strong style={{ color: '#fff' }}>{user?.username}</strong>
          </span>
          <button
            onClick={logout}
            className="btn-ghost btn-sm"
            style={{ color: '#a0aec0', borderColor: '#4a5568' }}
          >
            Sign out
          </button>
        </div>
      </header>

      <main style={{ flex: 1, padding: '32px 24px', maxWidth: '1100px', width: '100%', margin: '0 auto' }}>
        <Outlet />
      </main>
    </div>
  );
}
