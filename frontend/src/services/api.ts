import axios, { AxiosInstance, AxiosResponse } from 'axios';
import type {
  Agent,
  Conversation,
  Message,
  CodeExecution,
  GroupChat,
  GroupChatMessage,
  AgentDefinition,
  TeamConfiguration,
  TeamExecutionResult
} from '../types/models';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:5001';

class ApiService {
  private client: AxiosInstance;

  constructor() {
    this.client = axios.create({
      baseURL: `${API_BASE_URL}/api`,
      headers: {
        'Content-Type': 'application/json'
      },
      timeout: 30000
    });

    // Add request interceptor for auth token if needed
    this.client.interceptors.request.use((config) => {
      const token = localStorage.getItem('auth_token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });

    // Add response interceptor for error handling
    this.client.interceptors.response.use(
      (response) => response,
      (error) => {
        console.error('API Error:', error.response?.data || error.message);
        throw error;
      }
    );
  }

  // Agents
  async getAgents(): Promise<Agent[]> {
    const response: AxiosResponse<Agent[]> = await this.client.get('/agents');
    return response.data;
  }

  async getAgent(id: string): Promise<Agent> {
    const response: AxiosResponse<Agent> = await this.client.get(`/agents/${id}`);
    return response.data;
  }

  async getActiveAgents(): Promise<Agent[]> {
    const response: AxiosResponse<Agent[]> = await this.client.get('/agents/active');
    return response.data;
  }

  async getAgentsByType(type: string): Promise<Agent[]> {
    const response: AxiosResponse<Agent[]> = await this.client.get(`/agents/by-type/${type}`);
    return response.data;
  }

  async createAgent(agent: Partial<Agent>): Promise<Agent> {
    const response: AxiosResponse<Agent> = await this.client.post('/agents', agent);
    return response.data;
  }

  async updateAgent(id: string, agent: Partial<Agent>): Promise<Agent> {
    const response: AxiosResponse<Agent> = await this.client.put(`/agents/${id}`, agent);
    return response.data;
  }

  async deleteAgent(id: string): Promise<void> {
    await this.client.delete(`/agents/${id}`);
  }

  async activateAgent(id: string): Promise<Agent> {
    const response: AxiosResponse<Agent> = await this.client.post(`/agents/${id}/activate`);
    return response.data;
  }

  async deactivateAgent(id: string): Promise<Agent> {
    const response: AxiosResponse<Agent> = await this.client.post(`/agents/${id}/deactivate`);
    return response.data;
  }

  // Conversations
  async getConversations(): Promise<Conversation[]> {
    const response: AxiosResponse<Conversation[]> = await this.client.get('/conversations');
    return response.data;
  }

  async getConversation(id: string): Promise<Conversation> {
    const response: AxiosResponse<Conversation> = await this.client.get(`/conversations/${id}`);
    return response.data;
  }

  async getConversationMessages(
    id: string,
    skip = 0,
    take = 100
  ): Promise<Message[]> {
    const response: AxiosResponse<Message[]> = await this.client.get(
      `/conversations/${id}/messages`,
      { params: { skip, take } }
    );
    return response.data;
  }

  async createConversation(conversation: Partial<Conversation>): Promise<Conversation> {
    const response: AxiosResponse<Conversation> = await this.client.post('/conversations', conversation);
    return response.data;
  }

  async updateConversation(id: string, conversation: Partial<Conversation>): Promise<Conversation> {
    const response: AxiosResponse<Conversation> = await this.client.put(`/conversations/${id}`, conversation);
    return response.data;
  }

  async deleteConversation(id: string): Promise<void> {
    await this.client.delete(`/conversations/${id}`);
  }

  async sendMessage(
    conversationId: string,
    sourceAgentId: string,
    content: string,
    targetAgentId?: string
  ): Promise<Message> {
    const response: AxiosResponse<Message> = await this.client.post(
      `/conversations/${conversationId}/messages`,
      { sourceAgentId, content, targetAgentId }
    );
    return response.data;
  }

  async getConversationsByParticipant(agentId: string): Promise<Conversation[]> {
    const response: AxiosResponse<Conversation[]> = await this.client.get(`/conversations/by-participant/${agentId}`);
    return response.data;
  }

  // Code Execution
  async executeCode(
    language: string,
    code: string,
    conversationId?: string,
    requestingAgentId?: string,
    timeoutSeconds = 30,
    resourceLimits?: object
  ): Promise<CodeExecution> {
    const response: AxiosResponse<CodeExecution> = await this.client.post('/code/execute', {
      language,
      code,
      conversationId,
      requestingAgentId,
      timeoutSeconds,
      resourceLimits
    });
    return response.data;
  }

  async getCodeExecution(id: string): Promise<CodeExecution> {
    const response: AxiosResponse<CodeExecution> = await this.client.get(`/code/executions/${id}`);
    return response.data;
  }

  async getCodeExecutions(): Promise<CodeExecution[]> {
    const response: AxiosResponse<CodeExecution[]> = await this.client.get('/code/executions');
    return response.data;
  }

  async getRecentCodeExecutions(count = 50): Promise<CodeExecution[]> {
    const response: AxiosResponse<CodeExecution[]> = await this.client.get('/code/executions/recent', {
      params: { count }
    });
    return response.data;
  }

  async getConversationExecutions(conversationId: string): Promise<CodeExecution[]> {
    const response: AxiosResponse<CodeExecution[]> = await this.client.get(
      `/code/conversations/${conversationId}/executions`
    );
    return response.data;
  }

  // Group Chats
  async getGroupChats(): Promise<GroupChat[]> {
    const response: AxiosResponse<GroupChat[]> = await this.client.get('/groups');
    return response.data;
  }

  async getGroupChat(id: string): Promise<GroupChat> {
    const response: AxiosResponse<GroupChat> = await this.client.get(`/groups/${id}`);
    return response.data;
  }

  async getGroupChatMessages(id: string, skip = 0, take = 100): Promise<GroupChatMessage[]> {
    const response: AxiosResponse<GroupChatMessage[]> = await this.client.get(
      `/groups/${id}/messages`,
      { params: { skip, take } }
    );
    return response.data;
  }

  async createGroupChat(groupChat: Partial<GroupChat>): Promise<GroupChat> {
    const response: AxiosResponse<GroupChat> = await this.client.post('/groups', groupChat);
    return response.data;
  }

  async updateGroupChat(id: string, groupChat: Partial<GroupChat>): Promise<GroupChat> {
    const response: AxiosResponse<GroupChat> = await this.client.put(`/groups/${id}`, groupChat);
    return response.data;
  }

  async deleteGroupChat(id: string): Promise<void> {
    await this.client.delete(`/groups/${id}`);
  }

  async sendGroupMessage(groupChatId: string, sourceAgentId: string, content: string): Promise<GroupChatMessage> {
    const response: AxiosResponse<GroupChatMessage> = await this.client.post(
      `/groups/${groupChatId}/messages`,
      { sourceAgentId, content }
    );
    return response.data;
  }

  async addGroupParticipant(groupChatId: string, agentId: string): Promise<GroupChat> {
    const response: AxiosResponse<GroupChat> = await this.client.post(
      `/groups/${groupChatId}/participants/${agentId}`
    );
    return response.data;
  }

  async removeGroupParticipant(groupChatId: string, agentId: string): Promise<GroupChat> {
    const response: AxiosResponse<GroupChat> = await this.client.delete(
      `/groups/${groupChatId}/participants/${agentId}`
    );
    return response.data;
  }

  async getGroupChatsByParticipant(agentId: string): Promise<GroupChat[]> {
    const response: AxiosResponse<GroupChat[]> = await this.client.get(`/groups/by-participant/${agentId}`);
    return response.data;
  }

  // AutoGen
  async registerAgent(agent: Partial<AgentDefinition>): Promise<AgentDefinition> {
    const response: AxiosResponse<AgentDefinition> = await this.client.post('/autogen/agents', agent);
    return response.data;
  }

  async getRegisteredAgent(name: string): Promise<AgentDefinition> {
    const response: AxiosResponse<AgentDefinition> = await this.client.get(`/autogen/agents/${name}`);
    return response.data;
  }

  async getRegisteredAgents(): Promise<AgentDefinition[]> {
    const response: AxiosResponse<AgentDefinition[]> = await this.client.get('/autogen/agents');
    return response.data;
  }

  async unregisterAgent(name: string): Promise<void> {
    await this.client.delete(`/autogen/agents/${name}`);
  }

  async createTeam(name: string, pattern: string, participantNames: string[]): Promise<TeamConfiguration> {
    const response: AxiosResponse<TeamConfiguration> = await this.client.post('/autogen/teams', {
      name,
      pattern,
      participantNames
    });
    return response.data;
  }

  async runTeam(name: string, task: string, pattern?: string, participantNames?: string[]): Promise<TeamExecutionResult> {
    const response: AxiosResponse<TeamExecutionResult> = await this.client.post(`/autogen/teams/${name}/run`, {
      task,
      pattern,
      participantNames
    });
    return response.data;
  }

  async getTeamStatus(name: string): Promise<object> {
    const response: AxiosResponse<object> = await this.client.get(`/autogen/teams/${name}/status`);
    return response.data;
  }
}

export const apiService = new ApiService();
export default apiService;
