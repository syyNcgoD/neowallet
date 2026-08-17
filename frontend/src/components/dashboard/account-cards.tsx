"use client";

import { useState, useEffect, useCallback } from "react";
import { accountCards, walletBalance } from "@/data/seed";
import {
  CreditCardIcon,
  PlusIcon,
  TrendingUpIcon,
  EuroIcon,
  BitcoinIcon,
  ChartLineIcon,
  NfcIcon,
  XIcon,
  CheckCircle2Icon,
  LoaderCircleIcon,
  LockIcon,
  UnlockIcon,
  ArrowDownLeftIcon,
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
import { useAuth } from "@/contexts/auth-context";
import { useWalletSummary, useDeposit, useLockWallet, useUnlockWallet, useCreateWallet } from "@/hooks/use-wallets";
import { v4 as uuidv4 } from "uuid";

type AddState = "idle" | "form" | "adding" | "success" | "deposit";

const newCardOptions = [
  { value: "USD", label: "USD Core Wallet", currency: "$", style: "bg-primary text-primary-foreground", icon: <ChartLineIcon className="size-5 opacity-30" />, chipColor: "bg-primary-foreground/20" },
  { value: "EUR", label: "Euro Eurozone Wallet", currency: "€", style: "bg-muted text-foreground", icon: <EuroIcon className="size-5 opacity-30" />, chipColor: "bg-foreground/10" },
  { value: "GBP", label: "British Pound Wallet", currency: "£", style: "bg-emerald-600 text-white", icon: <TrendingUpIcon className="size-5 opacity-30" />, chipColor: "bg-white/20" },
];

export function AccountCards() {
  const { user } = useAuth();
  const walletId = user?.id;
  const { data: walletSummary, isLoading: isSummaryLoading } = useWalletSummary(walletId);
  const depositMutation = useDeposit(walletId || "");
  const lockMutation = useLockWallet(walletId || "");
  const unlockMutation = useUnlockWallet(walletId || "");
  const createWalletMutation = useCreateWallet();

  const [cards, setCards] = useState([
    {
      id: "card-usd-primary",
      label: "USD Primary Wallet",
      balance: walletSummary ? walletSummary.balance.toLocaleString("en-US", { minimumFractionDigits: 2 }) : "84,765.00",
      currency: "$",
      variant: "primary" as const,
      style: "bg-primary text-primary-foreground",
      icon: <ChartLineIcon className="size-5 opacity-30" />,
      chipColor: "bg-primary-foreground/20",
      last4: "2026",
    },
    {
      ...accountCards[0],
      id: "card-eur-vault",
      style: "bg-muted text-foreground",
      icon: <EuroIcon className="size-5 opacity-30" />,
      chipColor: "bg-foreground/10",
      last4: "4589",
    },
    {
      ...accountCards[1],
      id: "card-crypto-vault",
      style: "bg-card text-card-foreground ring-1 ring-border",
      icon: <BitcoinIcon className="size-5 opacity-30" />,
      chipColor: "bg-foreground/10",
      last4: "7321",
    },
  ]);

  const [order, setOrder] = useState(() => [0, 1, 2]);
  const [addState, setAddState] = useState<AddState>("idle");
  const [newCardType, setNewCardType] = useState("USD");
  const [depositAmount, setDepositAmount] = useState("500.00");

  useEffect(() => {
    if (walletSummary) {
      setCards((prev) => [
        {
          ...prev[0],
          balance: walletSummary.balance.toLocaleString("en-US", { minimumFractionDigits: 2 }),
          label: `${walletSummary.currency} ${walletSummary.status === "Locked" ? "(Locked)" : "Wallet"}`,
        },
        ...prev.slice(1),
      ]);
    }
  }, [walletSummary]);

  const cycle = useCallback(() => {
    setOrder((prev) => {
      const next = [...prev];
      const front = next.pop()!;
      next.unshift(front);
      return next;
    })
  }, []);

  useEffect(() => {
    if (addState !== "idle") return;
    const id = setInterval(cycle, 3500);
    return () => clearInterval(id);
  }, [cycle, addState]);

  const handleDeposit = async () => {
    if (!walletId || !depositAmount || parseFloat(depositAmount) <= 0) return;
    setAddState("adding");
    try {
      await depositMutation.mutateAsync({
        data: {
          amount: parseFloat(depositAmount),
          currency: walletSummary?.currency || "USD",
          reference: `DEP-${Date.now()}`,
          description: "Quick Dashboard Top-up",
        },
        idempotencyKey: uuidv4(),
      });
      setAddState("success");
      setTimeout(() => setAddState("idle"), 1500);
    } catch {
      setAddState("idle");
    }
  };

  const toggleLock = async () => {
    if (!walletId) return;
    if (walletSummary?.status === "Locked") {
      await unlockMutation.mutateAsync("User requested unlock from dashboard");
    } else {
      await lockMutation.mutateAsync("User locked wallet from dashboard");
    }
  };

  const handleCreateWallet = async () => {
    if (!user?.id) return;
    setAddState("adding");
    try {
      await createWalletMutation.mutateAsync({
        ownerId: user.id,
        currency: newCardType,
      });
      setAddState("success");
      setTimeout(() => setAddState("idle"), 1500);
    } catch {
      setAddState("idle");
    }
  };

  const currentBalance = walletSummary
    ? walletSummary.balance
    : walletBalance.amount;

  return (
    <Card>
      <CardContent className="flex flex-col gap-5 pt-6">
        <AnimatePresence mode="wait">
          {addState === "idle" ? (
            <motion.div
              key="cards"
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
            >
              {/* Stacked cards */}
              <div className="relative h-[200px]">
                {order.map((cardIndex, stackPos) => {
                  const c = cards[cardIndex];
                  if (!c) return null;
                  const isFront = stackPos === order.length - 1;
                  const maxOffset = 48 / Math.max(order.length - 1, 1);
                  return (
                    <motion.button
                      key={c.id}
                      onClick={cycle}
                      layout
                      animate={{
                        y: stackPos * Math.min(maxOffset, 16),
                        scale: 1 - (order.length - 1 - stackPos) * (0.12 / Math.max(order.length - 1, 1)),
                        zIndex: stackPos,
                      }}
                      transition={{ type: "spring", stiffness: 400, damping: 28 }}
                      className={cn(
                        "absolute inset-x-0 flex h-[152px] cursor-pointer flex-col justify-between rounded-2xl px-5 py-4 text-left",
                        c.style,
                        isFront ? "shadow-xl" : "shadow-md"
                      )}
                    >
                      <div className="flex items-center justify-between">
                        <span className="text-sm font-semibold tracking-wide">{c.label}</span>
                        {c.icon}
                      </div>
                      <div className="flex items-center gap-2">
                        <div className={cn("h-7 w-10 rounded-md", c.chipColor)} />
                        <NfcIcon className="size-4 opacity-20" />
                      </div>
                      <div className="flex items-end justify-between">
                        <span className="font-mono text-[10px] tracking-widest opacity-40">
                          **** {c.last4}
                        </span>
                        <p className="text-xl font-bold tabular-nums tracking-tight">
                          {c.currency === "BTC"
                            ? `${c.balance} ${c.currency}`
                            : `${c.currency}${c.balance}`}
                        </p>
                      </div>
                    </motion.button>
                  );
                })}
              </div>
            </motion.div>
          ) : addState === "deposit" ? (
            <motion.div
              key="deposit-flow"
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -10 }}
              className="flex h-[200px] flex-col justify-between"
            >
              <div className="flex items-center justify-between">
                <p className="text-sm font-semibold">Quick Deposit to Wallet</p>
                <Button
                  variant="ghost"
                  size="icon"
                  className="size-7"
                  onClick={() => setAddState("idle")}
                >
                  <XIcon className="size-4" />
                </Button>
              </div>
              <div className="space-y-1.5">
                <label className="text-xs text-muted-foreground">Amount ({walletSummary?.currency || "USD"})</label>
                <Input
                  type="number"
                  value={depositAmount}
                  onChange={(e) => setDepositAmount(e.target.value)}
                  className="h-10 text-lg font-semibold tabular-nums"
                />
              </div>
              <Button
                className="h-10 gap-2 text-xs"
                onClick={handleDeposit}
                disabled={depositMutation.isPending}
              >
                <ArrowDownLeftIcon className="size-4" />
                Confirm Deposit
              </Button>
            </motion.div>
          ) : (
            <motion.div
              key="add-flow"
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -10 }}
              className="flex h-[200px] flex-col"
            >
              {addState === "success" ? (
                <div className="flex flex-1 flex-col items-center justify-center gap-2">
                  <motion.div
                    initial={{ scale: 0 }}
                    animate={{ scale: 1 }}
                    transition={{ type: "spring", stiffness: 300, damping: 20 }}
                  >
                    <CheckCircle2Icon className="size-10 text-emerald-500" />
                  </motion.div>
                  <p className="text-sm font-semibold">Operation Completed!</p>
                </div>
              ) : addState === "adding" ? (
                <div className="flex flex-1 flex-col items-center justify-center gap-3">
                  <LoaderCircleIcon className="size-8 animate-spin text-muted-foreground" />
                  <p className="text-sm text-muted-foreground">Processing with Event Store...</p>
                </div>
              ) : (
                <div className="flex flex-1 flex-col gap-3">
                  <div className="flex items-center justify-between">
                    <p className="text-sm font-semibold">Add Currency Wallet</p>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="size-7"
                      onClick={() => setAddState("idle")}
                    >
                      <XIcon className="size-4" />
                    </Button>
                  </div>
                  <Select value={newCardType} onValueChange={(v) => v && setNewCardType(v)}>
                    <SelectTrigger className="h-9 text-xs">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {newCardOptions.map((o) => (
                        <SelectItem key={o.value} value={o.value}>
                          {o.label} ({o.value})
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <Button className="h-9 gap-2 text-xs" onClick={handleCreateWallet}>
                    <PlusIcon className="size-3.5" />
                    Initialize Wallet Stream
                  </Button>
                </div>
              )}
            </motion.div>
          )}
        </AnimatePresence>

        {/* Quick action buttons: Deposit & Freeze */}
        <div className="flex items-center justify-between gap-2">
          <div className="flex items-center gap-1.5">
            <Button
              variant="outline"
              size="sm"
              className="h-8 gap-1.5 text-xs"
              onClick={() => setAddState("deposit")}
            >
              <ArrowDownLeftIcon className="size-3.5 text-emerald-600 dark:text-emerald-400" />
              Deposit
            </Button>
            <Button
              variant="outline"
              size="sm"
              className="h-8 gap-1.5 text-xs"
              onClick={toggleLock}
              disabled={lockMutation.isPending || unlockMutation.isPending}
            >
              {walletSummary?.status === "Locked" ? (
                <>
                  <UnlockIcon className="size-3.5 text-amber-500" />
                  Unlock
                </>
              ) : (
                <>
                  <LockIcon className="size-3.5 text-muted-foreground" />
                  Freeze
                </>
              )}
            </Button>
          </div>
          <Button
            variant="outline"
            size="icon"
            className="size-8 rounded-full"
            onClick={() => addState === "idle" && setAddState("form")}
            title="Add Currency Wallet"
          >
            <PlusIcon className="size-4" />
          </Button>
        </div>

        {/* Wallet balance */}
        <div className="space-y-1.5 border-t pt-5">
          <div className="flex items-center justify-between">
            <p className="text-xs font-medium text-muted-foreground">Live Total Balance</p>
            {walletSummary && (
              <span className="font-mono text-[10px] text-muted-foreground">
                v{walletSummary.version} OCC
              </span>
            )}
          </div>
          <p className="text-3xl font-bold tabular-nums tracking-tight">
            ${currentBalance.toLocaleString("en-US", { minimumFractionDigits: 2 })}
          </p>
          <div className="flex items-center gap-1.5 text-sm font-medium text-emerald-600 dark:text-emerald-400">
            <TrendingUpIcon className="size-4" />
            <span>+12.4% this month</span>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
