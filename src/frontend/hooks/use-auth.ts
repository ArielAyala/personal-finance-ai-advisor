'use client';

import { useMutation } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { authTokens } from '@/lib/auth-tokens';
import { authApi } from '@/services/auth';
import type { AuthResponse, LoginInput, RegisterInput } from '@/types';

export const useLogin = () => {
  const router = useRouter();

  return useMutation({
    mutationFn: (data: LoginInput) => authApi.login(data),
    onSuccess: (response: AuthResponse) => {
      authTokens.save(response.accessToken, response.refreshToken);
      router.push('/accounts');
    },
  });
};

export const useRegister = () => {
  const router = useRouter();

  return useMutation({
    mutationFn: (data: RegisterInput) => authApi.register(data),
    onSuccess: (response: AuthResponse) => {
      authTokens.save(response.accessToken, response.refreshToken);
      router.push('/accounts');
    },
  });
};
