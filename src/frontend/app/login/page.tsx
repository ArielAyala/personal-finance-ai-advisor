import LoginForm from '@/components/auth/login-form';

export default function LoginPage() {
  return <LoginForm allowRegistration={process.env.NEXT_PUBLIC_ALLOW_REGISTRATION === 'true'} />;
}
