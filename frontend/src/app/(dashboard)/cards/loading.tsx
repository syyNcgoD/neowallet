import { Skeleton } from "@/components/ui/skeleton";

export default function CardsLoading() {
  return (
    <div className="space-y-6">
      <div className="grid gap-6 lg:grid-cols-12">
        <div className="lg:col-span-7 flex justify-center">
          <Skeleton className="h-[240px] w-[380px] rounded-2xl" />
        </div>
        <div className="lg:col-span-5">
          <Skeleton className="h-[240px] w-full rounded-xl" />
        </div>
      </div>
      <div className="grid gap-6 lg:grid-cols-12">
        <div className="lg:col-span-4">
          <Skeleton className="h-[280px] w-full rounded-xl" />
        </div>
        <div className="lg:col-span-8">
          <Skeleton className="h-[280px] w-full rounded-xl" />
        </div>
      </div>
    </div>
  );
}
