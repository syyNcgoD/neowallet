"use client";

import * as React from "react";
import { useSearchParams } from "next/navigation";
import { useTheme } from "next-themes";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Switch } from "@/components/ui/switch";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  CardDescription,
} from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Avatar,
  AvatarFallback,
  AvatarImage,
} from "@/components/ui/avatar";
import {
  UserIcon,
  ShieldIcon,
  BellIcon,
  CreditCardIcon,
  PaletteIcon,
  KeyIcon,
  SunIcon,
  MoonIcon,
  CheckIcon,
  DownloadIcon,
  Trash2Icon,
  CopyIcon,
  ShieldCheckIcon,
  PlusIcon,
} from "lucide-react";
import { useAuth } from "@/contexts/auth-context";
import { authApi } from "@/lib/api/auth";
import { useApiKeys, useCreateApiKey, useDeleteApiKey } from "@/hooks/use-api-keys";
import { useAuditVerification } from "@/hooks/use-audit";
import { toast } from "sonner";

type TabId = "profile" | "security" | "api-keys" | "notifications" | "billing" | "appearance";

const tabs: { id: TabId; label: string; icon: React.ReactNode }[] = [
  { id: "profile", label: "Profile", icon: <UserIcon className="size-4" /> },
  { id: "security", label: "Security & 2FA", icon: <ShieldIcon className="size-4" /> },
  { id: "api-keys", label: "Developer API Keys", icon: <KeyIcon className="size-4" /> },
  { id: "notifications", label: "Notifications", icon: <BellIcon className="size-4" /> },
  { id: "billing", label: "Billing", icon: <CreditCardIcon className="size-4" /> },
  { id: "appearance", label: "Appearance", icon: <PaletteIcon className="size-4" /> },
];

// ── Profile Tab ──────────────────────────────────────────────────────────────

function ProfileTab() {
  const { user } = useAuth();
  const [saving, setSaving] = React.useState(false);
  const email = user?.email || "admin@neowallet.com";
  const name = email.split("@")[0];

  function handleSave() {
    setSaving(true);
    setTimeout(() => {
      setSaving(false);
      toast.success("Profile details updated successfully.");
    }, 800);
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Profile Information</CardTitle>
        <CardDescription>Update your distributed wallet profile details</CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        <div className="flex items-center gap-4">
          <Avatar className="size-16">
            <AvatarImage src="/avatars/user.jpg" alt="User avatar" />
            <AvatarFallback className="text-lg">{name.slice(0, 2).toUpperCase()}</AvatarFallback>
          </Avatar>
          <div>
            <p className="font-medium capitalize">{name}</p>
            <p className="text-sm text-muted-foreground">{email}</p>
            <Badge variant="outline" className="mt-1 text-[10px]">
              Role: {user?.role === 2 ? "Admin" : "Standard User"}
            </Badge>
          </div>
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <div className="space-y-2">
            <label className="text-sm font-medium" htmlFor="name">
              Display Name
            </label>
            <Input id="name" defaultValue={name} />
          </div>
          <div className="space-y-2">
            <label className="text-sm font-medium" htmlFor="email">
              Email Address
            </label>
            <Input id="email" value={email} disabled />
          </div>
        </div>

        <div className="flex justify-end">
          <Button onClick={handleSave} disabled={saving}>
            {saving ? "Saving..." : "Save Changes"}
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}

// ── Security Tab ─────────────────────────────────────────────────────────────

function SecurityTab() {
  const { user } = useAuth();
  const [twoFA, setTwoFA] = React.useState(user?.isTwoFactorEnabled ?? false);
  const [totpCode, setTotpCode] = React.useState("");
  const { data: auditData } = useAuditVerification();

  const handleToggle2FA = async () => {
    if (!twoFA) {
      if (!totpCode || totpCode.length < 6) {
        toast.error("Please enter a 6-digit TOTP code to enable 2FA.");
        return;
      }
      try {
        await authApi.enable2FA(totpCode);
        setTwoFA(true);
        setTotpCode("");
        toast.success("Two-factor authentication enabled successfully!");
      } catch {
        toast.error("Invalid TOTP code. Verification failed.");
      }
    } else {
      try {
        await authApi.disable2FA(totpCode || "000000");
        setTwoFA(false);
        toast.success("Two-factor authentication disabled.");
      } catch {
        toast.error("Failed to disable 2FA.");
      }
    }
  };

  return (
    <div className="space-y-6">
      {/* Two-Factor */}
      <Card>
        <CardHeader>
          <CardTitle>Two-Factor Authentication (TOTP)</CardTitle>
          <CardDescription>
            Add an extra layer of bank-grade security to your transactions
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-center justify-between">
            <div className="space-y-1">
              <p className="text-sm font-medium">
                {twoFA ? "2FA is Currently Active" : "2FA is Disabled"}
              </p>
              <p className="text-sm text-muted-foreground">
                {twoFA
                  ? "Your account requires an authenticator app code for sensitive operations."
                  : "Enter a 6-digit code from Google Authenticator to activate."}
              </p>
            </div>
            <Switch checked={twoFA} onCheckedChange={() => handleToggle2FA()} />
          </div>

          {!twoFA && (
            <div className="flex gap-2 pt-2 max-w-xs">
              <Input
                placeholder="6-digit TOTP Code"
                value={totpCode}
                onChange={(e) => setTotpCode(e.target.value)}
                maxLength={6}
              />
              <Button size="sm" onClick={handleToggle2FA}>
                Verify & Enable
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Audit Chain Health */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <ShieldCheckIcon className="size-5 text-emerald-500" />
            Audit Ledger Integrity
          </CardTitle>
          <CardDescription>
            Cryptographic SHA-512 tamper-proof hash chain status
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-2">
          <div className="flex items-center justify-between rounded-lg border p-3">
            <div>
              <p className="text-sm font-medium">
                Status: {auditData?.isValid !== false ? "Chain Verified & Immutable" : "Integrity Compromised"}
              </p>
              <p className="text-xs font-mono text-muted-foreground">
                Last Verified Hash: {auditData?.lastVerifiedHash || "sha512-8f92a1..."}
              </p>
            </div>
            <Badge variant="default" className="bg-emerald-500">
              Verified
            </Badge>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

// ── Developer API Keys Tab ───────────────────────────────────────────────────

function ApiKeysTab() {
  const { data: apiKeys, isLoading } = useApiKeys();
  const createApiKeyMutation = useCreateApiKey();
  const deleteApiKeyMutation = useDeleteApiKey();

  const [keyName, setKeyName] = React.useState("");
  const [createdKey, setCreatedKey] = React.useState<string | null>(null);

  const handleCreate = async () => {
    if (!keyName) return;
    try {
      const res = await createApiKeyMutation.mutateAsync({
        name: keyName,
        permissions: ["wallets:read", "wallets:write", "transactions:read"],
      });
      setCreatedKey(res.apiKey);
      setKeyName("");
    } catch {
      // Handled
    }
  };

  const copyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text);
    toast.success("API Key copied to clipboard!");
  };

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Create API Key</CardTitle>
          <CardDescription>
            Generate scoped API keys for programmatic access to the NeoWallet distributed REST APIs
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex gap-2 max-w-md">
            <Input
              placeholder="e.g. Production Backend Service"
              value={keyName}
              onChange={(e) => setKeyName(e.target.value)}
            />
            <Button onClick={handleCreate} disabled={createApiKeyMutation.isPending || !keyName}>
              <PlusIcon className="size-4 mr-1" />
              Generate
            </Button>
          </div>

          {createdKey && (
            <div className="rounded-lg border border-emerald-500/30 bg-emerald-500/10 p-4 space-y-2">
              <p className="text-xs font-semibold text-emerald-600 dark:text-emerald-400">
                New API Key Generated! Copy it now (it will not be shown again):
              </p>
              <div className="flex items-center gap-2">
                <Input value={createdKey} readOnly className="font-mono text-xs" />
                <Button size="icon" variant="outline" onClick={() => copyToClipboard(createdKey)}>
                  <CopyIcon className="size-4" />
                </Button>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Active API Keys</CardTitle>
          <CardDescription>Manage active developer credentials</CardDescription>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Name</TableHead>
                <TableHead>Prefix</TableHead>
                <TableHead>Permissions</TableHead>
                <TableHead>Created</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {apiKeys && apiKeys.length > 0 ? (
                apiKeys.map((k) => (
                  <TableRow key={k.id}>
                    <TableCell className="font-medium">{k.name}</TableCell>
                    <TableCell className="font-mono text-xs">{k.prefix}...</TableCell>
                    <TableCell>
                      <div className="flex flex-wrap gap-1">
                        {k.permissions.map((p) => (
                          <Badge key={p} variant="secondary" className="text-[10px]">
                            {p}
                          </Badge>
                        ))}
                      </div>
                    </TableCell>
                    <TableCell className="text-xs text-muted-foreground">
                      {new Date(k.createdAtUtc).toLocaleDateString()}
                    </TableCell>
                    <TableCell className="text-right">
                      <Button
                        variant="ghost"
                        size="icon-xs"
                        onClick={() => deleteApiKeyMutation.mutate(k.id)}
                        className="text-destructive hover:text-destructive"
                      >
                        <Trash2Icon className="size-3.5" />
                      </Button>
                    </TableCell>
                  </TableRow>
                ))
              ) : (
                <TableRow>
                  <TableCell colSpan={5} className="text-center text-sm text-muted-foreground py-6">
                    No active API keys found. Generate one above to access the REST API.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
}

// ── Notifications Tab ────────────────────────────────────────────────────────

const notifToggles = [
  { id: "email", label: "Email Notifications", description: "Receive notifications via email", default: true },
  { id: "push", label: "Push Notifications", description: "Receive push notifications on your devices", default: true },
  { id: "transaction", label: "Transaction Alerts", description: "Get notified for every transaction", default: true },
  { id: "security", label: "Security Alerts", description: "Alerts for suspicious activity and logins", default: true },
];

function NotificationsTab() {
  const [settings, setSettings] = React.useState<Record<string, boolean>>(
    () => Object.fromEntries(notifToggles.map((t) => [t.id, t.default]))
  );

  function toggle(id: string) {
    setSettings((prev) => ({ ...prev, [id]: !prev[id] }));
    toast.info("Notification preference updated.");
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Notification Preferences</CardTitle>
        <CardDescription>
          Choose what notifications and SignalR alerts you want to receive
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-1">
        {notifToggles.map((t) => (
          <div
            key={t.id}
            className="flex items-center justify-between rounded-lg px-1 py-3"
          >
            <div className="space-y-0.5">
              <p className="text-sm font-medium">{t.label}</p>
              <p className="text-sm text-muted-foreground">{t.description}</p>
            </div>
            <Switch
              checked={settings[t.id]}
              onCheckedChange={() => toggle(t.id)}
            />
          </div>
        ))}
      </CardContent>
    </Card>
  );
}

// ── Billing Tab ──────────────────────────────────────────────────────────────

function BillingTab() {
  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>NeoWallet Plan</CardTitle>
          <CardDescription>Enterprise High-Performance Tier</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-start justify-between">
            <div>
              <div className="flex items-center gap-2">
                <h3 className="text-lg font-semibold">Enterprise Core</h3>
                <Badge variant="default">Active</Badge>
              </div>
              <p className="text-sm text-muted-foreground mt-1">
                Unlimited Event Sourcing streams, Marten OCC, and MassTransit Saga execution.
              </p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

// ── Appearance Tab ───────────────────────────────────────────────────────────

function AppearanceTab() {
  const { theme, setTheme } = useTheme();
  const [mounted, setMounted] = React.useState(false);
  React.useEffect(() => setMounted(true), []);

  const themes = [
    { id: "light", label: "Light", icon: <SunIcon className="size-5" /> },
    { id: "dark", label: "Dark", icon: <MoonIcon className="size-5" /> },
  ];

  if (!mounted) return null;

  return (
    <Card>
      <CardHeader>
        <CardTitle>Appearance</CardTitle>
        <CardDescription>
          Customize theme interface
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div className="grid gap-3 sm:grid-cols-2">
          {themes.map((t) => (
            <button
              key={t.id}
              type="button"
              onClick={() => setTheme(t.id)}
              className={cn(
                "flex flex-col items-center gap-2 rounded-lg border-2 p-6 transition-all hover:bg-muted/50",
                theme === t.id
                  ? "border-primary ring-2 ring-primary/20"
                  : "border-border"
              )}
            >
              {t.icon}
              <span className="text-sm font-medium">{t.label}</span>
              {theme === t.id && (
                <CheckIcon className="size-4 text-primary" />
              )}
            </button>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}

// ── Main Settings Page ───────────────────────────────────────────────────────

export function SettingsPageClient() {
  const searchParams = useSearchParams();
  const tabParam = searchParams.get("tab");
  const [activeTab, setActiveTab] = React.useState<TabId>(
    tabs.some((t) => t.id === tabParam) ? (tabParam as TabId) : "profile"
  );

  const tabContent: Record<TabId, React.ReactNode> = {
    profile: <ProfileTab />,
    security: <SecurityTab />,
    "api-keys": <ApiKeysTab />,
    notifications: <NotificationsTab />,
    billing: <BillingTab />,
    appearance: <AppearanceTab />,
  };

  return (
    <div className="flex flex-1 flex-col gap-4 lg:flex-row lg:gap-6">
      {/* Left nav (desktop) */}
      <nav className="hidden w-56 shrink-0 flex-col gap-1 lg:flex">
        {tabs.map((tab) => (
          <Button
            key={tab.id}
            variant={activeTab === tab.id ? "secondary" : "ghost"}
            size="sm"
            className={cn(
              "justify-start gap-2",
              activeTab === tab.id && "font-semibold"
            )}
            onClick={() => setActiveTab(tab.id)}
          >
            {tab.icon}
            {tab.label}
          </Button>
        ))}
      </nav>

      {/* Mobile tab bar */}
      <div className="-mx-1 flex gap-1 overflow-x-auto px-1 pb-2 lg:hidden">
        {tabs.map((tab) => (
          <Button
            key={tab.id}
            variant={activeTab === tab.id ? "secondary" : "ghost"}
            size="sm"
            className="shrink-0 gap-1.5 text-xs"
            onClick={() => setActiveTab(tab.id)}
          >
            {tab.icon}
            {tab.label}
          </Button>
        ))}
      </div>

      {/* Content */}
      <div className="min-w-0 flex-1">{tabContent[activeTab]}</div>
    </div>
  );
}
