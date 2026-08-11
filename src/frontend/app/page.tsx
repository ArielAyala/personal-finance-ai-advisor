import Link from "next/link";

export default function Home() {
  return (
    <div className="min-h-screen bg-zinc-50 dark:bg-zinc-950">
      {/* Navigation */}
      <nav className="border-b border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <h1 className="text-xl font-semibold text-zinc-900 dark:text-zinc-50">
                Personal Finance AI Advisor
              </h1>
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

      {/* Hero Section */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="text-center">
          <h2 className="text-4xl font-bold tracking-tight text-zinc-900 dark:text-zinc-50 sm:text-5xl md:text-6xl">
            Manage Your Finances
          </h2>
          <p className="mt-3 max-w-md mx-auto text-base text-zinc-500 dark:text-zinc-400 sm:text-lg md:mt-5 md:text-xl md:max-w-3xl">
            Track your income, expenses, and investments. Get AI-powered insights to make better financial decisions.
          </p>
          <div className="mt-5 max-w-md mx-auto sm:flex sm:justify-center md:mt-8">
            <Link
              href="/dashboard"
              className="w-full flex items-center justify-center px-8 py-3 border border-transparent text-base font-medium rounded-md text-white bg-zinc-900 hover:bg-zinc-800 dark:bg-zinc-50 dark:text-zinc-900 dark:hover:bg-zinc-200 md:py-4 md:text-lg md:px-10"
            >
              Get Started
            </Link>
          </div>
        </div>

        {/* Features */}
        <div className="mt-16">
          <div className="grid grid-cols-1 gap-8 sm:grid-cols-2 lg:grid-cols-3">
            <div className="pt-6">
              <div className="flow-root bg-white dark:bg-zinc-900 rounded-lg px-6 pb-8 border border-zinc-200 dark:border-zinc-800">
                <div className="-mt-6">
                  <div className="inline-flex items-center justify-center p-3 bg-zinc-900 dark:bg-zinc-50 rounded-md shadow-lg">
                    <svg className="h-6 w-6 text-white dark:text-zinc-900" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
                    </svg>
                  </div>
                  <h3 className="mt-8 text-lg font-medium text-zinc-900 dark:text-zinc-50 tracking-tight">
                    Track Transactions
                  </h3>
                  <p className="mt-5 text-base text-zinc-500 dark:text-zinc-400">
                    Import and categorize your transactions from multiple accounts.
                  </p>
                </div>
              </div>
            </div>

            <div className="pt-6">
              <div className="flow-root bg-white dark:bg-zinc-900 rounded-lg px-6 pb-8 border border-zinc-200 dark:border-zinc-800">
                <div className="-mt-6">
                  <div className="inline-flex items-center justify-center p-3 bg-zinc-900 dark:bg-zinc-50 rounded-md shadow-lg">
                    <svg className="h-6 w-6 text-white dark:text-zinc-900" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                    </svg>
                  </div>
                  <h3 className="mt-8 text-lg font-medium text-zinc-900 dark:text-zinc-50 tracking-tight">
                    Analyze Patterns
                  </h3>
                  <p className="mt-5 text-base text-zinc-500 dark:text-zinc-400">
                    Visualize your spending patterns and financial trends over time.
                  </p>
                </div>
              </div>
            </div>

            <div className="pt-6">
              <div className="flow-root bg-white dark:bg-zinc-900 rounded-lg px-6 pb-8 border border-zinc-200 dark:border-zinc-800">
                <div className="-mt-6">
                  <div className="inline-flex items-center justify-center p-3 bg-zinc-900 dark:bg-zinc-50 rounded-md shadow-lg">
                    <svg className="h-6 w-6 text-white dark:text-zinc-900" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                    </svg>
                  </div>
                  <h3 className="mt-8 text-lg font-medium text-zinc-900 dark:text-zinc-50 tracking-tight">
                    AI Insights
                  </h3>
                  <p className="mt-5 text-base text-zinc-500 dark:text-zinc-400">
                    Get personalized recommendations powered by artificial intelligence.
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
}
