<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useConversationsStore } from '../stores/conversations';

const router = useRouter();
const conversationsStore = useConversationsStore();

onMounted(async () => {
  await conversationsStore.fetchConversations();
});

async function createConversation() {
  const name = prompt('Enter conversation name:');
  if (name) {
    await conversationsStore.createConversation({
      name,
      participantIds: [],
      type: 'OneToOne' as any,
      metadata: {}
    });
  }
}
</script>

<template>
  <div class="conversations-view">
    <header class="page-header">
      <h1>Conversations</h1>
      <button class="btn-primary" @click="createConversation">New Conversation</button>
    </header>

    <div class="conversations-list">
      <div
        v-for="conv in conversationsStore.conversations"
        :key="conv.id"
        class="conversation-card"
        @click="router.push(`/conversations/${conv.id}`)"
      >
        <h3>{{ conv.name }}</h3>
        <p class="meta">{{ conv.participantIds.length }} participants</p>
        <p class="type">{{ conv.type }}</p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.conversations-view {
  max-width: 1200px;
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

.btn-primary {
  background: #667eea;
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
}

.conversations-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
}

.conversation-card {
  background: #fff;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s;
}

.conversation-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
}

.conversation-card h3 {
  margin: 0 0 0.5rem 0;
  color: #333;
}

.meta, .type {
  color: #666;
  font-size: 0.9rem;
  margin: 0.25rem 0;
}
</style>
