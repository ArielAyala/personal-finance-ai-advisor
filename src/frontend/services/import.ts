import apiClient from '@/lib/api-client';
import type { ExecuteImportInput, ImportPreview, ImportResult } from '@/types';

export const importApi = {
  preview: async (file: File): Promise<ImportPreview> => {
    const formData = new FormData();
    formData.append('file', file);
    // Drop the instance-level JSON content type so axios sends the FormData as multipart/form-data
    const response = await apiClient.post<ImportPreview>('/import/preview', formData, {
      headers: { 'Content-Type': undefined },
    });
    return response.data;
  },

  execute: async (data: ExecuteImportInput): Promise<ImportResult> => {
    const response = await apiClient.post<ImportResult>('/import/execute', data);
    return response.data;
  },
};
