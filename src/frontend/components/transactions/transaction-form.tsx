'use client';

import { useState, type FormEvent } from 'react';
import { useCreateTransaction, useUpdateTransaction } from '@/hooks/use-transactions';
import type { Account, Category, Transaction } from '@/types';

const TRANSACTION_TYPES = [
  { value: 'Expense', label: 'Expense' },
  { value: 'Income', label: 'Income' },
  { value: 'Transfer', label: 'Transfer' },
];

const labelClass = 'block text-sm font-medium text-zinc-700 dark:text-zinc-300';
const inputClass =
  'mt-1 block w-full rounded-md border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-800 px-3 py-2 text-sm text-zinc-900 dark:text-zinc-50 disabled:opacity-60';

interface TransactionFormProps {
  transaction?: Transaction;
  accounts: Account[];
  categories: Category[];
  onDone: () => void;
  onCancel: () => void;
}

export default function TransactionForm({
  transaction,
  accounts,
  categories,
  onDone,
  onCancel,
}: TransactionFormProps) {
  const createTransaction = useCreateTransaction();
  const updateTransaction = useUpdateTransaction();

  const [accountId, setAccountId] = useState(transaction?.accountId ?? accounts[0]?.id ?? '');
  const [type, setType] = useState(transaction?.type ?? 'Expense');
  const [date, setDate] = useState(transaction?.date.slice(0, 10) ?? new Date().toISOString().slice(0, 10));
  const [amount, setAmount] = useState(transaction ? String(transaction.amount) : '');
  const [categoryId, setCategoryId] = useState(transaction?.categoryId ?? '');
  const [merchant, setMerchant] = useState(transaction?.merchant ?? '');
  const [description, setDescription] = useState(transaction?.description ?? '');
  const [notes, setNotes] = useState(transaction?.notes ?? '');

  const selectedAccount = accounts.find((account) => account.id === accountId);

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (transaction) {
      updateTransaction.mutate(
        {
          id: transaction.id,
          data: {
            categoryId: categoryId || undefined,
            date,
            amount: Number(amount),
            merchant: merchant || undefined,
            description: description || undefined,
            notes: notes || undefined,
          },
        },
        { onSuccess: onDone }
      );
    } else if (selectedAccount) {
      createTransaction.mutate(
        {
          accountId,
          categoryId: categoryId || undefined,
          date,
          amount: Number(amount),
          currency: selectedAccount.currency,
          type,
          merchant: merchant || undefined,
          description: description || undefined,
          notes: notes || undefined,
        },
        { onSuccess: onDone }
      );
    }
  };

  const isPending = createTransaction.isPending || updateTransaction.isPending;
  const error = createTransaction.error ?? updateTransaction.error;

  return (
    <div className="rounded-lg border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-6 shadow-sm">
      <h2 className="text-lg font-semibold text-zinc-900 dark:text-zinc-50">
        {transaction ? 'Edit transaction' : 'New transaction'}
      </h2>

      <form onSubmit={handleSubmit} className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label htmlFor="transaction-account" className={labelClass}>
            Account
          </label>
          <select
            id="transaction-account"
            required
            value={accountId}
            onChange={(e) => setAccountId(e.target.value)}
            disabled={!!transaction}
            className={inputClass}
          >
            {accounts.map((account) => (
              <option key={account.id} value={account.id}>
                {account.name} ({account.currency})
              </option>
            ))}
          </select>
        </div>

        <div>
          <label htmlFor="transaction-type" className={labelClass}>
            Type
          </label>
          <select
            id="transaction-type"
            value={type}
            onChange={(e) => setType(e.target.value)}
            disabled={!!transaction}
            className={inputClass}
          >
            {TRANSACTION_TYPES.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label htmlFor="transaction-date" className={labelClass}>
            Date
          </label>
          <input
            id="transaction-date"
            type="date"
            required
            value={date}
            onChange={(e) => setDate(e.target.value)}
            className={inputClass}
          />
        </div>

        <div>
          <label htmlFor="transaction-amount" className={labelClass}>
            Amount {selectedAccount && `(${selectedAccount.currency})`}
          </label>
          <input
            id="transaction-amount"
            type="number"
            required
            min={0}
            step="0.01"
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            className={inputClass}
          />
        </div>

        <div>
          <label htmlFor="transaction-category" className={labelClass}>
            Category
          </label>
          <select
            id="transaction-category"
            value={categoryId}
            onChange={(e) => setCategoryId(e.target.value)}
            className={inputClass}
          >
            <option value="">No category</option>
            {categories.map((category) => (
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label htmlFor="transaction-merchant" className={labelClass}>
            Merchant
          </label>
          <input
            id="transaction-merchant"
            type="text"
            maxLength={200}
            value={merchant}
            onChange={(e) => setMerchant(e.target.value)}
            className={inputClass}
          />
        </div>

        <div className="sm:col-span-2">
          <label htmlFor="transaction-description" className={labelClass}>
            Description
          </label>
          <input
            id="transaction-description"
            type="text"
            maxLength={500}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            className={inputClass}
          />
        </div>

        <div className="sm:col-span-2">
          <label htmlFor="transaction-notes" className={labelClass}>
            Notes
          </label>
          <textarea
            id="transaction-notes"
            rows={3}
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            className={inputClass}
          />
        </div>

        {error && (
          <p className="sm:col-span-2 text-sm text-red-600 dark:text-red-400">
            Could not save the transaction. Check the data and try again.
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
