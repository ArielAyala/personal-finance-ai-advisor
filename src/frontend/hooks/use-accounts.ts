import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { accountsApi } from '@/services/accounts';
import type { CreateAccountInput, UpdateAccountInput } from '@/types';

export const useAccounts = () => {
  return useQuery({
    queryKey: ['accounts'],
    queryFn: accountsApi.getAll,
  });
};

export const useAccount = (id: string) => {
  return useQuery({
    queryKey: ['accounts', id],
    queryFn: () => accountsApi.getById(id),
    enabled: !!id,
  });
};

export const useCreateAccount = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateAccountInput) => accountsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
    },
  });
};

export const useUpdateAccount = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateAccountInput }) =>
      accountsApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
    },
  });
};

export const useDeleteAccount = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => accountsApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
    },
  });
};
