/**
 * Agent type definitions
 */
export interface Agent {
  id: string;
  name: string;
  type: AgentType;
  systemMessage: string;
  modelConfig: ModelConfig;
  status: AgentStatus;
  metadata: Record<string, string>;
  createdAt: string;
  updatedAt?: string;
  lastActivatedAt?: string;
}

export enum AgentType {
  System = 'System',
  User = 'User',
  Assistant = 'Assistant',
  Tool = 'Tool',
  Supervisor = 'Supervisor',
  Router = 'Router'
}

export enum AgentStatus {
  Active = 'Active',
  Inactive = 'Inactive',
  Error = 'Error',
  Busy = 'Busy',
  Terminated = 'Terminated'
}

export interface ModelConfig {
  provider: string;
  model: string;
  apiKey: string;
  baseUrl?: string;
  temperature: number;
  maxTokens: number;
  seed?: number;
}

/**
 * Conversation type definitions
 */
export interface Conversation {
  id: string;
  name: string;
  participantIds: string[];
  type: ConversationType;
  createdAt: string;
  updatedAt?: string;
  metadata: Record<string, string>;
}

export enum ConversationType {
  OneToOne = 'OneToOne',
  GroupChat = 'GroupChat',
  Broadcast = 'Broadcast',
  RoundRobin = 'RoundRobin',
  Supervised = 'Supervised'
}

export interface Message {
  id: string;
  conversationId: string;
  sourceAgentId: string;
  targetAgentId?: string;
  content: string;
  type: MessageType;
  timestamp: string;
  metadata: Record<string, string>;
}

export enum MessageType {
  Text = 'Text',
  Code = 'Code',
  Image = 'Image',
  ToolCall = 'ToolCall',
  ToolResult = 'ToolResult',
  System = 'System'
}

/**
 * Code execution type definitions
 */
export interface CodeExecution {
  id: string;
  conversationId?: string;
  requestingAgentId?: string;
  language: CodeLanguage;
  code: string;
  status: ExecutionStatus;
  result?: ExecutionResult;
  createdAt: string;
  startedAt?: string;
  completedAt?: string;
  timeoutSeconds: number;
  resourceLimits: ResourceLimits;
}

export enum CodeLanguage {
  Python = 'Python',
  JavaScript = 'JavaScript',
  TypeScript = 'TypeScript',
  CSharp = 'CSharp',
  Bash = 'Bash',
  PowerShell = 'PowerShell'
}

export enum ExecutionStatus {
  Pending = 'Pending',
  Running = 'Running',
  Completed = 'Completed',
  Failed = 'Failed',
  Timeout = 'Timeout',
  Cancelled = 'Cancelled'
}

export interface ExecutionResult {
  stdout: string;
  stderr: string;
  exitCode: number;
  duration: string;
  resourceUsage: ResourceUsage;
  error?: string;
}

export interface ResourceUsage {
  memoryUsedMB: number;
  cpuTimeSeconds: number;
  processId: number;
}

export interface ResourceLimits {
  maxMemoryMB: number;
  maxCpuSeconds: number;
  maxOutputLength: number;
}

/**
 * Group chat type definitions
 */
export interface GroupChat {
  id: string;
  name: string;
  description: string;
  participantIds: string[];
  pattern: GroupChatPattern;
  rules: ChatRule[];
  createdAt: string;
  updatedAt?: string;
  metadata: Record<string, string>;
}

export enum GroupChatPattern {
  RoundRobin = 'RoundRobin',
  Broadcast = 'Broadcast',
  Supervised = 'Supervised',
  Router = 'Router',
  FreeForAll = 'FreeForAll',
  SpeakerSelection = 'SpeakerSelection'
}

export interface ChatRule {
  id: string;
  name: string;
  condition: RuleCondition;
  action: RuleAction;
}

export interface RuleCondition {
  senderAgentId?: string;
  targetAgentId?: string;
  keyword?: string;
  messageType?: string;
}

export interface RuleAction {
  type: ActionType;
  targetAgentId?: string;
  targetAgentIds?: string[];
  shouldBroadcast: boolean;
}

export enum ActionType {
  RouteToAgent = 'RouteToAgent',
  Broadcast = 'Broadcast',
  Skip = 'Skip',
  Terminate = 'Terminate'
}

export interface GroupChatMessage {
  id: string;
  groupChatId: string;
  sourceAgentId: string;
  targetAgentIds: string[];
  content: string;
  timestamp: string;
  sequenceNumber: number;
}

/**
 * AutoGen orchestration types
 */
export interface AgentDefinition {
  id: string;
  name: string;
  type: AgentType;
  systemMessage: string;
  modelConfiguration: ModelConfiguration;
  capabilities: Record<string, string>;
}

export interface TeamConfiguration {
  name: string;
  pattern: TeamPattern;
  participants: AgentDefinition[];
  createdAt: string;
  settings: Record<string, unknown>;
}

export enum TeamPattern {
  RoundRobin = 'RoundRobin',
  Broadcast = 'Broadcast',
  Supervised = 'Supervised',
  Router = 'Router',
  FreeForAll = 'FreeForAll'
}

export interface TeamExecutionResult {
  teamName: string;
  task: string;
  status: TeamExecutionStatus;
  messages: TeamMessage[];
  startedAt: string;
  completedAt: string;
  summary?: string;
}

export enum TeamExecutionStatus {
  Running = 'Running',
  Completed = 'Completed',
  Failed = 'Failed',
  Cancelled = 'Cancelled'
}

export interface TeamMessage {
  agentName: string;
  content: string;
  timestamp: string;
}

/**
 * API response wrapper
 */
export interface ApiResponse<T> {
  data?: T;
  error?: string;
  timestamp: string;
}
