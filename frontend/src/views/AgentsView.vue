<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useAgentsStore } from '../stores/agents';
import type { Agent, AgentType, AgentStatus } from '../types/models';

const router = useRouter();
const agentsStore = useAgentsStore();

const selectedType = ref<AgentType | 'All'>('All');
const selectedStatus = ref<AgentStatus | 'All'>('All');
const showCreateModal = ref(false);

const newAgent = ref<Partial<Agent>>({
  name: '',
  type: AgentType.Assistant,
  systemMessage: '',
  modelConfig: {
    provider: 'openai',
    model: 'gpt-4o-mini',
    apiKey: '',
    temperature: 0.7,
    maxTokens: 2048
  },
  status: AgentStatus.Inactive,
  metadata: {}
});

const filteredAgents = computed(() => {
  let result = agentsStore.agents;

  if (selectedType.value !== 'All') {
    result = result.filter(a => a.type === selectedType.value);
  }

  if (selectedStatus.value !== 'All') {
    result = result.filter(a => a.status === selectedStatus.value);
  }

  return result;
});

onMounted(async () => {
  await agentsStore.fetchAgents();
});

async function createAgent() {
  try {
    await agentsStore.createAgent(newAgent.value);
    showCreateModal.value = false;
    newAgent.value = {
      name: '',
      type: AgentType.Assistant,
      systemMessage: '',
      modelConfig: {
        provider: 'openai',
        model: 'gpt-4o-mini',
        apiKey: '',
        temperature: 0.7,
        maxTokens: 2048
      },
      status: AgentStatus.Inactive,
      metadata: {}
    };
  } catch (error) {
    console.error('Failed to create agent:', error);
  }
}

async function toggleAgentStatus(agent: Agent) {
  if (agent.status === AgentStatus.Active) {
    await agentsStore.deactivateAgent(agent.id);
  } else {
    await agentsStore.activateAgent(agent.id);
  }
}

async function deleteAgent(agent: Agent) {
  if (confirm(`Are you sure you want to delete agent "${agent.name}"?`)) {
    await agentsStore.deleteAgent(agent.id);
  }
}

function getStatusClass(status: AgentStatus): string {
  switch (status) {
    case AgentStatus.Active: return 'active';
    case AgentStatus.Inactive: return 'inactive';
    case AgentStatus.Busy: return 'busy';
    case AgentStatus.Error: return 'error';
    case AgentStatus.Terminated: return 'terminated';
    default: return '';
  }
}

function getTypeIcon(type: AgentType): string {
  switch (type) {
    case AgentType.System: return '⚙️';
    case AgentType.User: return '👤';
    case AgentType.Assistant: return '🤖';
    case AgentType.Tool: return '🔧';
    case AgentType.Supervisor: return '👨‍💼';
    case AgentType.Router: return '🔀';
    default: return '❓';
  }
}
</script>

<template>
  <div class="agents-view">
    <header class="page-header">
      <h1>Agents</h1>
      <button class="btn-primary" @click="showCreateModal = true">Create Agent</button>
    </header>

    <div class="filters">
      <select v-model="selectedType" class="filter-select">
        <option value="All">All Types</option>
        <option v-for="type in Object.values(AgentType)" :key="type" :value="type">
          {{ type }}
        </option>
      </select>

      <select v-model="selectedStatus" class="filter-select">
        <option value="All">All Statuses</option>
        <option v-for="status in Object.values(AgentStatus)" :key="status" :value="status">
          {{ status }}
        </option>
      </select>

      <div class="stats">
        <span>{{ filteredAgents.length }} agents</span>
        <span>{{ agentsStore.activeAgents.length }} active</span>
      </div>
    </div>

    <div class="agents-grid">
      <div
        v-for="agent in filteredAgents"
        :key="agent.id"
        class="agent-card"
        :class="getStatusClass(agent.status)"
        @click="router.push(`/agents/${agent.id}`)"
      >
        <div class="agent-header">
          <span class="agent-icon">{{ getTypeIcon(agent.type) }}</span>
          <div class="agent-info">
            <h3>{{ agent.name }}</h3>
            <span class="agent-type">{{ agent.type }}</span>
          </div>
          <span class="agent-status" :class="getStatusClass(agent.status)">
            {{ agent.status }}
          </span>
        </div>

        <div class="agent-details">
          <p class="system-message">{{ agent.systemMessage || 'No system message' }}</p>
          <div class="model-info">
            <span>{{ agent.modelConfig.model }}</span>
            <span>{{ agent.modelConfig.temperature }} temp</span>
          </div>
        </div>

        <div class="agent-actions">
          <button
            class="btn-action"
            :class="agent.status === AgentStatus.Active ? 'btn-danger' : 'btn-success'"
            @click.stop="toggleAgentStatus(agent)"
          >
            {{ agent.status === AgentStatus.Active ? 'Deactivate' : 'Activate' }}
          </button>
          <button class="btn-action btn-danger" @click.stop="deleteAgent(agent)">
            Delete
          </button>
        </div>
      </div>
    </div>

    <!-- Create Agent Modal -->
    <div v-if="showCreateModal" class="modal-overlay" @click.self="showCreateModal = false">
      <div class="modal">
        <header class="modal-header">
          <h2>Create Agent</h2>
          <button class="btn-close" @click="showCreateModal = false">×</button>
        </header>

        <form @submit.prevent="createAgent" class="modal-body">
          <div class="form-group">
            <label for="name">Name</label>
            <input id="name" v-model="newAgent.name" type="text" required placeholder="Agent name" />
          </div>

          <div class="form-group">
            <label for="type">Type</label>
            <select id="type" v-model="newAgent.type">
              <option v-for="type in Object.values(AgentType)" :key="type" :value="type">
                {{ type }}
              </option>
            </select>
          </div>

          <div class="form-group">
            <label for="systemMessage">System Message</label>
            <textarea
              id="systemMessage"
              v-model="newAgent.systemMessage"
              rows="4"
              placeholder="You are a helpful assistant..."
            ></textarea>
          </div>

          <div class="form-group">
            <label for="model">Model</label>
            <input id="model" v-model="newAgent.modelConfig!.model" type="text" />
          </div>

          <div class="form-row">
            <div class="form-group">
              <label for="temperature">Temperature</label>
              <input
                id="temperature"
                v-model.number="newAgent.modelConfig!.temperature"
                type="number"
                step="0.1"
                min="0"
                max="2"
              />
            </div>

            <div class="form-group">
              <label for="maxTokens">Max Tokens</label>
              <input
                id="maxTokens"
                v-model.number="newAgent.modelConfig!.maxTokens"
                type="number"
                min="1"
              />
            </div>
          </div>

          <footer class="modal-footer">
            <button type="button" class="btn-secondary" @click="showCreateModal = false">Cancel</button>
            <button type="submit" class="btn-primary" :disabled="agentsStore.loading">
              {{ agentsStore.loading ? 'Creating...' : 'Create Agent' }}
            </button>
          </footer>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.agents-view {
  max-width: 1400px;
  margin: 0 auto;
  padding: 2rem;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.page-header h1 {
  font-size: 2rem;
  color: #333;
}

.filters {
  display: flex;
  gap: 1rem;
  align-items: center;
  margin-bottom: 2rem;
  padding: 1rem;
  background: #f5f5f5;
  border-radius: 8px;
}

.filter-select {
  padding: 0.5rem 1rem;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 0.9rem;
}

.stats {
  margin-left: auto;
  display: flex;
  gap: 1rem;
  color: #666;
  font-size: 0.9rem;
}

.agents-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 1.5rem;
}

.agent-card {
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  padding: 1.5rem;
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s;
  border-left: 4px solid transparent;
}

.agent-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
}

.agent-card.active {
  border-left-color: #4caf50;
}

.agent-card.inactive {
  border-left-color: #9e9e9e;
}

.agent-card.busy {
  border-left-color: #ff9800;
}

.agent-card.error {
  border-left-color: #f44336;
}

.agent-header {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 1rem;
}

.agent-icon {
  font-size: 2rem;
}

.agent-info {
  flex: 1;
}

.agent-info h3 {
  margin: 0;
  color: #333;
  font-size: 1.1rem;
}

.agent-type {
  font-size: 0.85rem;
  color: #666;
}

.agent-status {
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
}

.agent-status.active {
  background: #e8f5e9;
  color: #2e7d32;
}

.agent-status.inactive {
  background: #f5f5f5;
  color: #616161;
}

.agent-status.busy {
  background: #fff3e0;
  color: #e65100;
}

.agent-status.error {
  background: #ffebee;
  color: #c62828;
}

.agent-details {
  margin-bottom: 1rem;
}

.system-message {
  color: #666;
  font-size: 0.9rem;
  line-height: 1.5;
  margin: 0 0 0.75rem 0;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.model-info {
  display: flex;
  gap: 1rem;
  font-size: 0.8rem;
  color: #999;
}

.agent-actions {
  display: flex;
  gap: 0.5rem;
}

/* Buttons */
.btn-primary, .btn-secondary, .btn-action, .btn-danger, .btn-success, .btn-close {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 6px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-primary {
  background: #667eea;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #5568d3;
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-secondary {
  background: #f5f5f5;
  color: #333;
}

.btn-secondary:hover {
  background: #e0e0e0;
}

.btn-action {
  flex: 1;
  font-size: 0.85rem;
}

.btn-success {
  background: #4caf50;
  color: white;
}

.btn-danger {
  background: #f44336;
  color: white;
}

.btn-close {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  padding: 0;
  width: 2rem;
  height: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
}

.btn-close:hover {
  background: #f5f5f5;
}

/* Modal */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal {
  background: white;
  border-radius: 12px;
  width: 90%;
  max-width: 500px;
  max-height: 90vh;
  overflow: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid #eee;
}

.modal-header h2 {
  margin: 0;
  font-size: 1.25rem;
}

.modal-body {
  padding: 1.5rem;
}

.form-group {
  margin-bottom: 1rem;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: #333;
  font-size: 0.9rem;
}

.form-group input,
.form-group select,
.form-group textarea {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 0.95rem;
}

.form-group textarea {
  resize: vertical;
  min-height: 80px;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  padding: 1rem 1.5rem;
  border-top: 1px solid #eee;
}
</style>
