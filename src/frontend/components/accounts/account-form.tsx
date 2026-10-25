'use client';

import { useState, type FormEvent } from 'react';
import { useCreateAccount, useUpdateAccount } from '@/hooks/use-accounts';
import type { Account } from '@/types';

const ACCOUNT_TYPES = [
  { value: 'BankAccount', label: 'Bank account' },
  { value: 'CreditCard', label: 'Credit card' },
  { value: 'Cash', label: 'Cash' },
  { value: 'Investment', label: 'Investment' },
  { value: 'Other', label: 'Other' },
];

const CURRENCIES = ['PYG', 'USD'];

const labelClass = 'block text-sm font-medium text-zinc-700 dark:text-zinc-300';
const inputClass =
  'mt-1 block w-full rounded-md border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-800 px-3 py-2 text-sm text-zinc-900 dark:text-zinc-50 disabled:opacity-60';

interface AccountFormProps {
  account?: Account;
  onDone: () => void;
  onCancel: () => void;
}

export default function AccountForm({ account, onDone, onCancel }: AccountFormProps) {
  const createAccount = useCreateAccount();
  const updateAccount = useUpdateAccount();

  const [name, setName] = useState(account?.name ?? '');
  const [type, setType] = useState(account?.type ?? 'BankAccount');
  const [currency, setCurrency] = useState(account?.currency ?? 'PYG');
  const [institution, setInstitution] = useState(account?.institution ?? '');
  const [accountNumber, setAccountNumber] = useState(account?.accountNumber ?? '');
  const [balance, setBalance] = useState(account ? String(account.currentBalance) : '0');
  const [isActive, setIsActive] = useState(account?.isActive ?? true);

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (account) {
      updateAccount.mutate(
        {
          id: account.id,
          data: {
            name,
            institution,
            accountNumber,
            currentBalance: Number(balance),
            isActive,
          },
        },
        { onSuccess: onDone }
      );
    } else {
      createAccount.mutate(
        {
          name,
          type,
          currency,
          institution: institution || undefined,
          accountNumber: accountNumber || undefined,
          initialBalance: Number(balance),
        },
        { onSuccess: onDone }
      );
    }
  };

  const isPending = createAccount.isPending || updateAccount.isPending;
  const error = createAccount.error ?? updateAccount.error;

  return (
    <div className="rounded-lg border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-6 shadow-sm">
      <h2 className="text-lg font-semibold text-zinc-900 dark:text-zinc-50">
        {account ? 'Edit account' : 'New account'}
      </h2>

      <form onSubmit={handleSubmit} className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div className="sm:col-span-2">
          <label htmlFor="account-name" className={labelClass}>
            Name
          </label>
          <input
            id="account-name"
            type="text"
            required
            minLength={3}
            maxLength={200}
            value={name}
            onChange={(e) => setName(e.target.value)}
            className={inputClass}
          />
        </div>

        <div>
          <label htmlFor="account-type" className={labelClass}>
            Type
          </label>
          <select
            id="account-type"
            value={type}
            onChange={(e) => setType(e.target.value)}
            disabled={!!account}
            className={inputClass}
          >
            {ACCOUNT_TYPES.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label htmlFor="account-currency" className={labelClass}>
            Currency
          </label>
          <select
            id="account-currency"
            value={currency}
            onChange={(e) => setCurrency(e.target.value)}
            disabled={!!account}
            className={inputClass}
          >
            {CURRENCIES.map((code) => (
              <option key={code} value={code}>
                {code}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label htmlFor="account-institution" className={labelClass}>
            Institution
          </label>
          <input
            id="account-institution"
            type="text"
            maxLength={200}
            value={institution}
            onChange={(e) => setInstitution(e.target.value)}
            className={inputClass}
          />
        </div>

        <div>
          <label htmlFor="account-number" className={labelClass}>
            Account number
          </label>
          <input
            id="account-number"
            type="text"
            maxLength={50}
            value={accountNumber}
            onChange={(e) => setAccountNumber(e.target.value)}
            className={inputClass}
          />
        </div>

        <div>
          <label htmlFor="account-balance" className={labelClass}>
            {account ? 'Current balance' : 'Initial balance'}
          </label>
          <input
            id="account-balance"
            type="number"
            required
            min={0}
            step="0.01"
            value={balance}
            onChange={(e) => setBalance(e.target.value)}
            className={inputClass}
          />
        </div>

        {account && (
          <div className="flex items-center gap-2 sm:self-end sm:pb-2">
            <input
              id="account-active"
              type="checkbox"
              checked={isActive}
              onChange={(e) => setIsActive(e.target.checked)}
              className="h-4 w-4 rounded border-zinc-300"
            />
            <label htmlFor="account-active" className="text-sm text-zinc-700 dark:text-zinc-300">
              Active
            </label>
          </div>
        )}

        {error && (
          <p className="sm:col-span-2 text-sm text-red-600 dark:text-red-400">
            Could not save the account. Check the data and try again.
          </p>
        )}

        <div className="sm:col-span-2 flex justify-end gap-3">
          <button
            type="button"
            onClick={onCancel}
            className="inline-flex items-center px-4 py-2 rounded-md border border-zinc-300 dark:border-zinc-700 text-sm font-medium text-zinc-700 dark:text-zinc-300 hover:bg-zinc-50 dark:hover:bg-zinc-800"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={isPending}
            className="inline-flex items-center px-4 py-2 rounded-md shadow-sm text-sm font-medium text-white bg-zinc-900 hover:bg-zinc-800 disabled:opacity-50 dark:bg-zinc-50 dark:text-zinc-900 dark:hover:bg-zinc-200"
          >
            {isPending ? 'Saving...' : 'Save'}
          </button>
        </div>
      </form>
    </div>
  );
}
