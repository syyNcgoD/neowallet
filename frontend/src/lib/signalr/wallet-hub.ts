import * as signalR from "@microsoft/signalr";
import { authService } from "@/lib/auth/auth-service";
import type { TransactionHistoryDto } from "@/types/api";

const SIGNALR_URL = process.env.NEXT_PUBLIC_SIGNALR_URL || "http://localhost:5000/hubs/wallet";

export interface BalanceChangedEvent {
  walletId: string;
  newBalance: number;
  currency: string;
}

export class WalletHubService {
  private connection: signalR.HubConnection | null = null;
  private isConnecting = false;

  public getConnection(): signalR.HubConnection {
    if (!this.connection) {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(SIGNALR_URL, {
          accessTokenFactory: () => authService.getAccessToken() || "",
          transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Warning)
        .build();
    }
    return this.connection;
  }

  public async start(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) return;
    if (this.isConnecting) return;

    this.isConnecting = true;
    try {
      const conn = this.getConnection();
      if (conn.state === signalR.HubConnectionState.Disconnected) {
        await conn.start();
      }
    } catch {
      // Background retry handled by SignalR
    } finally {
      this.isConnecting = false;
    }
  }

  public async stop(): Promise<void> {
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.connection.stop();
      } catch {
        // Ignore stop errors
      }
    }
  }

  public async joinWalletGroup(walletId: string): Promise<void> {
    const conn = this.getConnection();
    if (conn.state === signalR.HubConnectionState.Connected) {
      try {
        await conn.invoke("JoinWalletGroup", walletId);
      } catch {
        // Safe invoke
      }
    }
  }

  public async leaveWalletGroup(walletId: string): Promise<void> {
    const conn = this.getConnection();
    if (conn.state === signalR.HubConnectionState.Connected) {
      try {
        await conn.invoke("LeaveWalletGroup", walletId);
      } catch {
        // Safe invoke
      }
    }
  }

  public onBalanceChanged(callback: (e: BalanceChangedEvent) => void): () => void {
    const conn = this.getConnection();
    conn.on("BalanceChanged", callback);
    return () => conn.off("BalanceChanged", callback);
  }

  public onTransactionOccurred(callback: (tx: TransactionHistoryDto) => void): () => void {
    const conn = this.getConnection();
    conn.on("TransactionOccurred", callback);
    return () => conn.off("TransactionOccurred", callback);
  }
}

export const walletHubService = new WalletHubService();
