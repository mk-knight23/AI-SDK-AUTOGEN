<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useGroupChatsStore } from '../stores/groupChats';

const router = useRouter();
const groupChatsStore = useGroupChatsStore();

onMounted(async () => {
  await groupChatsStore.fetchGroupChats();
});

async function createGroupChat() {
  const name = prompt('Enter group chat name:');
  if (name) {
    await groupChatsStore.createGroupChat({
      name,
      description: '',
      participantIds: [],
      pattern: 'RoundRobin' as any,
      rules: [],
      metadata: {}
    });
  }
}
</script>

<template>
  <div class="group-chats-view">
    <header class="page-header">
      <h1>Group Chats</h1>
      <button class="btn-primary" @click="createGroupChat">New Group Chat</button>
    </header>

    <div class="group-chats-list">
      <div
        v-for="chat in groupChatsStore.groupChats"
        :key="chat.id"
        class="group-chat-card"
        @click="router.push(`/groups/${chat.id}`)"
      >
        <h3>{{ chat.name }}</h3>
        <p class="description">{{ chat.description || 'No description' }}</p>
        <p class="meta">{{ chat.participantIds.length }} participants</p>
        <p class="pattern">{{ chat.pattern }}</p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.group-chats-view {
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

.group-chats-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
}

.group-chat-card {
  background: #fff;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s;
}

.group-chat-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
}

.group-chat-card h3 {
  margin: 0 0 0.5rem 0;
  color: #333;
}

.description {
  color: #666;
  font-size: 0.9rem;
  margin: 0.25rem 0 0.5rem 0;
}

.meta, .pattern {
  color: #999;
  font-size: 0.85rem;
  margin: 0.25rem 0;
}
</style>
