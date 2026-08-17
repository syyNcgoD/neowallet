"use client";

import { useState } from "react";
import {
  PlusIcon,
  TrendingUpIcon,
  EuroIcon,
  ChartLineIcon,
  NfcIcon,
  XIcon,
  CheckCircle2Icon,
  LoaderCircleIcon,
  LockIcon,
  UnlockIcon,
  ArrowDownLeftIcon,
  ArrowUpRightIcon,
  WalletIcon,
  RefreshCwIcon,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent } from "@/components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { motion, AnimatePresence } from "motion/react";
import { cn } from "@/lib/utils";
import { useWallet } from "@/contexts/wallet-context";

type ModalState = "idle" | "create-wallet" | "deposit" | "withdraw" | "lock";

const CURRENCY_OPTIONS = [
  { value: "USD", label: "USD Core Wallet", symbol: "$", style: "bg-primary text-primary-foreground", icon: <ChartLineIcon className="size-5 opacity-30" /> },
  { value: "EUR", label: "EUR European Vault", symbol: "€", style: "bg-zinc-900 text-zinc-100 border border-zinc-700", icon: <EuroIcon className="size-5 opacity-30" /> },
  { value: "GBP", label: "GBP British Sterling", symbol: "£", style: "bg-emerald-800 text-white", icon: <TrendingUpIcon className="size-5 opacity-30" /> },
];

export function AccountCards() {
  const {
    wallets,
    activeWallet,
    setActiveWalletId,
    isLoading,
    isMutating,
    createWallet,
    deposit,
    withdraw,
    toggleLock,
    refresh,
  } = useWallet();

  const [modal, setModal] = useState<ModalState>("idle");
  const [selectedCurrency, setSelectedCurrency] = useState("USD");
  const [amountInput, setAmountInput] = useState("500");
  const [lockReason, setLockReason] = useState("User manual security freeze");

  const handleCreateWallet = async () => {
    await createWallet(selectedCurrency);
    setModal("idle");
  };

  const handleDeposit = async () => {
    const num = parseFloat(amountInput);
    if (!num || num <= 0) return;
    await deposit(num, `Direct Deposit into ${activeWallet?.currency} Wallet`);
    setModal("idle");
    setAmountInput("500");
  };

  const handleWithdraw = async () => {
    const num = parseFloat(amountInput);
    if (!num || num <= 0) return;
    await withdraw(num, `Direct Withdrawal from ${activeWallet?.currency} Wallet`);
    setModal("idle");
    setAmountInput("100");
  };

  const handleToggleLock = async () => {
    await toggleLock(lockReason);
    setModal("idle");
  };

  // If loading or no wallets
  if (isLoading) {
    return (
      <Card className="h-full flex items-center justify-center p-8 bg-card/60 backdrop-blur-sm border-border/40">
        <div className="flex flex-col items-center gap-3 text-muted-foreground">
          <LoaderCircleIcon className="size-8 animate-spin text-primary" />
          <p className="text-sm font-medium">Connecting to Event Store...</p>
        </div>
      </Card>
    );
  }

  // If user has 0 wallets yet
  if (wallets.length === 0) {
    return (
      <Card className="h-full flex flex-col justify-between p-6 bg-gradient-to-br from-card/80 to-card border-dashed border-primary/30">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <WalletIcon className="size-5 text-primary" />
            <span className="font-semibold text-sm">Real Multi-Currency Ledger</span>
          </div>
          <Button variant="ghost" size="sm" onClick={refresh} className="h-8 w-8 p-0">
            <RefreshCwIcon className="size-4" />
          </Button>
        </div>

        <div className="my-6 text-center">
          <div className="mx-auto size-12 rounded-full bg-primary/10 flex items-center justify-center mb-3">
            <PlusIcon className="size-6 text-primary" />
          </div>
          <h3 className="font-semibold text-base">No active wallets found</h3>
          <p className="text-xs text-muted-foreground mt-1 max-w-[260px] mx-auto">
            Create your first event-sourced multi-currency wallet to start depositing and transferring real funds.
          </p>
        </div>

        <Button
          onClick={() => createWallet("USD")}
          disabled={isMutating}
          className="w-full font-medium"
        >
          {isMutating ? <LoaderCircleIcon className="size-4 animate-spin mr-2" /> : <PlusIcon className="size-4 mr-2" />}
          Create Primary USD Wallet
        </Button>
      </Card>
    );
  }

  const isLocked = String(activeWallet?.status) === "2" || String(activeWallet?.status).toLowerCase() === "locked";

  return (
    <div className="flex flex-col gap-4">
      {/* Wallet Stack Card */}
      <Card className="relative overflow-hidden bg-card/60 backdrop-blur-sm border-border/40 p-5">
        <div className="flex items-center justify-between pb-3 border-b border-border/40">
          <div className="flex items-center gap-2">
            <span className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">Active Ledger</span>
            <span className={cn(
              "px-2 py-0.5 rounded-full text-[10px] font-semibold",
              isLocked ? "bg-rose-500/20 text-rose-500 border border-rose-500/30" : "bg-emerald-500/20 text-emerald-500 border border-emerald-500/30"
            )}>
              {isLocked ? "FROZEN" : "ACTIVE"} (v{activeWallet?.version || 1})
            </span>
          </div>

          <div className="flex items-center gap-1.5">
            <Button
              variant="outline"
              size="sm"
              onClick={() => setModal("create-wallet")}
              className="h-7 text-xs px-2.5 gap-1"
            >
              <PlusIcon className="size-3" />
              New Wallet
            </Button>
            <Button
              variant="ghost"
              size="sm"
              onClick={refresh}
              className="h-7 w-7 p-0 text-muted-foreground"
            >
              <RefreshCwIcon className={cn("size-3.5", isMutating && "animate-spin")} />
            </Button>
          </div>
        </div>

        {/* Card Body */}
        <div className="pt-4 pb-2">
          {/* Wallet Selector Tabs */}
          {wallets.length > 1 && (
            <div className="flex gap-2 mb-4 overflow-x-auto pb-1">
              {wallets.map((w) => (
                <button
                  key={w.id}
                  onClick={() => setActiveWalletId(w.id)}
                  className={cn(
                    "px-3 py-1 rounded-lg text-xs font-medium transition-all whitespace-nowrap",
                    w.id === activeWallet?.id
                      ? "bg-primary text-primary-foreground shadow-sm"
                      : "bg-muted/60 text-muted-foreground hover:bg-muted"
                  )}
                >
                  {w.currency} Wallet (${w.balance.toLocaleString()})
                </button>
              ))}
            </div>
          )}

          {/* Active Balance Display */}
          <div className="space-y-1">
            <div className="text-xs text-muted-foreground font-medium">
              {activeWallet?.currency === "USD" ? "United States Dollar" : activeWallet?.currency === "EUR" ? "Euro" : "British Pound"} (ID: {activeWallet?.id.slice(0, 8)}...)
            </div>
            <div className="text-3xl font-bold tracking-tight">
              {activeWallet?.currency === "USD" ? "$" : activeWallet?.currency === "EUR" ? "€" : "£"}
              {activeWallet?.balance.toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
            </div>
          </div>
        </div>

        {/* Quick Financial Actions */}
        <div className="grid grid-cols-3 gap-2 pt-4 border-t border-border/40 mt-3">
          <Button
            size="sm"
            onClick={() => setModal("deposit")}
            disabled={isLocked || isMutating}
            className="h-9 gap-1.5 font-medium text-xs bg-emerald-600 hover:bg-emerald-700 text-white"
          >
            <ArrowDownLeftIcon className="size-3.5" />
            Deposit
          </Button>

          <Button
            size="sm"
            variant="outline"
            onClick={() => setModal("withdraw")}
            disabled={isLocked || isMutating || (activeWallet?.balance || 0) <= 0}
            className="h-9 gap-1.5 font-medium text-xs"
          >
            <ArrowUpRightIcon className="size-3.5" />
            Withdraw
          </Button>

          <Button
            size="sm"
            variant={isLocked ? "destructive" : "secondary"}
            onClick={() => setModal("lock")}
            disabled={isMutating}
            className="h-9 gap-1.5 font-medium text-xs"
          >
            {isLocked ? <UnlockIcon className="size-3.5" /> : <LockIcon className="size-3.5" />}
            {isLocked ? "Unfreeze" : "Freeze"}
          </Button>
        </div>
      </Card>

      {/* Action Modals */}
      <AnimatePresence>
        {modal !== "idle" && (
          <div className="fixed inset-0 z-50 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4">
            <motion.div
              initial={{ opacity: 0, scale: 0.95, y: 10 }}
              animate={{ opacity: 1, scale: 1, y: 0 }}
              exit={{ opacity: 0, scale: 0.95, y: 10 }}
              className="w-full max-w-md bg-card border border-border rounded-xl shadow-2xl overflow-hidden p-6"
            >
              {/* Modal Header */}
              <div className="flex items-center justify-between pb-4 border-b border-border/40">
                <h3 className="font-semibold text-base">
                  {modal === "create-wallet" && "Create New Multi-Currency Wallet"}
                  {modal === "deposit" && `Deposit Funds into ${activeWallet?.currency} Wallet`}
                  {modal === "withdraw" && `Withdraw Funds from ${activeWallet?.currency} Wallet`}
                  {modal === "lock" && (isLocked ? "Unlock / Unfreeze Wallet" : "Freeze / Lock Wallet")}
                </h3>
                <Button variant="ghost" size="sm" onClick={() => setModal("idle")} className="h-7 w-7 p-0">
                  <XIcon className="size-4" />
                </Button>
              </div>

              {/* Modal Content */}
              <div className="py-4 space-y-4">
                {modal === "create-wallet" && (
                  <>
                    <p className="text-xs text-muted-foreground">
                      Choose currency for your new event-sourced ledger stream:
                    </p>
                    <div className="grid grid-cols-3 gap-2">
                      {CURRENCY_OPTIONS.map((c) => (
                        <button
                          key={c.value}
                          onClick={() => setSelectedCurrency(c.value)}
                          className={cn(
                            "p-3 rounded-lg border text-center transition-all",
                            selectedCurrency === c.value
                              ? "border-primary bg-primary/10 text-primary font-semibold"
                              : "border-border/60 hover:bg-muted/50 text-muted-foreground"
                          )}
                        >
                          <div className="text-lg">{c.symbol}</div>
                          <div className="text-xs mt-1">{c.value}</div>
                        </button>
                      ))}
                    </div>
                  </>
                )}

                {(modal === "deposit" || modal === "withdraw") && (
                  <>
                    <div className="space-y-2">
                      <label className="text-xs font-medium text-muted-foreground">Amount ({activeWallet?.currency}):</label>
                      <Input
                        type="number"
                        min="1"
                        step="any"
                        value={amountInput}
                        onChange={(e) => setAmountInput(e.target.value)}
                        placeholder="e.g. 500"
                        className="text-lg font-semibold"
                      />
                    </div>

                    {modal === "deposit" && (
                      <div className="flex gap-2">
                        {["100", "500", "1000", "5000"].map((preset) => (
                          <Button
                            key={preset}
                            variant="outline"
                            size="sm"
                            onClick={() => setAmountInput(preset)}
                            className="flex-1 text-xs"
                          >
                            +${preset}
                          </Button>
                        ))}
                      </div>
                    )}
                  </>
                )}

                {modal === "lock" && (
                  <div className="space-y-2">
                    <p className="text-xs text-muted-foreground">
                      {isLocked
                        ? "Are you sure you want to unfreeze this wallet? Normal transactions will resume immediately."
                        : "Freezing this wallet will immediately block all withdrawals and transfers until unfreezed."}
                    </p>
                    <Input
                      value={lockReason}
                      onChange={(e) => setLockReason(e.target.value)}
                      placeholder="Reason for audit log..."
                      className="text-xs"
                    />
                  </div>
                )}
              </div>

              {/* Modal Footer */}
              <div className="flex justify-end gap-2 pt-4 border-t border-border/40">
                <Button variant="outline" size="sm" onClick={() => setModal("idle")}>
                  Cancel
                </Button>

                {modal === "create-wallet" && (
                  <Button size="sm" onClick={handleCreateWallet} disabled={isMutating}>
                    {isMutating ? <LoaderCircleIcon className="size-3.5 animate-spin mr-1.5" /> : null}
                    Create Wallet
                  </Button>
                )}

                {modal === "deposit" && (
                  <Button size="sm" onClick={handleDeposit} disabled={isMutating} className="bg-emerald-600 hover:bg-emerald-700 text-white">
                    {isMutating ? <LoaderCircleIcon className="size-3.5 animate-spin mr-1.5" /> : null}
                    Confirm Deposit
                  </Button>
                )}

                {modal === "withdraw" && (
                  <Button size="sm" onClick={handleWithdraw} disabled={isMutating}>
                    {isMutating ? <LoaderCircleIcon className="size-3.5 animate-spin mr-1.5" /> : null}
                    Confirm Withdrawal
                  </Button>
                )}

                {modal === "lock" && (
                  <Button
                    size="sm"
                    variant={isLocked ? "default" : "destructive"}
                    onClick={handleToggleLock}
                    disabled={isMutating}
                  >
                    {isMutating ? <LoaderCircleIcon className="size-3.5 animate-spin mr-1.5" /> : null}
                    {isLocked ? "Unfreeze Now" : "Freeze Now"}
                  </Button>
                )}
              </div>
            </motion.div>
          </div>
        )}
      </AnimatePresence>
    </div>
  );
}
