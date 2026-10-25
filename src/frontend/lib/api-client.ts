import axios, { type AxiosRequestConfig } from 'axios';
import { authTokens } from '@/lib/auth-tokens';
import type { RefreshTokenResponse } from '@/types';

const baseURL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5213/api';

const apiClient = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true,
});

type RetriableRequest = AxiosRequestConfig & { _retry?: boolean };

let refreshPromise: Promise<string> | null = null;

const refreshAccessToken = (): Promise<string> => {
  if (!refreshPromise) {
    refreshPromise = (async () => {
      const refreshToken = authTokens.getRefreshToken();
      if (!refreshToken) {
        throw new Error('No refresh token available.');
      }

      const response = await axios.post<RefreshTokenResponse>(`${baseURL}/auth/refresh`, { refreshToken });
      authTokens.save(response.data.accessToken, response.data.refreshToken);
      return response.data.accessToken;
    })().finally(() => {
      refreshPromise = null;
    });
  }

  return refreshPromise;
};

const redirectToLogin = () => {
  authTokens.clear();
  if (window.location.pathname !== '/login') {
    window.location.href = '/login';
  }
};

apiClient.interceptors.request.use(
  (config) => {
    const token = authTokens.getAccessToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config as RetriableRequest | undefined;
    const isAuthRequest = originalRequest?.url?.startsWith('/auth/') ?? false;

    if (error.response?.status !== 401 || !originalRequest) {
      return Promise.reject(error);
    }

    if (isAuthRequest || originalRequest._retry) {
      redirectToLogin();
      return Promise.reject(error);
    }

    originalRequest._retry = true;

    try {
      const accessToken = await refreshAccessToken();
      originalRequest.headers = {
        ...originalRequest.headers,
        Authorization: `Bearer ${accessToken}`,
      };
      return apiClient(originalRequest);
    } catch (refreshError) {
      redirectToLogin();
      return Promise.reject(refreshError);
    }
  }
);

export default apiClient;
