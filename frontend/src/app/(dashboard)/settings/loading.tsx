import { Skeleton } from "@/components/ui/skeleton";

export default function SettingsLoading() {
  return (
    <div className="flex flex-1 flex-col gap-4 lg:flex-row lg:gap-6">
      <div className="hidden w-52 shrink-0 flex-col gap-2 lg:flex">
        {Array.from({ length: 6 }).map((_, i) => (
          <Skeleton key={i} className="h-9 w-full rounded-md" />
        ))}
      </div>
      <div className="min-w-0 flex-1 space-y-4">
        <Skeleton className="h-[360px] w-full rounded-xl" />
      </div>
    </div>
  );
}
