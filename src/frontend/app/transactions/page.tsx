'use client';

import Link from "next/link";
import { useState } from "react";
import { useAccounts } from "@/hooks/use-accounts";
import { useCategories } from "@/hooks/use-categories";
import { useDeleteTransaction, useTransactions } from "@/hooks/use-transactions";
import TransactionForm from "@/components/transactions/transaction-form";
import type { Transaction, TransactionFilter } from "@/types";

const PAGE_SIZE = 20;

const TYPE_OPTIONS = ['Income', 'Expense', 'Transfer'];

const selectClass =
  'mt-1 block w-full rounded-md border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-800 px-3 py-2 text-sm text-zinc-900 dark:text-zinc-50';

const emptyFilters = {
  accountId: '',
  categoryId: '',
  type: '',
  startDate: '',
  endDate: '',
};

export default function TransactionsPage() {
  const [filters, setFilters] = useState(emptyFilters);
  const [pageNumber, setPageNumber] = useState(1);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<Transaction | null>(null);

  const filter: TransactionFilter = {
    accountId: filters.accountId || undefined,
    categoryId: filters.categoryId || undefined,
    type: filters.type || undefined,
    startDate: filters.startDate || undefined,
    endDate: filters.endDate || undefined,
    pageNumber,
    pageSize: PAGE_SIZE,
  };

  const { data: transactions, isLoading, error } = useTransactions(filter);
  const { data: accounts } = useAccounts();
  const { data: categories } = useCategories();
  const deleteTransaction = useDeleteTransaction();

  const isFormOpen = showForm || editing !== null;

  const closeForm = () => {
    setShowForm(false);
    setEditing(null);
  };

  const updateFilter = (key: keyof typeof emptyFilters, value: string) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
    setPageNumber(1);
  };

  const handleDelete = (transaction: Transaction) => {
    if (window.confirm(`Delete this transaction of ${transaction.amount} ${transaction.currency}?`)) {
      deleteTransaction.mutate(transaction.id);
    }
  };

  const formatAmount = (transaction: Transaction) =>
    new Intl.NumberFormat('es-PY', {
      style: 'currency',
      currency: transaction.currency,
    }).format(transaction.amount);

  return (
    <div className="min-h-screen bg-zinc-50 dark:bg-zinc-950">
      {/* Navigation */}
      <nav className="border-b border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <Link href="/" className="text-xl font-semibold text-zinc-900 dark:text-zinc-50">
                Personal Finance AI Advisor
              </Link>
            </div>
            <div className="flex items-center gap-4">
              <Link
                href="/dashboard"
                className="text-sm font-medium text-zinc-700 hover:text-zinc-900 dark:text-zinc-300 dark:hover:text-zinc-50"
              >
                Dashboard
              </Link>
              <Link
                href="/accounts"
                className="text-sm font-medium text-zinc-700 hover:text-zinc-900 dark:text-zinc-300 dark:hover:text-zinc-50"
              >
                Accounts
              </Link>
              <Link
                href="/transactions"
                className="text-sm font-medium text-zinc-900 dark:text-zinc-50"
              >
                Transactions
              </Link>
            </div>
          </div>
        </div>
      </nav>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="sm:flex sm:items-center sm:justify-between">
          <div>
            <h1 className="text-3xl font-bold text-zinc-900 dark:text-zinc-50">Transactions</h1>
            <p className="mt-2 text-sm text-zinc-600 dark:text-zinc-400">
              Track your income, expenses and transfers
            </p>
          </div>
          <div className="mt-4 sm:mt-0">
            <button
              type="button"
              onClick={() => setShowForm(true)}
              disabled={isFormOpen || !accounts?.length}
              className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-zinc-900 hover:bg-zinc-800 disabled:opacity-50 dark:bg-zinc-50 dark:text-zinc-900 dark:hover:bg-zinc-200"
            >
              Add Transaction
            </button>
          </div>
        </div>

        {isFormOpen && accounts && (
          <div className="mt-8">
            <TransactionForm
              key={editing?.id ?? "new"}
              transaction={editing ?? undefined}
              accounts={accounts}
              categories={categories ?? []}
              onDone={closeForm}
              onCancel={closeForm}
            />
          </div>
        )}

        {/* Filters */}
        <div className="mt-8 grid grid-cols-1 gap-4 rounded-lg border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-4 sm:grid-cols-2 lg:grid-cols-5">
          <div>
            <label htmlFor="filter-account" className="block text-xs font-medium text-zinc-600 dark:text-zinc-400">Account</label>
            <select id="filter-account" value={filters.accountId} onChange={(e) => updateFilter('accountId', e.target.value)} className={selectClass}>
              <option value="">All accounts</option>
              {accounts?.map((account) => (
                <option key={account.id} value={account.id}>{account.name}</option>
              ))}
            </select>
          </div>
          <div>
            <label htmlFor="filter-category" className="block text-xs font-medium text-zinc-600 dark:text-zinc-400">Category</label>
            <select id="filter-category" value={filters.categoryId} onChange={(e) => updateFilter('categoryId', e.target.value)} className={selectClass}>
              <option value="">All categories</option>
              {categories?.map((category) => (
                <option key={category.id} value={category.id}>{category.name}</option>
              ))}
            </select>
          </div>
          <div>
            <label htmlFor="filter-type" className="block text-xs font-medium text-zinc-600 dark:text-zinc-400">Type</label>
            <select id="filter-type" value={filters.type} onChange={(e) => updateFilter('type', e.target.value)} className={selectClass}>
              <option value="">All types</option>
              {TYPE_OPTIONS.map((type) => (
                <option key={type} value={type}>{type}</option>
              ))}
            </select>
          </div>
          <div>
            <label htmlFor="filter-start" className="block text-xs font-medium text-zinc-600 dark:text-zinc-400">From</label>
            <input id="filter-start" type="date" value={filters.startDate} onChange={(e) => updateFilter('startDate', e.target.value)} className={selectClass} />
          </div>
          <div>
            <label htmlFor="filter-end" className="block text-xs font-medium text-zinc-600 dark:text-zinc-400">To</label>
            <input id="filter-end" type="date" value={filters.endDate} onChange={(e) => updateFilter('endDate', e.target.value)} className={selectClass} />
          </div>
        </div>

        {/* Transactions List */}
        <div className="mt-8">
          {isLoading && (
            <div className="text-center py-12">
              <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-zinc-900 dark:border-zinc-50"></div>
            </div>
          )}

          {error && (
            <div className="rounded-md bg-red-50 dark:bg-red-900/20 p-4">
              <h3 className="text-sm font-medium text-red-800 dark:text-red-200">
                Error loading transactions
              </h3>
              <p className="mt-2 text-sm text-red-700 dark:text-red-300">
                {error instanceof Error ? error.message : 'An error occurred'}
              </p>
            </div>
          )}

          {!isLoading && !error && transactions && transactions.length === 0 && (
            <div className="text-center py-12">
              <h3 className="mt-2 text-sm font-medium text-zinc-900 dark:text-zinc-50">No transactions</h3>
              <p className="mt-1 text-sm text-zinc-500 dark:text-zinc-400">
                {pageNumber > 1
                  ? 'No more transactions on this page.'
                  : 'Get started by adding a transaction.'}
              </p>
            </div>
          )}

          {!isLoading && !error && transactions && transactions.length > 0 && (
            <div className="overflow-x-auto rounded-lg border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 shadow-sm">
              <table className="min-w-full divide-y divide-zinc-200 dark:divide-zinc-800">
                <thead className="bg-zinc-50 dark:bg-zinc-800/50">
                  <tr>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-zinc-500 dark:text-zinc-400">Date</th>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-zinc-500 dark:text-zinc-400">Description</th>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-zinc-500 dark:text-zinc-400">Account</th>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-zinc-500 dark:text-zinc-400">Category</th>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-zinc-500 dark:text-zinc-400">Type</th>
                    <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-zinc-500 dark:text-zinc-400">Amount</th>
                    <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-zinc-500 dark:text-zinc-400">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-zinc-200 dark:divide-zinc-800">
                  {transactions.map((transaction) => (
                    <tr key={transaction.id}>
                      <td className="px-4 py-3 text-sm text-zinc-700 dark:text-zinc-300 whitespace-nowrap">
                        {new Date(transaction.date).toLocaleDateString()}
                      </td>
                      <td className="px-4 py-3 text-sm text-zinc-900 dark:text-zinc-50">
                        {transaction.merchant || transaction.description || '—'}
                      </td>
                      <td className="px-4 py-3 text-sm text-zinc-700 dark:text-zinc-300">{transaction.accountName}</td>
                      <td className="px-4 py-3 text-sm text-zinc-700 dark:text-zinc-300">{transaction.categoryName || '—'}</td>
                      <td className="px-4 py-3 text-sm text-zinc-700 dark:text-zinc-300">{transaction.type}</td>
                      <td className="px-4 py-3 text-sm text-right font-medium text-zinc-900 dark:text-zinc-50 whitespace-nowrap">
                        {formatAmount(transaction)}
                      </td>
                      <td className="px-4 py-3 text-sm text-right whitespace-nowrap">
                        <button
                          type="button"
                          onClick={() => setEditing(transaction)}
                          disabled={isFormOpen}
                          className="font-medium text-zinc-700 hover:text-zinc-900 disabled:opacity-50 dark:text-zinc-300 dark:hover:text-zinc-50"
                        >
                          Edit
                        </button>
                        <button
                          type="button"
                          onClick={() => handleDelete(transaction)}
                          disabled={deleteTransaction.isPending}
                          className="ml-3 font-medium text-red-600 hover:text-red-800 disabled:opacity-50 dark:text-red-400 dark:hover:text-red-300"
                        >
                          Delete
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {/* Pagination */}
          {!error && transactions && (transactions.length > 0 || pageNumber > 1) && (
            <div className="mt-4 flex items-center justify-between">
              <button
                type="button"
                onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
                disabled={pageNumber === 1 || isLoading}
                className="inline-flex items-center px-4 py-2 rounded-md border border-zinc-300 dark:border-zinc-700 text-sm font-medium text-zinc-700 dark:text-zinc-300 hover:bg-zinc-50 dark:hover:bg-zinc-800 disabled:opacity-50"
              >
                Previous
              </button>
              <span className="text-sm text-zinc-600 dark:text-zinc-400">Page {pageNumber}</span>
              <button
                type="button"
                onClick={() => setPageNumber((p) => p + 1)}
                disabled={transactions.length < PAGE_SIZE || isLoading}
                className="inline-flex items-center px-4 py-2 rounded-md border border-zinc-300 dark:border-zinc-700 text-sm font-medium text-zinc-700 dark:text-zinc-300 hover:bg-zinc-50 dark:hover:bg-zinc-800 disabled:opacity-50"
              >
                Next
              </button>
            </div>
          )}
        </div>
      </main>
    </div>
  );
}
