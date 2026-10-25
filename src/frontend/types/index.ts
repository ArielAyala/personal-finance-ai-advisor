export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  preferredCurrency: string;
  createdAt: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}

export interface RefreshTokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface LoginInput {
  email: string;
  password: string;
}

export interface RegisterInput {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  preferredCurrency?: string;
}

export interface Account {
  id: string;
  name: string;
  type: string;
  currency: string;
  accountNumber?: string | null;
  institution?: string | null;
  currentBalance: number;
  isActive: boolean;
  createdAt: string;
}

export interface CreateAccountInput {
  name: string;
  type: string;
  currency: string;
  accountNumber?: string;
  institution?: string;
  initialBalance?: number;
}

export interface UpdateAccountInput {
  name?: string;
  accountNumber?: string;
  institution?: string;
  currentBalance?: number;
  isActive?: boolean;
}

export interface Category {
  id: string;
  name: string;
  description?: string | null;
  type: string;
  color?: string | null;
  icon?: string | null;
  createdAt: string;
}

export interface CreateCategoryInput {
  name: string;
  description?: string;
  type: string;
  color?: string;
  icon?: string;
}

export interface UpdateCategoryInput {
  name?: string;
  description?: string;
  color?: string;
  icon?: string;
}

export interface Transaction {
  id: string;
  accountId: string;
  accountName: string;
  categoryId?: string | null;
  categoryName?: string | null;
  date: string;
  amount: number;
  currency: string;
  merchant?: string | null;
  description?: string | null;
  notes?: string | null;
  type: string;
  reference?: string | null;
  createdAt: string;
}

export interface CreateTransactionInput {
  accountId: string;
  categoryId?: string;
  date: string;
  amount: number;
  currency: string;
  merchant?: string;
  description?: string;
  notes?: string;
  type: string;
}

export interface UpdateTransactionInput {
  categoryId?: string;
  date?: string;
  amount?: number;
  merchant?: string;
  description?: string;
  notes?: string;
}

export interface TransactionFilter {
  startDate?: string;
  endDate?: string;
  accountId?: string;
  categoryId?: string;
  type?: string;
  pageNumber?: number;
  pageSize?: number;
}
