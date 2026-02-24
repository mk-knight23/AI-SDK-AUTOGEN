<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useAgentsStore } from '../stores/agents';

const route = useRoute();
const router = useRouter();
const agentsStore = useAgentsStore();

onMounted(async () => {
  await agentsStore.fetchAgent(route.params.id as string);
});
</script>

<template>
  <div class="agent-detail-view">
    <button class="btn-back" @click="router.back()">← Back</button>

    <div v-if="agentsStore.currentAgent" class="agent-detail">
      <h1>{{ agentsStore.currentAgent.name }}</h1>
      <p class="type">{{ agentsStore.currentAgent.type }}</p>
      <p class="status" :class="{ active: agentsStore.currentAgent.status === 'Active' }">
        {{ agentsStore.currentAgent.status }}
      </p>

      <div class="details">
        <h3>System Message</h3>
        <p>{{ agentsStore.currentAgent.systemMessage }}</p>

        <h3>Model Configuration</h3>
        <div class="model-config">
          <p><strong>Provider:</strong> {{ agentsStore.currentAgent.modelConfig.provider }}</p>
          <p><strong>Model:</strong> {{ agentsStore.currentAgent.modelConfig.model }}</p>
          <p><strong>Temperature:</strong> {{ agentsStore.currentAgent.modelConfig.temperature }}</p>
          <p><strong>Max Tokens:</strong> {{ agentsStore.currentAgent.modelConfig.maxTokens }}</p>
        </div>
      </div>
    </div>

    <div v-else-if="agentsStore.loading" class="loading">Loading...</div>

    <div v-else class="error">Agent not found</div>
  </div>
</template>

<style scoped>
.agent-detail-view {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem;
}

.btn-back {
  background: none;
  border: none;
  color: #667eea;
  font-size: 1rem;
  cursor: pointer;
  margin-bottom: 1rem;
}

.agent-detail h1 {
  font-size: 2.5rem;
  color: #333;
  margin: 0 0 0.5rem 0;
}

.type, .status {
  font-size: 1rem;
  color: #666;
  margin: 0.25rem 0;
}

.status.active {
  color: #4caf50;
  font-weight: 600;
}

.details {
  margin-top: 2rem;
  background: #fff;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.details h3 {
  margin: 0 0 1rem 0;
  color: #333;
}

.details p {
  color: #666;
  line-height: 1.6;
  margin-bottom: 1rem;
}

.model-config p {
  margin: 0.5rem 0;
}
</style>
