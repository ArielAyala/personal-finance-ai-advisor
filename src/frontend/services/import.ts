import apiClient from '@/lib/api-client';
import type { ExecuteImportInput, ImportPreview, ImportResult } from '@/types';

export const importApi = {
  preview: async (file: File): Promise<ImportPreview> => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await apiClient.post<ImportPreview>('/import/preview', formData);
    return response.data;
  },

  execute: async (data: ExecuteImportInput): Promise<ImportResult> => {
    const response = await apiClient.post<ImportResult>('/import/execute', data);
    return response.data;
  },
};
