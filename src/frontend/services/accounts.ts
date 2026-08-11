import apiClient from '@/lib/api-client';
import type { Account, CreateAccountInput, UpdateAccountInput } from '@/types';

export const accountsApi = {
  getAll: async (): Promise<Account[]> => {
    const response = await apiClient.get<Account[]>('/accounts');
    return response.data;
  },

  getById: async (id: string): Promise<Account> => {
    const response = await apiClient.get<Account>(`/accounts/${id}`);
    return response.data;
  },

  create: async (data: CreateAccountInput): Promise<Account> => {
    const response = await apiClient.post<Account>('/accounts', data);
    return response.data;
  },

  update: async (id: string, data: UpdateAccountInput): Promise<Account> => {
    const response = await apiClient.put<Account>(`/accounts/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/accounts/${id}`);
  },
};
