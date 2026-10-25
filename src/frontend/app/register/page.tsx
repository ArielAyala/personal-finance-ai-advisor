import { notFound } from 'next/navigation';
import RegisterForm from '@/components/auth/register-form';

export default function RegisterPage() {
  if (process.env.NEXT_PUBLIC_ALLOW_REGISTRATION !== 'true') {
    notFound();
  }

  return <RegisterForm />;
}
