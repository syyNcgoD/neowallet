"use client";

import { useMemo, useState } from "react";
import { fullTransactions, type FullTransaction } from "@/data/seed";
import { TransactionSummary } from "@/components/transactions/transaction-summary";
import { TransactionFilters } from "@/components/transactions/transaction-filters";
import { TransactionTable } from "@/components/transactions/transaction-table";
import { TransactionActions } from "@/components/transactions/transaction-actions";
import { useAuth } from "@/contexts/auth-context";
import { useWalletTransactions } from "@/hooks/use-wallets";
import { toast } from "sonner";
import { RefreshCwIcon } from "lucide-react";
import { Button } from "@/components/ui/button";

export function TransactionsPageClient() {
  const { user } = useAuth();
  const { data: liveTransactions, isLoading, refetch, isRefetching } = useWalletTransactions(user?.id);

  const [search, setSearch] = useState("");
  const [categoryFilter, setCategoryFilter] = useState("all");
  const [statusFilter, setStatusFilter] = useState("all");
  const [typeFilter, setTypeFilter] = useState("all");
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [expandedId, setExpandedId] = useState<string | null>(null);

  // Map live backend transactions into FullTransaction format
  const mergedTransactions = useMemo(() => {
    if (!liveTransactions || liveTransactions.length === 0) {
      return fullTransactions;
    }

    const liveMapped: FullTransaction[] = liveTransactions.map((tx) => ({
      id: tx.id,
      merchant: tx.description || (tx.type === "Deposit" ? "Wallet Deposit" : tx.type === "Withdraw" ? "Wallet Withdrawal" : "Peer-to-Peer Transfer"),
      logo: "/logos/stripe-com.png",
      category: tx.type === "Deposit" ? "Income" : tx.type === "TransferIn" ? "Income" : "Technology",
      amount: tx.type === "Deposit" || tx.type === "TransferIn" ? tx.amount : -Math.abs(tx.amount),
      date: new Date(tx.occurredAtUtc).toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" }),
      status: "completed",
      type: tx.type === "Deposit" || tx.type === "TransferIn" ? "income" : "expense",
      transactionId: tx.id.slice(0, 16).toUpperCase(),
      cardLast4: tx.walletId.slice(-4),
      merchantInfo: tx.reference || undefined,
      notes: `Balance After: ${tx.currency} ${tx.balanceAfter.toLocaleString()}`,
    }));

    // Put live event-sourced transactions first, followed by seed historical transactions
    return [...liveMapped, ...fullTransactions];
  }, [liveTransactions]);

  const categories = useMemo(() => {
    const cats = new Set(mergedTransactions.map((t) => t.category));
    return Array.from(cats).sort();
  }, [mergedTransactions]);

  const filteredData = useMemo(() => {
    let data: FullTransaction[] = mergedTransactions;

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
  }, [mergedTransactions, search, categoryFilter, statusFilter, typeFilter]);

  function handleExport() {
    const toExport = selectedIds.size > 0
      ? mergedTransactions.filter((t) => selectedIds.has(t.id))
      : filteredData;

    const header = "Merchant,Transaction ID,Amount,Date,Status,Type,Category,Card,Notes";
    const rows = toExport.map(
      (t) =>
        `"${t.merchant}","${t.transactionId}",${t.amount},"${t.date}","${t.status}","${t.type}","${t.category}","${t.cardLast4 || ""}","${t.notes || ""}"`
    );
    const csv = [header, ...rows].join("\n");
    const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `neowallet_transactions_${Date.now()}.csv`;
    a.click();
    URL.revokeObjectURL(url);
    toast.success(`Exported ${toExport.length} transactions to CSV!`);
  }

  return (
    <div className="flex flex-col gap-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold tracking-tight">Transactions</h1>
        <Button
          variant="outline"
          size="sm"
          onClick={() => {
            refetch();
            toast.info("Refreshed transaction stream from Event Store");
          }}
          disabled={isRefetching}
          className="gap-2 text-xs"
        >
          <RefreshCwIcon className={`size-3.5 ${isRefetching ? "animate-spin" : ""}`} />
          Refresh Stream
        </Button>
      </div>

      <TransactionSummary transactions={filteredData} />

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

      <TransactionTable
        transactions={filteredData}
        selectedIds={selectedIds}
        setSelectedIds={setSelectedIds}
        expandedId={expandedId}
        setExpandedId={setExpandedId}
      />

      <TransactionActions
        selectedCount={selectedIds.size}
        onExport={handleExport}
        onClear={() => setSelectedIds(new Set())}
      />
    </div>
  );
}
