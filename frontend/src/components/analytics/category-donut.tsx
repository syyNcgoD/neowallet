"use client";

import { useMemo, useState } from "react";
import { PieChart, Pie, Cell } from "recharts";
import { AnimatePresence, motion } from "motion/react";
import { ArrowLeftIcon } from "lucide-react";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  type ChartConfig,
} from "@/components/ui/chart";
import { categoryBreakdowns } from "@/data/seed";
import { useAuth } from "@/contexts/auth-context";
import { useWalletTransactions } from "@/hooks/use-wallets";

const SUBCATEGORY_COLORS = [
  "var(--color-chart-1)",
  "var(--color-chart-2)",
  "var(--color-chart-3)",
  "var(--color-chart-4)",
  "var(--color-chart-5)",
];

export function CategoryDonut() {
  const { user } = useAuth();
  const { data: liveTransactions } = useWalletTransactions(user?.id);
  const [selected, setSelected] = useState<string | null>(null);

  const dynamicBreakdowns = useMemo(() => {
    if (!liveTransactions || liveTransactions.length === 0) {
      return categoryBreakdowns;
    }

    // Aggregate expenses by category/type
    const categoryMap: Record<string, number> = {};
    liveTransactions.forEach((tx) => {
      const cat = tx.type === "Deposit" ? "Income" : tx.type === "Withdraw" ? "Bills" : "Shopping";
      categoryMap[cat] = (categoryMap[cat] || 0) + Math.abs(tx.amount);
    });

    const colors = [
      "var(--color-chart-1)",
      "var(--color-chart-2)",
      "var(--color-chart-3)",
      "var(--color-chart-4)",
      "var(--color-chart-5)",
    ];

    const entries = Object.entries(categoryMap);
    if (entries.length === 0) return categoryBreakdowns;

    return entries.map(([cat, amt], idx) => ({
      category: cat,
      amount: amt,
      color: colors[idx % colors.length],
      subcategories: [
        { name: "Direct Ledger", amount: amt },
      ],
    }));
  }, [liveTransactions]);

  const total = useMemo(
    () => dynamicBreakdowns.reduce((s, c) => s + c.amount, 0),
    [dynamicBreakdowns]
  );

  const selectedCategory = useMemo(
    () => dynamicBreakdowns.find((c) => c.category === selected) ?? null,
    [dynamicBreakdowns, selected]
  );

  const chartConfig = useMemo<ChartConfig>(() => {
    if (selectedCategory) {
      const config: ChartConfig = {};
      selectedCategory.subcategories.forEach((sub, i) => {
        config[sub.name] = {
          label: sub.name,
          color: SUBCATEGORY_COLORS[i % SUBCATEGORY_COLORS.length],
        };
      });
      return config;
    }
    const config: ChartConfig = {};
    dynamicBreakdowns.forEach((c) => {
      config[c.category] = {
        label: c.category,
        color: c.color,
      };
    });
    return config;
  }, [selectedCategory, dynamicBreakdowns]);

  const pieData = useMemo(() => {
    if (selectedCategory) {
      return selectedCategory.subcategories.map((sub, i) => ({
        name: sub.name,
        value: sub.amount,
        fill: SUBCATEGORY_COLORS[i % SUBCATEGORY_COLORS.length],
      }));
    }
    return dynamicBreakdowns.map((c) => ({
      name: c.category,
      value: c.amount,
      fill: c.color,
    }));
  }, [selectedCategory, dynamicBreakdowns]);

  const centerAmount = selectedCategory ? selectedCategory.amount : total;

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle>
            {selectedCategory
              ? selectedCategory.category
              : "Spending by Category"}
          </CardTitle>
          {selectedCategory && (
            <Button
              variant="ghost"
              size="sm"
              className="h-7 gap-1 text-xs"
              onClick={() => setSelected(null)}
            >
              <ArrowLeftIcon className="size-3" />
              Back to all
            </Button>
          )}
        </div>
      </CardHeader>
      <CardContent>
        <div className="flex flex-col items-center gap-4 sm:flex-row">
          <div className="relative flex aspect-square h-[220px] items-center justify-center">
            <ChartContainer config={chartConfig} className="size-full">
              <PieChart>
                <ChartTooltip
                  content={
                    <ChartTooltipContent
                      formatter={(value) =>
                        `$${Number(value).toLocaleString()}`
                      }
                    />
                  }
                />
                <Pie
                  data={pieData}
                  dataKey="value"
                  nameKey="name"
                  innerRadius={60}
                  outerRadius={85}
                  paddingAngle={3}
                  strokeWidth={2}
                  stroke="var(--color-background)"
                  onClick={(_, idx) => {
                    if (!selectedCategory) {
                      setSelected(dynamicBreakdowns[idx].category);
                    }
                  }}
                  className={!selectedCategory ? "cursor-pointer" : ""}
                >
                  {pieData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={entry.fill} />
                  ))}
                </Pie>
              </PieChart>
            </ChartContainer>
            {/* Center label */}
            <div className="pointer-events-none absolute flex flex-col items-center justify-center text-center">
              <AnimatePresence mode="wait">
                <motion.div
                  key={centerAmount}
                  initial={{ opacity: 0, scale: 0.8 }}
                  animate={{ opacity: 1, scale: 1 }}
                  exit={{ opacity: 0, scale: 0.8 }}
                  transition={{ duration: 0.2 }}
                >
                  <p className="text-xl font-bold tabular-nums">
                    ${centerAmount.toLocaleString()}
                  </p>
                  <p className="text-[11px] text-muted-foreground">
                    {selectedCategory ? "in category" : "Total"}
                  </p>
                </motion.div>
              </AnimatePresence>
            </div>
          </div>

          {/* Legend */}
          <div className="flex flex-1 flex-col gap-2">
            {(selectedCategory
              ? selectedCategory.subcategories
              : dynamicBreakdowns
            ).map((item, i) => {
              const name = "name" in item ? item.name : item.category;
              const color =
                "color" in item
                  ? item.color
                  : SUBCATEGORY_COLORS[i % SUBCATEGORY_COLORS.length];
              const pct = total > 0 ? Math.round((item.amount / total) * 100) : 0;
              return (
                <div
                  key={name}
                  className="flex items-center justify-between text-xs"
                >
                  <div className="flex items-center gap-2">
                    <div
                      className="size-2.5 rounded-full"
                      style={{ backgroundColor: color }}
                    />
                    <span className="text-muted-foreground">{name}</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="font-medium tabular-nums">
                      ${item.amount.toLocaleString()}
                    </span>
                    <span className="w-8 text-right text-muted-foreground tabular-nums">
                      {pct}%
                    </span>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
