import {
  createContext,
  useState,
  useEffect,
  useCallback,
  type ReactNode,
} from 'react';
import api from '../services/api';
import type {
  UserInfo,
  LoginRequest,
  RegisterRequest,
  AuthResponse,
} from '../types';

export interface AuthContextType {
  user: UserInfo | null;
  isAuthenticated: boolean;
  isAdmin: boolean;
  login: (data: LoginRequest) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
  refreshUser: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

function decodeJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(jsonPayload);
  } catch {
    return null;
  }
}

function extractUserFromToken(token: string): UserInfo | null {
  const payload = decodeJwtPayload(token);
  if (!payload) return null;

  const id =
    (payload['nameid'] as string) ||
    (payload[
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'
    ] as string) ||
    (payload['sub'] as string) ||
    '';
  const email =
    (payload['email'] as string) ||
    (payload[
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'
    ] as string) ||
    '';
  const fullName =
    (payload['unique_name'] as string) ||
    (payload[
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'
    ] as string) ||
    (payload['name'] as string) ||
    '';
  const role =
    (payload['role'] as string) ||
    (payload[
      'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
    ] as string) ||
    'User';
  const balance = (payload['balance'] as number) || 0;

  return { id, email, fullName, role, balance };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserInfo | null>(null);

  useEffect(() => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      const userFromToken = extractUserFromToken(token);
      if (userFromToken) {
        setUser(userFromToken);
        // Fetch fresh data from server
        api
          .get('/api/users/me')
          .then((res) => setUser(res.data))
          .catch(() => {
            // Token may be expired; let the interceptor handle refresh
          });
      }
    }
  }, []);

  const login = useCallback(async (data: LoginRequest) => {
    const response = await api.post<AuthResponse>('/api/auth/login', data);
    const { accessToken, refreshToken, user: userInfo } = response.data;
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
    setUser(userInfo);
  }, []);

  const register = useCallback(async (data: RegisterRequest) => {
    const response = await api.post<AuthResponse>('/api/auth/register', data);
    const { accessToken, refreshToken, user: userInfo } = response.data;
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
    setUser(userInfo);
  }, []);

  const logout = useCallback(async () => {
    try {
      await api.post('/api/auth/logout');
    } catch {
      // Ignore logout errors
    }
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    setUser(null);
  }, []);

  const refreshUser = useCallback(async () => {
    try {
      const response = await api.get<UserInfo>('/api/users/me');
      setUser(response.data);
    } catch {
      // If fetching user fails, leave current state
    }
  }, []);

  const isAuthenticated = user !== null;
  const isAdmin = user?.role === 'Admin';

  return (
    <AuthContext.Provider
      value={{ user, isAuthenticated, isAdmin, login, register, logout, refreshUser }}
    >
      {children}
    </AuthContext.Provider>
  );
}
