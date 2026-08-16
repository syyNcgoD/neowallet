"use client";

import { useMemo, useState, useEffect } from "react";
import { bankAccounts, type BankAccount } from "@/data/seed";
import { cn } from "@/lib/utils";
import { AccountSummary } from "@/components/accounts/account-summary";
import { AccountCard } from "@/components/accounts/account-grid";
import { AddAccount } from "@/components/accounts/add-account";
import { EmptyState } from "@/components/empty-state";
import { useAuth } from "@/contexts/auth-context";
import { useWalletSummary, useCreateWallet } from "@/hooks/use-wallets";

const filterTabs = [
  { value: "all", label: "All" },
  { value: "checking", label: "Checking" },
  { value: "savings", label: "Savings" },
  { value: "crypto", label: "Crypto" },
  { value: "investment", label: "Investment" },
] as const;

type AccountType = (typeof filterTabs)[number]["value"];

export function AccountsPageClient() {
  const { user } = useAuth();
  const walletId = user?.id;
  const { data: walletSummary } = useWalletSummary(walletId);
  const createWalletMutation = useCreateWallet();

  const [selectedType, setSelectedType] = useState<AccountType>("all");
  const [accounts, setAccounts] = useState<BankAccount[]>(bankAccounts);

  useEffect(() => {
    if (walletSummary) {
      const primaryAccount: BankAccount = {
        id: `wallet-${walletSummary.id}`,
        name: `NeoWallet Core (${walletSummary.currency})`,
        type: "checking",
        institution: "NeoWallet Distributed Ledger",
        institutionLogo: "/logos/stripe-com.png",
        accountNumber: `****${walletSummary.id.slice(-4)}`,
        balance: walletSummary.balance,
        currency: walletSummary.currency === "EUR" ? "€" : "$",
        change: 12.4,
        changePercent: 12.4,
        lastActivity: "Just now",
        color: "bg-primary",
      };

      setAccounts([primaryAccount, ...bankAccounts]);
    }
  }, [walletSummary]);

  const filtered = useMemo(
    () =>
      selectedType === "all"
        ? accounts
        : accounts.filter((a) => a.type === selectedType),
    [accounts, selectedType]
  );

  const handleAddAccount = async (account: BankAccount) => {
    if (user?.id) {
      try {
        await createWalletMutation.mutateAsync({
          ownerId: user.id,
          currency: account.currency === "€" ? "EUR" : "USD",
        });
      } catch {
        // Handled in mutation
      }
    }
    setAccounts((prev) => [account, ...prev]);
  };

  return (
    <div className="flex flex-col gap-4">
      {/* Summary row */}
      <AccountSummary accounts={accounts} />

      {/* Filter tabs */}
      <div className="flex flex-wrap gap-1.5">
        {filterTabs.map((tab) => (
          <button
            key={tab.value}
            onClick={() => setSelectedType(tab.value)}
            className={cn(
              "rounded-lg px-3 py-1.5 text-sm font-medium transition-colors",
              selectedType === tab.value
                ? "bg-primary text-primary-foreground"
                : "bg-muted text-muted-foreground hover:bg-muted/80 hover:text-foreground"
            )}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* Account grid + add card */}
      {filtered.length === 0 ? (
        <EmptyState
          variant="filter"
          title="No accounts in this category"
          description="You don't have any accounts of this type yet. Try a different filter or link a new account."
        />
      ) : (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
          {filtered.map((account, i) => (
            <AccountCard key={account.id} account={account} index={i} />
          ))}
          <AddAccount onAdd={handleAddAccount} />
        </div>
      )}
    </div>
  );
}
