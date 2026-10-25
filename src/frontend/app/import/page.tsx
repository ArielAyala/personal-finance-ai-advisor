'use client';

import Link from "next/link";
import { useState } from "react";
import { useExecuteImport, usePreviewImport } from "@/hooks/use-import";
import type { ImportPreview, ImportResult } from "@/types";

export default function ImportPage() {
  const [file, setFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<ImportPreview | null>(null);
  const [result, setResult] = useState<ImportResult | null>(null);
  const [skipDuplicates, setSkipDuplicates] = useState(true);
  const [rejectedAccounts, setRejectedAccounts] = useState<Set<string>>(new Set());
  const [rejectedCategories, setRejectedCategories] = useState<Set<string>>(new Set());

  const previewImport = usePreviewImport();
  const executeImport = useExecuteImport();

  const handlePreview = () => {
    if (!file) return;
    setResult(null);
    setRejectedAccounts(new Set());
    setRejectedCategories(new Set());
    previewImport.mutate(file, {
      onSuccess: (data) => setPreview(data),
    });
  };

  const toggle = (set: Set<string>, name: string, update: (next: Set<string>) => void) => {
    const next = new Set(set);
    if (next.has(name)) {
      next.delete(name);
    } else {
      next.add(name);
    }
    update(next);
  };

  const handleConfirm = () => {
    if (!preview) return;
    executeImport.mutate(
      {
        batchId: preview.batchId,
        skipDuplicates,
        transactions: preview.transactions,
        approvedAccounts: preview.newAccounts
          .map((a) => a.name)
          .filter((name) => !rejectedAccounts.has(name)),
        approvedCategories: preview.newCategories
          .map((c) => c.name)
          .filter((name) => !rejectedCategories.has(name)),
      },
      {
        onSuccess: (data) => {
          setResult(data);
          setPreview(null);
          setFile(null);
        },
      }
    );
  };

  const formatAmount = (amount: number, currency: string) =>
    new Intl.NumberFormat('es-PY', { style: 'currency', currency }).format(Math.abs(amount));

  const cellClass = "px-4 py-3 text-sm text-zinc-700 dark:text-zinc-300 whitespace-nowrap";
  const headClass = "px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-zinc-500 dark:text-zinc-400";
  const cardClass = "rounded-lg border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-6 shadow-sm";

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
                className="text-sm font-medium text-zinc-700 hover:text-zinc-900 dark:text-zinc-300 dark:hover:text-zinc-50"
              >
                Transactions
              </Link>
            </div>
          </div>
        </div>
      </nav>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8">
        <div>
          <h1 className="text-3xl font-bold text-zinc-900 dark:text-zinc-50">Import transactions</h1>
          <p className="mt-2 text-sm text-zinc-600 dark:text-zinc-400">
            Upload a CSV or Excel file, review the preview, then confirm the import.
          </p>
        </div>

        {/* Step 1: upload */}
        <section className={cardClass}>
          <div className="flex flex-col gap-4 sm:flex-row sm:items-end">
            <div className="flex-1">
              <label htmlFor="import-file" className="block text-sm font-medium text-zinc-700 dark:text-zinc-300">
                File (.csv, .xlsx, .xls)
              </label>
              <input
                id="import-file"
                type="file"
                accept=".csv,.xlsx,.xls"
                onChange={(e) => {
                  setFile(e.target.files?.[0] ?? null);
                  setPreview(null);
                }}
                className="mt-1 block w-full text-sm text-zinc-700 dark:text-zinc-300"
              />
            </div>
            <button
              type="button"
              onClick={handlePreview}
              disabled={!file || previewImport.isPending}
              className="inline-flex items-center px-4 py-2 rounded-md shadow-sm text-sm font-medium text-white bg-zinc-900 hover:bg-zinc-800 disabled:opacity-50 dark:bg-zinc-50 dark:text-zinc-900 dark:hover:bg-zinc-200"
            >
              {previewImport.isPending ? 'Reading file...' : 'Preview'}
            </button>
          </div>

          {previewImport.error && (
            <p className="mt-4 text-sm text-red-600 dark:text-red-400">
              Could not read the file. Check the format and try again.
            </p>
          )}
        </section>

        {/* Result */}
        {result && (
          <section className={cardClass}>
            <h2 className="text-lg font-semibold text-zinc-900 dark:text-zinc-50">
              {result.success ? 'Import completed' : 'Import finished with errors'}
            </h2>
            <dl className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-3">
              <div>
                <dt className="text-sm text-zinc-500 dark:text-zinc-400">Imported</dt>
                <dd className="text-2xl font-semibold text-green-600 dark:text-green-400">{result.importedRecords}</dd>
              </div>
              <div>
                <dt className="text-sm text-zinc-500 dark:text-zinc-400">Skipped</dt>
                <dd className="text-2xl font-semibold text-zinc-900 dark:text-zinc-50">{result.skippedRecords}</dd>
              </div>
              <div>
                <dt className="text-sm text-zinc-500 dark:text-zinc-400">Failed</dt>
                <dd className="text-2xl font-semibold text-red-600 dark:text-red-400">{result.failedRecords}</dd>
              </div>
            </dl>
            {result.errors.length > 0 && (
              <ul className="mt-4 list-disc pl-5 text-sm text-zinc-600 dark:text-zinc-400">
                {result.errors.map((message, index) => (
                  <li key={index}>{message}</li>
                ))}
              </ul>
            )}
            <div className="mt-4">
              <Link href="/transactions" className="text-sm font-medium text-zinc-900 underline dark:text-zinc-50">
                View transactions
              </Link>
            </div>
          </section>
        )}

        {/* Step 2: preview */}
        {preview && (
          <>
            <section className={cardClass}>
              <h2 className="text-lg font-semibold text-zinc-900 dark:text-zinc-50">{preview.fileName}</h2>
              <dl className="mt-4 grid grid-cols-2 gap-4 sm:grid-cols-4">
                <div>
                  <dt className="text-sm text-zinc-500 dark:text-zinc-400">Rows</dt>
                  <dd className="text-xl font-semibold text-zinc-900 dark:text-zinc-50">{preview.totalRecords}</dd>
                </div>
                <div>
                  <dt className="text-sm text-zinc-500 dark:text-zinc-400">Valid</dt>
                  <dd className="text-xl font-semibold text-green-600 dark:text-green-400">{preview.validRecords}</dd>
                </div>
                <div>
                  <dt className="text-sm text-zinc-500 dark:text-zinc-400">Invalid</dt>
                  <dd className="text-xl font-semibold text-red-600 dark:text-red-400">{preview.invalidRecords}</dd>
                </div>
                <div>
                  <dt className="text-sm text-zinc-500 dark:text-zinc-400">Possible duplicates</dt>
                  <dd className="text-xl font-semibold text-amber-600 dark:text-amber-400">{preview.duplicateRecords}</dd>
                </div>
              </dl>

              {preview.errors.length > 0 && (
                <ul className="mt-4 list-disc pl-5 text-sm text-red-600 dark:text-red-400">
                  {preview.errors.map((error, index) => (
                    <li key={index}>{error.message}</li>
                  ))}
                </ul>
              )}
            </section>

            {/* New accounts and categories */}
            {(preview.newAccounts.length > 0 || preview.newCategories.length > 0) && (
              <section className={cardClass}>
                <h2 className="text-lg font-semibold text-zinc-900 dark:text-zinc-50">New accounts and categories</h2>
                <p className="mt-1 text-sm text-zinc-600 dark:text-zinc-400">
                  These do not exist yet. Uncheck any you do not want created; their rows will be skipped (accounts) or imported without a category (categories).
                </p>

                {preview.newAccounts.length > 0 && (
                  <div className="mt-4">
                    <h3 className="text-sm font-medium text-zinc-900 dark:text-zinc-50">Accounts to create</h3>
                    <ul className="mt-2 divide-y divide-zinc-200 dark:divide-zinc-800">
                      {preview.newAccounts.map((account) => (
                        <li key={account.name} className="flex items-center gap-3 py-2">
                          <input
                            id={`account-${account.name}`}
                            type="checkbox"
                            checked={!rejectedAccounts.has(account.name)}
                            onChange={() => toggle(rejectedAccounts, account.name, setRejectedAccounts)}
                            className="h-4 w-4 rounded border-zinc-300"
                          />
                          <label htmlFor={`account-${account.name}`} className="flex-1 text-sm text-zinc-900 dark:text-zinc-50">
                            {account.name}
                          </label>
                          <span className="text-sm text-zinc-500 dark:text-zinc-400">{account.currency} · {account.type}</span>
                          <span className="text-sm text-zinc-500 dark:text-zinc-400">{account.rowCount} rows</span>
                        </li>
                      ))}
                    </ul>
                  </div>
                )}

                {preview.newCategories.length > 0 && (
                  <div className="mt-6">
                    <h3 className="text-sm font-medium text-zinc-900 dark:text-zinc-50">Categories to create</h3>
                    <ul className="mt-2 divide-y divide-zinc-200 dark:divide-zinc-800">
                      {preview.newCategories.map((category) => (
                        <li key={category.name} className="flex items-center gap-3 py-2">
                          <input
                            id={`category-${category.name}`}
                            type="checkbox"
                            checked={!rejectedCategories.has(category.name)}
                            onChange={() => toggle(rejectedCategories, category.name, setRejectedCategories)}
                            className="h-4 w-4 rounded border-zinc-300"
                          />
                          <label htmlFor={`category-${category.name}`} className="flex-1 text-sm text-zinc-900 dark:text-zinc-50">
                            {category.name}
                          </label>
                          <span className="text-sm text-zinc-500 dark:text-zinc-400">{category.type}</span>
                          <span className="text-sm text-zinc-500 dark:text-zinc-400">{category.rowCount} rows</span>
                        </li>
                      ))}
                    </ul>
                  </div>
                )}
              </section>
            )}

            {/* Rows */}
            <section className="overflow-x-auto rounded-lg border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 shadow-sm">
              <table className="min-w-full divide-y divide-zinc-200 dark:divide-zinc-800">
                <thead className="bg-zinc-50 dark:bg-zinc-800/50">
                  <tr>
                    <th className={headClass}>Row</th>
                    <th className={headClass}>Date</th>
                    <th className={headClass}>Merchant</th>
                    <th className={headClass}>Account</th>
                    <th className={headClass}>Category</th>
                    <th className={headClass}>Type</th>
                    <th className={`${headClass} text-right`}>Amount</th>
                    <th className={headClass}>Status</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-zinc-200 dark:divide-zinc-800">
                  {preview.transactions.map((row) => (
                    <tr key={row.rowNumber} className={row.isValid ? undefined : "bg-red-50 dark:bg-red-900/10"}>
                      <td className={cellClass}>{row.rowNumber}</td>
                      <td className={cellClass}>{new Date(row.date).toLocaleDateString()}</td>
                      <td className={cellClass}>{row.merchant || '—'}</td>
                      <td className={cellClass}>{row.account || '—'}</td>
                      <td className={cellClass}>{row.category || '—'}</td>
                      <td className={cellClass}>{row.amount < 0 ? 'Expense' : 'Income'}</td>
                      <td className={`${cellClass} text-right font-medium`}>{formatAmount(row.amount, row.currency)}</td>
                      <td className={cellClass}>
                        {!row.isValid ? (
                          <span className="text-red-600 dark:text-red-400">{row.validationMessages.join(', ')}</span>
                        ) : row.isDuplicate ? (
                          <span className="text-amber-600 dark:text-amber-400">Possible duplicate</span>
                        ) : (
                          <span className="text-green-600 dark:text-green-400">Ready</span>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </section>

            {/* Confirm */}
            <section className={`${cardClass} flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between`}>
              <div className="flex items-center gap-2">
                <input
                  id="skip-duplicates"
                  type="checkbox"
                  checked={skipDuplicates}
                  onChange={(e) => setSkipDuplicates(e.target.checked)}
                  className="h-4 w-4 rounded border-zinc-300"
                />
                <label htmlFor="skip-duplicates" className="text-sm text-zinc-700 dark:text-zinc-300">
                  Skip possible duplicates
                </label>
              </div>
              <div className="flex gap-3">
                <button
                  type="button"
                  onClick={() => setPreview(null)}
                  className="inline-flex items-center px-4 py-2 rounded-md border border-zinc-300 dark:border-zinc-700 text-sm font-medium text-zinc-700 dark:text-zinc-300 hover:bg-zinc-50 dark:hover:bg-zinc-800"
                >
                  Discard
                </button>
                <button
                  type="button"
                  onClick={handleConfirm}
                  disabled={executeImport.isPending || preview.validRecords === 0}
                  className="inline-flex items-center px-4 py-2 rounded-md shadow-sm text-sm font-medium text-white bg-zinc-900 hover:bg-zinc-800 disabled:opacity-50 dark:bg-zinc-50 dark:text-zinc-900 dark:hover:bg-zinc-200"
                >
                  {executeImport.isPending ? 'Importing...' : 'Confirm import'}
                </button>
              </div>
            </section>

            {executeImport.error && (
              <p className="text-sm text-red-600 dark:text-red-400">
                The import could not be completed. Try again.
              </p>
            )}
          </>
        )}
      </main>
    </div>
  );
}
