"use client";

import { useMemo, useState } from "react";
import type { FullTransaction } from "@/data/seed";
import { TransactionSummary } from "@/components/transactions/transaction-summary";
import { TransactionFilters } from "@/components/transactions/transaction-filters";
import { TransactionTable } from "@/components/transactions/transaction-table";
import { TransactionActions } from "@/components/transactions/transaction-actions";
import { useWallet } from "@/contexts/wallet-context";
import { useWalletTransactions } from "@/hooks/use-wallets";
import { toast } from "sonner";
import { RefreshCwIcon, ArrowDownLeftIcon, PlusIcon } from "lucide-react";
import { Button } from "@/components/ui/button";

export function TransactionsPageClient() {
  const { activeWallet, deposit, isMutating } = useWallet();
  const { data: liveTransactions = [], isLoading, refetch, isRefetching } = useWalletTransactions(activeWallet?.id);

  const [search, setSearch] = useState("");
  const [categoryFilter, setCategoryFilter] = useState("all");
  const [statusFilter, setStatusFilter] = useState("all");
  const [typeFilter, setTypeFilter] = useState("all");
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [expandedId, setExpandedId] = useState<string | null>(null);

  // Map real live backend transactions into FullTransaction format
  const mappedTransactions = useMemo<FullTransaction[]>(() => {
    return liveTransactions.map((tx) => ({
      id: tx.id,
      merchant: tx.description || (tx.type === "Deposit" ? "Wallet Deposit" : tx.type === "Withdraw" ? "Wallet Withdrawal" : "Peer-to-Peer Transfer"),
      logo: "/logos/stripe-com.png",
      category: tx.type === "Deposit" || tx.type === "TransferIn" ? "Income" : tx.type === "Withdraw" ? "Withdrawal" : "Transfer",
      amount: tx.type === "Deposit" || tx.type === "TransferIn" ? tx.amount : -Math.abs(tx.amount),
      date: new Date(tx.occurredAtUtc).toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" }),
      status: "completed",
      type: tx.type === "Deposit" || tx.type === "TransferIn" ? "income" : "expense",
      transactionId: tx.id.slice(0, 16).toUpperCase(),
      cardLast4: tx.walletId.slice(-4),
      merchantInfo: tx.reference || undefined,
      notes: `Balance After: ${tx.currency} ${tx.balanceAfter.toLocaleString()}`,
    }));
  }, [liveTransactions]);

  const categories = useMemo(() => {
    const cats = new Set(mappedTransactions.map((t) => t.category));
    return Array.from(cats).sort();
  }, [mappedTransactions]);

  const filteredData = useMemo(() => {
    let data = mappedTransactions;

    if (search) {
      const q = search.toLowerCase();
      data = data.filter(
        (t) =>
          t.merchant.toLowerCase().includes(q) ||
          t.transactionId.toLowerCase().includes(q) ||
          t.category.toLowerCase().includes(q)
      );
    }

    if (categoryFilter !== "all") {
      data = data.filter((t) => t.category === categoryFilter);
    }

    if (statusFilter !== "all") {
      data = data.filter((t) => t.status === statusFilter);
    }

    if (typeFilter !== "all") {
      data = data.filter((t) => t.type === typeFilter);
    }

    return data;
  }, [mappedTransactions, search, categoryFilter, statusFilter, typeFilter]);

  const handleSelectAll = (checked: boolean) => {
    if (checked) {
      setSelectedIds(new Set(filteredData.map((t) => t.id)));
    } else {
      setSelectedIds(new Set());
    }
  };

  const handleSelectOne = (id: string, checked: boolean) => {
    const next = new Set(selectedIds);
    if (checked) {
      next.add(id);
    } else {
      next.delete(id);
    }
    setSelectedIds(next);
  };

  const handleExport = () => {
    const toExport =
      selectedIds.size > 0
        ? filteredData.filter((t) => selectedIds.has(t.id))
        : filteredData;

    if (toExport.length === 0) {
      toast.error("No transactions to export");
      return;
    }

    const headers = "ID,Merchant,Category,Amount,Date,Status,Type\n";
    const rows = toExport
      .map(
        (t) =>
          `"${t.transactionId}","${t.merchant}","${t.category}",${t.amount},"${t.date}","${t.status}","${t.type}"`
      )
      .join("\n");
    const blob = new Blob([headers + rows], { type: "text/csv" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `transactions-${new Date().toISOString().slice(0, 10)}.csv`;
    a.click();
    URL.revokeObjectURL(url);
    toast.success(`Exported ${toExport.length} transactions as CSV`);
  };

  return (
    <div className="flex flex-1 flex-col gap-4 p-4">
      {/* Top action bar */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold tracking-tight">Ledger Transactions</h1>
          <p className="text-xs text-muted-foreground">
            Immutable Event Sourced Ledger for {activeWallet ? `${activeWallet.currency} Wallet (${activeWallet.id.slice(0, 8)}...)` : "Wallet"}
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => refetch()}
            disabled={isRefetching}
            className="h-8 gap-1 text-xs"
          >
            <RefreshCwIcon className={`size-3.5 ${isRefetching ? "animate-spin" : ""}`} />
            Refresh
          </Button>
          <TransactionActions
            selectedCount={selectedIds.size}
            onExport={handleExport}
            onClear={() => setSelectedIds(new Set())}
          />
        </div>
      </div>

      {/* Summary Cards */}
      <TransactionSummary transactions={mappedTransactions} />

      {/* Filters */}
      <TransactionFilters
        search={search}
        setSearch={setSearch}
        categoryFilter={categoryFilter}
        setCategoryFilter={setCategoryFilter}
        statusFilter={statusFilter}
        setStatusFilter={setStatusFilter}
        typeFilter={typeFilter}
        setTypeFilter={setTypeFilter}
        categories={categories}
      />

      {/* Table or Real Empty State */}
      {mappedTransactions.length === 0 && !isLoading ? (
        <div className="rounded-xl border border-dashed p-12 text-center bg-card/40 space-y-4">
          <div className="mx-auto size-12 rounded-full bg-primary/10 flex items-center justify-center text-primary">
            <ArrowDownLeftIcon className="size-6" />
          </div>
          <div>
            <h3 className="font-semibold text-base">No transactions recorded yet</h3>
            <p className="text-xs text-muted-foreground mt-1 max-w-sm mx-auto">
              Your wallet ledger is completely fresh. Make a real deposit or transfer to see your live audit trail.
            </p>
          </div>
          <Button
            size="sm"
            onClick={() => deposit(500, "Initial Ledger Deposit")}
            disabled={isMutating || !activeWallet}
            className="bg-emerald-600 hover:bg-emerald-700 text-white text-xs gap-1.5"
          >
            <PlusIcon className="size-3.5" />
            Make a $500 Deposit Now
          </Button>
        </div>
      ) : (
        <TransactionTable
          transactions={filteredData}
          selectedIds={selectedIds}
          setSelectedIds={setSelectedIds}
          expandedId={expandedId}
          setExpandedId={setExpandedId}
        />
      )}
    </div>
  );
}
