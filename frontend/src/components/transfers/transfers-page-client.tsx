"use client";

import { useMemo, useState } from "react";
import { transferRecords, type TransferRecord } from "@/data/seed";
import { cn } from "@/lib/utils";
import { TransferStats } from "@/components/transfers/transfer-stats";
import { TransferList } from "@/components/transfers/transfer-list";
import { QuickSend } from "@/components/transfers/quick-send";
import { useAuth } from "@/contexts/auth-context";
import { useWalletTransactions } from "@/hooks/use-wallets";

type TabKey = "all" | "sent" | "received" | "scheduled";

const tabs: { key: TabKey; label: string }[] = [
  { key: "all", label: "All" },
  { key: "sent", label: "Sent" },
  { key: "received", label: "Received" },
  { key: "scheduled", label: "Scheduled" },
];

export function TransfersPageClient() {
  const { user } = useAuth();
  const { data: liveTransactions } = useWalletTransactions(user?.id);
  const [activeTab, setActiveTab] = useState<TabKey>("all");
  const [localTransfers, setLocalTransfers] = useState<TransferRecord[]>([]);

  const mergedTransfers = useMemo(() => {
    const liveTransfers: TransferRecord[] = (liveTransactions || [])
      .filter((tx) => tx.type === "TransferIn" || tx.type === "TransferOut")
      .map((tx) => ({
        id: tx.id,
        type: tx.type === "TransferOut" ? ("sent" as const) : ("received" as const),
        contactName: tx.description || (tx.type === "TransferOut" ? `Recipient (Wallet ${tx.relatedWalletId?.slice(-4) || "P2P"})` : `Sender (Wallet ${tx.relatedWalletId?.slice(-4) || "P2P"})`),
        contactAvatar: "/avatars/1.jpg",
        amount: Math.abs(tx.amount),
        date: new Date(tx.occurredAtUtc).toLocaleDateString("en-US", { month: "short", day: "2-digit", year: "numeric" }),
        status: "completed" as const,
        note: tx.reference || undefined,
      }));

    return [...localTransfers, ...liveTransfers, ...transferRecords];
  }, [liveTransactions, localTransfers]);

  const filtered = useMemo(() => {
    if (activeTab === "all") return mergedTransfers;
    return mergedTransfers.filter((t) => t.type === activeTab);
  }, [activeTab, mergedTransfers]);

  function handleCancel(id: string) {
    setLocalTransfers((prev) => prev.filter((t) => t.id !== id));
  }

  return (
    <div className="flex flex-col gap-4">
      {/* Stats */}
      <TransferStats transfers={mergedTransfers} />

      {/* Tab filter bar */}
      <div className="flex items-center gap-1 rounded-lg bg-muted p-1">
        {tabs.map((tab) => (
          <button
            key={tab.key}
            onClick={() => setActiveTab(tab.key)}
            className={cn(
              "rounded-md px-3 py-1.5 text-sm font-medium transition-colors",
              activeTab === tab.key
                ? "bg-background text-foreground shadow-sm"
                : "text-muted-foreground hover:text-foreground"
            )}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* Transfer list */}
      <TransferList transfers={filtered} onCancel={handleCancel} />

      {/* Quick send */}
      <QuickSend
        onSend={(record) =>
          setLocalTransfers((prev) => [record, ...prev])
        }
      />
    </div>
  );
}
