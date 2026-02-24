import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import apiService from '../services/api';
import type { Agent, AgentType, AgentStatus } from '../types/models';

export const useAgentsStore = defineStore('agents', () => {
  // State
  const agents = ref<Agent[]>([]);
  const currentAgent = ref<Agent | null>(null);
  const loading = ref(false);
  const error = ref<string | null>(null);

  // Computed
  const activeAgents = computed(() =>
    agents.value.filter((agent) => agent.status === AgentStatus.Active)
  );

  const agentsByType = computed(() => {
    const grouped: Record<AgentType, Agent[]> = {
      [AgentType.System]: [],
      [AgentType.User]: [],
      [AgentType.Assistant]: [],
      [AgentType.Tool]: [],
      [AgentType.Supervisor]: [],
      [AgentType.Router]: []
    };
    agents.value.forEach((agent) => {
      grouped[agent.type].push(agent);
    });
    return grouped;
  });

  const totalAgents = computed(() => agents.value.length);

  // Actions
  async function fetchAgents() {
    loading.value = true;
    error.value = null;
    try {
      agents.value = await apiService.getAgents();
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch agents';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function fetchAgent(id: string) {
    loading.value = true;
    error.value = null;
    try {
      currentAgent.value = await apiService.getAgent(id);
      return currentAgent.value;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch agent';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function fetchActiveAgents() {
    loading.value = true;
    error.value = null;
    try {
      const active = await apiService.getActiveAgents();
      // Update only active agents in the store
      active.forEach((agent) => {
        const index = agents.value.findIndex((a) => a.id === agent.id);
        if (index >= 0) {
          agents.value[index] = agent;
        } else {
          agents.value.push(agent);
        }
      });
      return active;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch active agents';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function createAgent(agent: Partial<Agent>) {
    loading.value = true;
    error.value = null;
    try {
      const created = await apiService.createAgent(agent);
      agents.value.push(created);
      return created;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to create agent';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function updateAgent(id: string, agent: Partial<Agent>) {
    loading.value = true;
    error.value = null;
    try {
      const updated = await apiService.updateAgent(id, agent);
      const index = agents.value.findIndex((a) => a.id === id);
      if (index >= 0) {
        agents.value[index] = updated;
      }
      if (currentAgent.value?.id === id) {
        currentAgent.value = updated;
      }
      return updated;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to update agent';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function deleteAgent(id: string) {
    loading.value = true;
    error.value = null;
    try {
      await apiService.deleteAgent(id);
      agents.value = agents.value.filter((a) => a.id !== id);
      if (currentAgent.value?.id === id) {
        currentAgent.value = null;
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to delete agent';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function activateAgent(id: string) {
    loading.value = true;
    error.value = null;
    try {
      const activated = await apiService.activateAgent(id);
      const index = agents.value.findIndex((a) => a.id === id);
      if (index >= 0) {
        agents.value[index] = activated;
      }
      if (currentAgent.value?.id === id) {
        currentAgent.value = activated;
      }
      return activated;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to activate agent';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function deactivateAgent(id: string) {
    loading.value = true;
    error.value = null;
    try {
      const deactivated = await apiService.deactivateAgent(id);
      const index = agents.value.findIndex((a) => a.id === id);
      if (index >= 0) {
        agents.value[index] = deactivated;
      }
      if (currentAgent.value?.id === id) {
        currentAgent.value = deactivated;
      }
      return deactivated;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to deactivate agent';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  function updateAgentStatus(id: string, status: AgentStatus) {
    const index = agents.value.findIndex((a) => a.id === id);
    if (index >= 0) {
      agents.value[index].status = status;
    }
    if (currentAgent.value?.id === id) {
      currentAgent.value.status = status;
    }
  }

  function clearCurrentAgent() {
    currentAgent.value = null;
  }

  function clearError() {
    error.value = null;
  }

  return {
    // State
    agents,
    currentAgent,
    loading,
    error,
    // Computed
    activeAgents,
    agentsByType,
    totalAgents,
    // Actions
    fetchAgents,
    fetchAgent,
    fetchActiveAgents,
    createAgent,
    updateAgent,
    deleteAgent,
    activateAgent,
    deactivateAgent,
    updateAgentStatus,
    clearCurrentAgent,
    clearError
  };
});
