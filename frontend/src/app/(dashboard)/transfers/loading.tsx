import { Skeleton } from "@/components/ui/skeleton";

export default function TransfersLoading() {
  return (
    <div className="flex flex-col gap-4">
      <div className="grid gap-4 sm:grid-cols-3">
        <Skeleton className="h-28 rounded-xl" />
        <Skeleton className="h-28 rounded-xl" />
        <Skeleton className="h-28 rounded-xl" />
      </div>
      <Skeleton className="h-10 w-64 rounded-lg" />
      <Skeleton className="h-[300px] w-full rounded-xl" />
      <Skeleton className="h-[140px] w-full rounded-xl" />
    </div>
  );
}
