'use client';

import Link from "next/link";
import { useState } from "react";
import { useAccounts, useDeleteAccount } from "@/hooks/use-accounts";
import AccountForm from "@/components/accounts/account-form";
import type { Account } from "@/types";

export default function AccountsPage() {
  const { data: accounts, isLoading, error } = useAccounts();
  const deleteAccount = useDeleteAccount();
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<Account | null>(null);

  const isFormOpen = showForm || editing !== null;

  const closeForm = () => {
    setShowForm(false);
    setEditing(null);
  };

  const handleDelete = (account: Account) => {
    if (window.confirm(`Delete account "${account.name}"?`)) {
      deleteAccount.mutate(account.id);
    }
  };

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
                className="text-sm font-medium text-zinc-900 dark:text-zinc-50"
              >
                Accounts
              </Link>
              <Link
                href="/transactions"
                className="text-sm font-medium text-zinc-700 hover:text-zinc-900 dark:text-zinc-300 dark:hover:text-zinc-50"
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
            <h1 className="text-3xl font-bold text-zinc-900 dark:text-zinc-50">Accounts</h1>
            <p className="mt-2 text-sm text-zinc-600 dark:text-zinc-400">
              Manage your financial accounts
            </p>
          </div>
          <div className="mt-4 sm:mt-0">
            <button
              type="button"
              onClick={() => setShowForm(true)}
              disabled={isFormOpen}
              className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-zinc-900 hover:bg-zinc-800 disabled:opacity-50 dark:bg-zinc-50 dark:text-zinc-900 dark:hover:bg-zinc-200"
            >
              Add Account
            </button>
          </div>
        </div>

        {isFormOpen && (
          <div className="mt-8">
            <AccountForm
              key={editing?.id ?? "new"}
              account={editing ?? undefined}
              onDone={closeForm}
              onCancel={closeForm}
            />
          </div>
        )}

        {/* Accounts List */}
        <div className="mt-8">
          {isLoading && (
            <div className="text-center py-12">
              <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-zinc-900 dark:border-zinc-50"></div>
            </div>
          )}

          {error && (
            <div className="rounded-md bg-red-50 dark:bg-red-900/20 p-4">
              <div className="flex">
                <div className="ml-3">
                  <h3 className="text-sm font-medium text-red-800 dark:text-red-200">
                    Error loading accounts
                  </h3>
                  <div className="mt-2 text-sm text-red-700 dark:text-red-300">
                    <p>{error instanceof Error ? error.message : 'An error occurred'}</p>
                  </div>
                </div>
              </div>
            </div>
          )}

          {!isLoading && !error && accounts && accounts.length === 0 && (
            <div className="text-center py-12">
              <svg
                className="mx-auto h-12 w-12 text-zinc-400"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                aria-hidden="true"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z"
                />
              </svg>
              <h3 className="mt-2 text-sm font-medium text-zinc-900 dark:text-zinc-50">No accounts</h3>
              <p className="mt-1 text-sm text-zinc-500 dark:text-zinc-400">
                Get started by creating a new account.
              </p>
            </div>
          )}

          {!isLoading && !error && accounts && accounts.length > 0 && (
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
              {accounts.map((account) => (
                <div
                  key={account.id}
                  className="relative rounded-lg border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 px-6 py-5 shadow-sm hover:border-zinc-300 dark:hover:border-zinc-700"
                >
                  <div className="flex items-center justify-between">
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium text-zinc-900 dark:text-zinc-50 truncate">
                        {account.name}
                      </p>
                      <p className="text-sm text-zinc-500 dark:text-zinc-400 truncate">
                        {account.type}
                      </p>
                    </div>
                    <div
                      className={`ml-4 flex-shrink-0 h-3 w-3 rounded-full ${
                        account.isActive
                          ? 'bg-green-400 dark:bg-green-500'
                          : 'bg-zinc-300 dark:bg-zinc-600'
                      }`}
                    />
                  </div>
                  <div className="mt-4">
                    <p className="text-2xl font-semibold text-zinc-900 dark:text-zinc-50">
                      {new Intl.NumberFormat('es-PY', {
                        style: 'currency',
                        currency: account.currency,
                      }).format(account.currentBalance)}
                    </p>
                  </div>
                  {account.institution && (
                    <p className="mt-2 text-xs text-zinc-500 dark:text-zinc-400">
                      {account.institution}
                    </p>
                  )}
                  <div className="mt-4 flex justify-end gap-3">
                    <button
                      type="button"
                      onClick={() => setEditing(account)}
                      disabled={isFormOpen}
                      className="text-sm font-medium text-zinc-700 hover:text-zinc-900 disabled:opacity-50 dark:text-zinc-300 dark:hover:text-zinc-50"
                    >
                      Edit
                    </button>
                    <button
                      type="button"
                      onClick={() => handleDelete(account)}
                      disabled={deleteAccount.isPending}
                      className="text-sm font-medium text-red-600 hover:text-red-800 disabled:opacity-50 dark:text-red-400 dark:hover:text-red-300"
                    >
                      Delete
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </main>
    </div>
  );
}
