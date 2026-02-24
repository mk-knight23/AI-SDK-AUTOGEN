<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useAgentsStore } from '../stores/agents';
import { useConversationsStore } from '../stores/conversations';
import { useGroupChatsStore } from '../stores/groupChats';

const router = useRouter();
const agentsStore = useAgentsStore();
const conversationsStore = useConversationsStore();
const groupChatsStore = useGroupChatsStore();

const healthStatus = ref<string>('Checking...');
const isHealthy = ref<boolean | null>(null);

const quickStats = computed(() => ({
  totalAgents: agentsStore.totalAgents,
  activeAgents: agentsStore.activeAgents.length,
  totalConversations: conversationsStore.totalConversations,
  totalGroupChats: groupChatsStore.totalGroupChats
}));

onMounted(async () => {
  await Promise.all([
    agentsStore.fetchAgents(),
    conversationsStore.fetchConversations(),
    groupChatsStore.fetchGroupChats()
  ]);

  try {
    const response = await fetch('/api');
    const data = await response.json();
    healthStatus.value = data.Status || 'Healthy';
    isHealthy.value = true;
  } catch (error) {
    healthStatus.value = 'Backend unavailable';
    isHealthy.value = false;
  }
});

function navigateTo(path: string) {
  router.push(path);
}
</script>

<template>
  <div class="home">
    <header class="hero">
      <h1>AI-SDK-AUTOGEN</h1>
      <p class="subtitle">Multi-Agent System with Microsoft AutoGen</p>
      <div class="status" :class="{ healthy: isHealthy, unhealthy: isHealthy === false }">
        <span class="status-indicator"></span>
        <span class="status-text">{{ healthStatus }}</span>
      </div>
    </header>

    <div class="quick-stats">
      <div class="stat-card" @click="navigateTo('/agents')">
        <div class="stat-value">{{ quickStats.totalAgents }}</div>
        <div class="stat-label">Total Agents</div>
      </div>
      <div class="stat-card" @click="navigateTo('/agents')">
        <div class="stat-value">{{ quickStats.activeAgents }}</div>
        <div class="stat-label">Active Agents</div>
      </div>
      <div class="stat-card" @click="navigateTo('/conversations')">
        <div class="stat-value">{{ quickStats.totalConversations }}</div>
        <div class="stat-label">Conversations</div>
      </div>
      <div class="stat-card" @click="navigateTo('/groups')">
        <div class="stat-value">{{ quickStats.totalGroupChats }}</div>
        <div class="stat-label">Group Chats</div>
      </div>
    </div>

    <div class="navigation">
      <h2>Quick Actions</h2>
      <div class="nav-grid">
        <router-link to="/agents" class="nav-card">
          <div class="nav-icon">🤖</div>
          <h3>Agents</h3>
          <p>Manage AI agents and their configurations</p>
        </router-link>
        <router-link to="/conversations" class="nav-card">
          <div class="nav-icon">💬</div>
          <h3>Conversations</h3>
          <p>View and manage agent conversations</p>
        </router-link>
        <router-link to="/code" class="nav-card">
          <div class="nav-icon">⚡</div>
          <h3>Code Execution</h3>
          <p>Execute code in isolated sandboxes</p>
        </router-link>
        <router-link to="/groups" class="nav-card">
          <div class="nav-icon">👥</div>
          <h3>Group Chats</h3>
          <p>Multi-agent collaboration patterns</p>
        </router-link>
        <router-link to="/teams" class="nav-card">
          <div class="nav-icon">🔧</div>
          <h3>Teams</h3>
          <p>AutoGen team orchestration</p>
        </router-link>
      </div>
    </div>

    <div class="features">
      <h2>Features</h2>
      <div class="feature-grid">
        <div class="feature-card">
          <h3>Multi-Agent Communication</h3>
          <p>gRPC-based agent messaging with real-time updates via SignalR</p>
        </div>
        <div class="feature-card">
          <h3>Code Execution</h3>
          <p>Docker-based sandboxed execution for Python, JavaScript, and C#</p>
        </div>
        <div class="feature-card">
          <h3>Group Chat Patterns</h3>
          <p>Round-robin, broadcast, and supervised conversation patterns</p>
        </div>
        <div class="feature-card">
          <h3>AutoGen Integration</h3>
          <p>Microsoft AutoGen framework for team orchestration</p>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.home {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}

.hero {
  text-align: center;
  margin-bottom: 3rem;
  padding: 3rem 1rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 16px;
  color: white;
}

.hero h1 {
  font-size: 3rem;
  margin-bottom: 0.5rem;
  font-weight: 700;
}

.subtitle {
  font-size: 1.25rem;
  opacity: 0.9;
  margin-bottom: 1.5rem;
}

.status {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  border-radius: 20px;
  font-weight: 500;
  background: rgba(255, 255, 255, 0.2);
  backdrop-filter: blur(10px);
}

.status.healthy {
  background: rgba(76, 175, 80, 0.3);
}

.status.unhealthy {
  background: rgba(244, 67, 54, 0.3);
}

.status-indicator {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: currentColor;
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

.quick-stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  margin-bottom: 3rem;
}

.stat-card {
  background: #fff;
  padding: 1.5rem;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  text-align: center;
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s;
}

.stat-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
}

.stat-value {
  font-size: 2.5rem;
  font-weight: 700;
  color: #667eea;
}

.stat-label {
  color: #666;
  font-size: 0.9rem;
  margin-top: 0.25rem;
}

.navigation {
  margin-bottom: 3rem;
}

.navigation h2 {
  margin-bottom: 1.5rem;
  color: #333;
  font-size: 1.5rem;
}

.nav-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.5rem;
}

.nav-card {
  background: #fff;
  padding: 2rem;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  text-align: center;
  text-decoration: none;
  color: inherit;
  transition: transform 0.2s, box-shadow 0.2s;
}

.nav-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
}

.nav-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.nav-card h3 {
  color: #333;
  margin-bottom: 0.5rem;
}

.nav-card p {
  color: #666;
  font-size: 0.9rem;
  line-height: 1.5;
}

.features {
  margin-top: 3rem;
}

.features h2 {
  margin-bottom: 1.5rem;
  color: #333;
  font-size: 1.5rem;
}

.feature-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1.5rem;
}

.feature-card {
  background: #fff;
  padding: 1.5rem;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  text-align: left;
}

.feature-card h3 {
  color: #667eea;
  margin-bottom: 0.5rem;
}

.feature-card p {
  color: #666;
  line-height: 1.6;
  font-size: 0.95rem;
}
</style>
