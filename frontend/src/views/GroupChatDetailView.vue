<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useGroupChatsStore } from '../stores/groupChats';

const route = useRoute();
const router = useRouter();
const groupChatsStore = useGroupChatsStore();

const newMessage = ref('');

const currentMessages = computed(() => groupChatsStore.currentMessages);

onMounted(async () => {
  const id = route.params.id as string;
  await groupChatsStore.fetchGroupChat(id);
  await groupChatsStore.fetchGroupChatMessages(id);
});

async function sendMessage() {
  if (!newMessage.value.trim() || !groupChatsStore.currentGroupChat) return;

  // Use first participant as source for demo
  const sourceId = groupChatsStore.currentGroupChat.participantIds[0];
  await groupChatsStore.sendGroupMessage(
    groupChatsStore.currentGroupChat.id,
    sourceId,
    newMessage.value
  );
  newMessage.value = '';
}
</script>

<template>
  <div class="group-chat-detail-view">
    <button class="btn-back" @click="router.back()">← Back</button>

    <div v-if="groupChatsStore.currentGroupChat" class="group-chat-detail">
      <header class="chat-header">
        <h1>{{ groupChatsStore.currentGroupChat.name }}</h1>
        <p class="description">{{ groupChatsStore.currentGroupChat.description }}</p>
        <p class="meta">{{ groupChatsStore.currentGroupChat.participantIds.length }} participants · {{ groupChatsStore.currentGroupChat.pattern }}</p>
      </header>

      <div class="messages-list">
        <div v-for="msg in currentMessages" :key="msg.id" class="message">
          <span class="sender">{{ msg.sourceAgentId }}</span>
          <p class="content">{{ msg.content }}</p>
          <span class="time">#{{ msg.sequenceNumber }} · {{ new Date(msg.timestamp).toLocaleTimeString() }}</span>
        </div>
      </div>

      <form class="message-form" @submit.prevent="sendMessage">
        <input v-model="newMessage" type="text" placeholder="Type a message..." />
        <button type="submit" :disabled="!newMessage.trim()">Send</button>
      </form>
    </div>

    <div v-else-if="groupChatsStore.loading" class="loading">Loading...</div>

    <div v-else class="error">Group chat not found</div>
  </div>
</template>

<style scoped>
.group-chat-detail-view {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem;
  display: flex;
  flex-direction: column;
  height: calc(100vh - 200px);
}

.btn-back {
  background: none;
  border: none;
  color: #667eea;
  font-size: 1rem;
  cursor: pointer;
  margin-bottom: 1rem;
}

.group-chat-detail {
  flex: 1;
  display: flex;
  flex-direction: column;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.chat-header {
  padding: 1.5rem;
  border-bottom: 1px solid #eee;
}

.chat-header h1 {
  margin: 0 0 0.25rem 0;
  font-size: 1.5rem;
}

.description {
  color: #666;
  font-size: 0.95rem;
  margin: 0.25rem 0 0 0;
}

.meta {
  color: #999;
  font-size: 0.85rem;
  margin: 0.5rem 0 0 0;
}

.messages-list {
  flex: 1;
  overflow-y: auto;
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.message {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.sender {
  font-size: 0.8rem;
  color: #667eea;
  font-weight: 600;
}

.content {
  background: #f0f4ff;
  padding: 0.75rem 1rem;
  border-radius: 12px;
  margin: 0;
  border-left: 3px solid #667eea;
}

.time {
  font-size: 0.75rem;
  color: #999;
}

.message-form {
  display: flex;
  gap: 0.5rem;
  padding: 1rem;
  border-top: 1px solid #eee;
}

.message-form input {
  flex: 1;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 24px;
  font-size: 0.95rem;
}

.message-form button {
  background: #667eea;
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 24px;
  font-weight: 500;
  cursor: pointer;
}

.message-form button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
