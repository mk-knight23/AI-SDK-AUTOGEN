import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import apiService from '../services/api';
import type { Conversation, Message } from '../types/models';

export const useConversationsStore = defineStore('conversations', () => {
  // State
  const conversations = ref<Conversation[]>([]);
  const currentConversation = ref<Conversation | null>(null);
  const messages = ref<Map<string, Message[]>>(new Map());
  const loading = ref(false);
  const error = ref<string | null>(null);

  // Computed
  const currentMessages = computed(() =>
    currentConversation.value
      ? messages.value.get(currentConversation.value.id) ?? []
      : []
  );

  const totalConversations = computed(() => conversations.value.length);

  // Actions
  async function fetchConversations() {
    loading.value = true;
    error.value = null;
    try {
      conversations.value = await apiService.getConversations();
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch conversations';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function fetchConversation(id: string) {
    loading.value = true;
    error.value = null;
    try {
      currentConversation.value = await apiService.getConversation(id);
      return currentConversation.value;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch conversation';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function fetchMessages(conversationId: string, skip = 0, take = 100) {
    loading.value = true;
    error.value = null;
    try {
      const fetchedMessages = await apiService.getConversationMessages(conversationId, skip, take);
      const existing = messages.value.get(conversationId) ?? [];
      messages.value.set(conversationId, [...existing, ...fetchedMessages]);
      return fetchedMessages;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch messages';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function createConversation(conversation: Partial<Conversation>) {
    loading.value = true;
    error.value = null;
    try {
      const created = await apiService.createConversation(conversation);
      conversations.value.push(created);
      return created;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to create conversation';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function updateConversation(id: string, conversation: Partial<Conversation>) {
    loading.value = true;
    error.value = null;
    try {
      const updated = await apiService.updateConversation(id, conversation);
      const index = conversations.value.findIndex((c) => c.id === id);
      if (index >= 0) {
        conversations.value[index] = updated;
      }
      if (currentConversation.value?.id === id) {
        currentConversation.value = updated;
      }
      return updated;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to update conversation';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function deleteConversation(id: string) {
    loading.value = true;
    error.value = null;
    try {
      await apiService.deleteConversation(id);
      conversations.value = conversations.value.filter((c) => c.id !== id);
      messages.value.delete(id);
      if (currentConversation.value?.id === id) {
        currentConversation.value = null;
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to delete conversation';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function sendMessage(
    conversationId: string,
    sourceAgentId: string,
    content: string,
    targetAgentId?: string
  ) {
    loading.value = true;
    error.value = null;
    try {
      const message = await apiService.sendMessage(conversationId, sourceAgentId, content, targetAgentId);
      const conversationMessages = messages.value.get(conversationId) ?? [];
      messages.value.set(conversationId, [...conversationMessages, message]);
      return message;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to send message';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  function addMessage(conversationId: string, message: Message) {
    const conversationMessages = messages.value.get(conversationId) ?? [];
    messages.value.set(conversationId, [...conversationMessages, message]);
  }

  function clearMessages(conversationId: string) {
    messages.value.delete(conversationId);
  }

  function clearCurrentConversation() {
    currentConversation.value = null;
  }

  function clearError() {
    error.value = null;
  }

  return {
    // State
    conversations,
    currentConversation,
    messages,
    currentMessages,
    loading,
    error,
    // Computed
    totalConversations,
    // Actions
    fetchConversations,
    fetchConversation,
    fetchMessages,
    createConversation,
    updateConversation,
    deleteConversation,
    sendMessage,
    addMessage,
    clearMessages,
    clearCurrentConversation,
    clearError
  };
});
