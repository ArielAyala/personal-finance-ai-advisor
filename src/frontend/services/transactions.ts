import apiClient from '@/lib/api-client';
import type { 
  Transaction, 
  CreateTransactionInput, 
  UpdateTransactionInput,
  TransactionFilter 
} from '@/types';

export const transactionsApi = {
  getAll: async (filter?: TransactionFilter): Promise<Transaction[]> => {
    const response = await apiClient.get<Transaction[]>('/transactions', {
      params: filter,
    });
    return response.data;
  },

  getById: async (id: string): Promise<Transaction> => {
    const response = await apiClient.get<Transaction>(`/transactions/${id}`);
    return response.data;
  },

  create: async (data: CreateTransactionInput): Promise<Transaction> => {
    const response = await apiClient.post<Transaction>('/transactions', data);
    return response.data;
  },

  update: async (id: string, data: UpdateTransactionInput): Promise<Transaction> => {
    const response = await apiClient.put<Transaction>(`/transactions/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/transactions/${id}`);
  },
};
