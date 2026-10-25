import { useMutation, useQueryClient } from '@tanstack/react-query';
import { importApi } from '@/services/import';

export const usePreviewImport = () => {
  return useMutation({
    mutationFn: (file: File) => importApi.preview(file),
  });
};

export const useExecuteImport = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: importApi.execute,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
      queryClient.invalidateQueries({ queryKey: ['categories'] });
    },
  });
};
