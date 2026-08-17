"use client";

import { useState } from "react";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  SendIcon,
  LoaderCircleIcon,
  CheckCircle2Icon,
  ArrowRightIcon,
} from "lucide-react";
import { motion, AnimatePresence } from "motion/react";
import { useWallet } from "@/contexts/wallet-context";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

export function QuickTransfer() {
  const { activeWallet, wallets, transfer, isMutating } = useWallet();
  const [recipientWalletId, setRecipientWalletId] = useState("");
  const [amount, setAmount] = useState("100");
  const [description, setDescription] = useState("");
  const [isSuccess, setIsSuccess] = useState(false);

  // Filter other wallets owned by user for quick internal transfers
  const otherWallets = wallets.filter((w) => w.id !== activeWallet?.id);

  const handleSend = async () => {
    const num = parseFloat(amount);
    if (!recipientWalletId.trim()) {
      toast.error("Please enter a recipient Wallet ID.");
      return;
    }
    if (!num || num <= 0) {
      toast.error("Please enter a valid transfer amount.");
      return;
    }
    if (activeWallet && num > activeWallet.balance) {
      toast.error("Insufficient balance in active wallet.");
      return;
    }

    try {
      await transfer(recipientWalletId.trim(), num, description || "P2P Quick Transfer");
      setIsSuccess(true);
      setTimeout(() => {
        setIsSuccess(false);
        setRecipientWalletId("");
        setAmount("100");
        setDescription("");
      }, 2500);
    } catch {
      // Handled in context toast
    }
  };

  return (
    <Card className="h-full flex flex-col justify-between p-5 bg-card/60 backdrop-blur-sm border-border/40">
      <CardHeader className="p-0 pb-3 border-b border-border/40 flex flex-row items-center justify-between">
        <CardTitle className="text-sm font-semibold flex items-center gap-2">
          <SendIcon className="size-4 text-primary" />
          Live P2P Transfer (Saga)
        </CardTitle>
        <span className="text-[10px] font-medium text-muted-foreground">
          From: {activeWallet?.currency || "USD"} Wallet
        </span>
      </CardHeader>

      <CardContent className="p-0 py-4 space-y-3 flex-1 flex flex-col justify-center">
        {/* Quick select other user wallets */}
        {otherWallets.length > 0 && (
          <div className="space-y-1.5">
            <span className="text-[11px] text-muted-foreground font-medium">Transfer to your other wallet:</span>
            <div className="flex gap-1.5 flex-wrap">
              {otherWallets.map((w) => (
                <button
                  key={w.id}
                  onClick={() => setRecipientWalletId(w.id)}
                  className={cn(
                    "text-xs px-2.5 py-1 rounded-md border transition-all",
                    recipientWalletId === w.id
                      ? "border-primary bg-primary/10 text-primary font-semibold"
                      : "border-border/60 hover:bg-muted/50 text-muted-foreground"
                  )}
                >
                  {w.currency} Wallet ({w.id.slice(0, 6)}...)
                </button>
              ))}
            </div>
          </div>
        )}

        {/* Recipient Wallet ID Input */}
        <div className="space-y-1">
          <label className="text-xs font-medium text-muted-foreground">Recipient Wallet ID (UUID):</label>
          <Input
            value={recipientWalletId}
            onChange={(e) => setRecipientWalletId(e.target.value)}
            placeholder="e.g. 6d5c357c-6537-4323-bb36-..."
            className="text-xs font-mono"
          />
        </div>

        {/* Amount Input */}
        <div className="space-y-1">
          <div className="flex justify-between items-center">
            <label className="text-xs font-medium text-muted-foreground">Amount ({activeWallet?.currency || "USD"}):</label>
            <span className="text-[11px] text-muted-foreground">
              Avail: ${activeWallet?.balance.toLocaleString() || "0.00"}
            </span>
          </div>
          <Input
            type="number"
            min="1"
            step="any"
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            placeholder="Amount"
            className="text-sm font-semibold"
          />
        </div>
      </CardContent>

      <Button
        onClick={handleSend}
        disabled={isMutating || isSuccess || !activeWallet || (activeWallet.balance <= 0)}
        className="w-full font-medium h-9 text-xs gap-2"
      >
        {isMutating ? (
          <>
            <LoaderCircleIcon className="size-3.5 animate-spin" />
            Executing Saga State Machine...
          </>
        ) : isSuccess ? (
          <>
            <CheckCircle2Icon className="size-3.5 text-emerald-400" />
            Transfer Settled & Verified!
          </>
        ) : (
          <>
            <ArrowRightIcon className="size-3.5" />
            Send {activeWallet?.currency || "$"} {amount || "0"} Now
          </>
        )}
      </Button>
    </Card>
  );
}
