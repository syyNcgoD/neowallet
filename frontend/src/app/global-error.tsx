"use client";

import { Button } from "@/components/ui/button";
import { AlertCircleIcon, RefreshCwIcon } from "lucide-react";

export default function GlobalError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <html>
      <body className="flex min-h-screen items-center justify-center bg-background text-foreground p-6">
        <div className="flex flex-col items-center justify-center text-center max-w-md">
          <div className="flex size-14 items-center justify-center rounded-2xl bg-destructive/10 text-destructive mb-4">
            <AlertCircleIcon className="size-7" />
          </div>
          <h2 className="text-2xl font-bold tracking-tight">Application Error</h2>
          <p className="mt-2 text-sm text-muted-foreground">
            A critical system error occurred. Please refresh or try again.
          </p>
          <Button onClick={() => reset()} className="mt-6 gap-2">
            <RefreshCwIcon className="size-4" />
            Reload Application
          </Button>
        </div>
      </body>
    </html>
  );
}
