import axios from 'axios';

const api = axios.create({ baseURL: '/api' });

// Attach JWT token to every request if present
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// ── Auth ──────────────────────────────────────────────────────────────────────
export const login = (username, password, application) =>
  api.post('/auth/login', { username, password, application });

export const registerAdmin = (username, password, secretKey) =>
  api.post('/auth/register-admin', { username, password, secretKey });

// ── Users ─────────────────────────────────────────────────────────────────────
export const getUsers = () => api.get('/users');
export const getMe = () => api.get('/users/me');
export const getUser = (id) => api.get(`/users/${id}`);
export const createUser = (username, password) =>
  api.post('/users', { username, password });
export const deactivateUser = (id) => api.patch(`/users/${id}/deactivate`);
export const activateUser = (id) => api.patch(`/users/${id}/activate`);
export const deleteUser = (id) => api.delete(`/users/${id}`);

// ── Application Assignments ───────────────────────────────────────────────────
export const assignApplication = (userId, applicationId) =>
  api.post(`/users/${userId}/applications`, { applicationId });
export const revokeApplication = (userId, appId) =>
  api.delete(`/users/${userId}/applications/${appId}`);

// ── Applications ──────────────────────────────────────────────────────────────
export const getApplications = () => api.get('/applications');
