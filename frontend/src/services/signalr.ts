import * as signalR from '@microsoft/signalr';

const HUB_URL = import.meta.env.VITE_HUB_URL || 'https://localhost:5001/hubs/agent';

type MessageReceivedCallback = (data: {
  conversationId: string;
  agentId: string;
  content: string;
  timestamp: string;
}) => void;

type GroupMessageReceivedCallback = (data: {
  groupChatId: string;
  agentId: string;
  content: string;
  timestamp: string;
}) => void;

type AgentStatusChangedCallback = (data: {
  agentId: string;
  status: string;
  timestamp: string;
}) => void;

type ExecutionCompletedCallback = (data: {
  executionId: string;
  result: unknown;
  timestamp: string;
}) => void;

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private messageCallbacks: Set<MessageReceivedCallback> = new Set();
  private groupMessageCallbacks: Set<GroupMessageReceivedCallback> = new Set();
  private agentStatusCallbacks: Set<AgentStatusChangedCallback> = new Set();
  private executionCompletedCallbacks: Set<ExecutionCompletedCallback> = new Set();

  async connect(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        skipNegotiation: false,
        withCredentials: true,
        // For development with self-signed cert
        accessTokenFactory: () => localStorage.getItem('auth_token') || ''
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          // Exponential backoff
          if (retryContext.previousRetryCount === 0) {
            return 0;
          }
          return Math.min(10000, 1000 * Math.pow(2, retryContext.previousRetryCount));
        }
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Register event handlers
    this.connection.on('MessageReceived', (data) => {
      this.messageCallbacks.forEach((callback) => callback(data));
    });

    this.connection.on('GroupMessageReceived', (data) => {
      this.groupMessageCallbacks.forEach((callback) => callback(data));
    });

    this.connection.on('AgentStatusChanged', (data) => {
      this.agentStatusCallbacks.forEach((callback) => callback(data));
    });

    this.connection.on('ExecutionCompleted', (data) => {
      this.executionCompletedCallbacks.forEach((callback) => callback(data));
    });

    // Handle connection close
    this.connection.onclose((error) => {
      console.error('SignalR connection closed:', error);
    });

    // Handle reconnection
    this.connection.onreconnecting((error) => {
      console.log('SignalR reconnecting...', error);
    });

    this.connection.onreconnected((connectionId) => {
      console.log('SignalR reconnected:', connectionId);
    });

    try {
      await this.connection.start();
      console.log('SignalR connected');
    } catch (error) {
      console.error('Error starting SignalR connection:', error);
      throw error;
    }
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }

  getConnectionState(): signalR.HubConnectionState | null {
    return this.connection?.state ?? null;
  }

  isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }

  // Conversation methods
  async joinConversation(conversationId: string): Promise<void> {
    if (!this.connection) {
      throw new Error('Not connected to hub');
    }
    await this.connection.invoke('JoinConversation', conversationId);
  }

  async leaveConversation(conversationId: string): Promise<void> {
    if (!this.connection) {
      throw new Error('Not connected to hub');
    }
    await this.connection.invoke('LeaveConversation', conversationId);
  }

  // Group chat methods
  async joinGroupChat(groupChatId: string): Promise<void> {
    if (!this.connection) {
      throw new Error('Not connected to hub');
    }
    await this.connection.invoke('JoinGroupChat', groupChatId);
  }

  async leaveGroupChat(groupChatId: string): Promise<void> {
    if (!this.connection) {
      throw new Error('Not connected to hub');
    }
    await this.connection.invoke('LeaveGroupChat', groupChatId);
  }

  // Message methods (for server-side notifications)
  async notifyMessageReceived(conversationId: string, agentId: string, content: string): Promise<void> {
    if (!this.connection) {
      throw new Error('Not connected to hub');
    }
    await this.connection.invoke('SendMessageToConversation', conversationId, agentId, content);
  }

  async notifyGroupMessageReceived(groupChatId: string, agentId: string, content: string): Promise<void> {
    if (!this.connection) {
      throw new Error('Not connected to hub');
    }
    await this.connection.invoke('SendMessageToGroupChat', groupChatId, agentId, content);
  }

  // Event subscription methods
  onMessageReceived(callback: MessageReceivedCallback): () => void {
    this.messageCallbacks.add(callback);
    return () => this.messageCallbacks.delete(callback);
  }

  onGroupMessageReceived(callback: GroupMessageReceivedCallback): () => void {
    this.groupMessageCallbacks.add(callback);
    return () => this.groupMessageCallbacks.delete(callback);
  }

  onAgentStatusChanged(callback: AgentStatusChangedCallback): () => void {
    this.agentStatusCallbacks.add(callback);
    return () => this.agentStatusCallbacks.delete(callback);
  }

  onExecutionCompleted(callback: ExecutionCompletedCallback): () => void {
    this.executionCompletedCallbacks.add(callback);
    return () => this.executionCompletedCallbacks.delete(callback);
  }
}

export const signalrService = new SignalRService();
export default signalrService;
