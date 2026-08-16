"use client";

import { useEffect } from "react";
import { Button } from "@/components/ui/button";
import { AlertTriangleIcon, RefreshCwIcon, HomeIcon } from "lucide-react";
import Link from "next/link";

export default function ErrorPage({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    // Log unexpected errors
    console.error("Dashboard error caught by boundary:", error);
  }, [error]);

  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center p-6 text-center">
      <div className="flex size-14 items-center justify-center rounded-2xl bg-destructive/10 text-destructive mb-4">
        <AlertTriangleIcon className="size-7" />
      </div>
      <h2 className="text-2xl font-bold tracking-tight">Something went wrong</h2>
      <p className="mt-2 max-w-md text-sm text-muted-foreground">
        {error.message || "An unexpected error occurred while communicating with the distributed ledger."}
      </p>

      <div className="mt-6 flex items-center gap-3">
        <Button onClick={() => reset()} className="gap-2">
          <RefreshCwIcon className="size-4" />
          Try Again
        </Button>
        <Button variant="outline" render={<Link href="/dashboard" />} className="gap-2">
          <HomeIcon className="size-4" />
          Go to Dashboard
        </Button>
      </div>
    </div>
  );
}
