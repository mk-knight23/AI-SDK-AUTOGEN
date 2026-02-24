import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import apiService from '../services/api';
import type { GroupChat, GroupChatMessage } from '../types/models';

export const useGroupChatsStore = defineStore('groupChats', () => {
  // State
  const groupChats = ref<GroupChat[]>([]);
  const currentGroupChat = ref<GroupChat | null>(null);
  const messages = ref<Map<string, GroupChatMessage[]>>(new Map());
  const loading = ref(false);
  const error = ref<string | null>(null);

  // Computed
  const currentMessages = computed(() =>
    currentGroupChat.value
      ? messages.value.get(currentGroupChat.value.id) ?? []
      : []
  );

  const totalGroupChats = computed(() => groupChats.value.length);

  // Actions
  async function fetchGroupChats() {
    loading.value = true;
    error.value = null;
    try {
      groupChats.value = await apiService.getGroupChats();
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch group chats';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function fetchGroupChat(id: string) {
    loading.value = true;
    error.value = null;
    try {
      currentGroupChat.value = await apiService.getGroupChat(id);
      return currentGroupChat.value;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch group chat';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function fetchGroupChatMessages(groupChatId: string, skip = 0, take = 100) {
    loading.value = true;
    error.value = null;
    try {
      const fetchedMessages = await apiService.getGroupChatMessages(groupChatId, skip, take);
      const existing = messages.value.get(groupChatId) ?? [];
      messages.value.set(groupChatId, [...existing, ...fetchedMessages]);
      return fetchedMessages;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch messages';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function createGroupChat(groupChat: Partial<GroupChat>) {
    loading.value = true;
    error.value = null;
    try {
      const created = await apiService.createGroupChat(groupChat);
      groupChats.value.push(created);
      return created;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to create group chat';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function updateGroupChat(id: string, groupChat: Partial<GroupChat>) {
    loading.value = true;
    error.value = null;
    try {
      const updated = await apiService.updateGroupChat(id, groupChat);
      const index = groupChats.value.findIndex((g) => g.id === id);
      if (index >= 0) {
        groupChats.value[index] = updated;
      }
      if (currentGroupChat.value?.id === id) {
        currentGroupChat.value = updated;
      }
      return updated;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to update group chat';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function deleteGroupChat(id: string) {
    loading.value = true;
    error.value = null;
    try {
      await apiService.deleteGroupChat(id);
      groupChats.value = groupChats.value.filter((g) => g.id !== id);
      messages.value.delete(id);
      if (currentGroupChat.value?.id === id) {
        currentGroupChat.value = null;
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to delete group chat';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function sendGroupMessage(groupChatId: string, sourceAgentId: string, content: string) {
    loading.value = true;
    error.value = null;
    try {
      const message = await apiService.sendGroupMessage(groupChatId, sourceAgentId, content);
      const chatMessages = messages.value.get(groupChatId) ?? [];
      messages.value.set(groupChatId, [...chatMessages, message]);
      return message;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to send message';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function addParticipant(groupChatId: string, agentId: string) {
    loading.value = true;
    error.value = null;
    try {
      const updated = await apiService.addGroupParticipant(groupChatId, agentId);
      const index = groupChats.value.findIndex((g) => g.id === groupChatId);
      if (index >= 0) {
        groupChats.value[index] = updated;
      }
      if (currentGroupChat.value?.id === groupChatId) {
        currentGroupChat.value = updated;
      }
      return updated;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to add participant';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function removeParticipant(groupChatId: string, agentId: string) {
    loading.value = true;
    error.value = null;
    try {
      const updated = await apiService.removeGroupParticipant(groupChatId, agentId);
      const index = groupChats.value.findIndex((g) => g.id === groupChatId);
      if (index >= 0) {
        groupChats.value[index] = updated;
      }
      if (currentGroupChat.value?.id === groupChatId) {
        currentGroupChat.value = updated;
      }
      return updated;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to remove participant';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  function addMessage(groupChatId: string, message: GroupChatMessage) {
    const chatMessages = messages.value.get(groupChatId) ?? [];
    messages.value.set(groupChatId, [...chatMessages, message]);
  }

  function clearMessages(groupChatId: string) {
    messages.value.delete(groupChatId);
  }

  function clearCurrentGroupChat() {
    currentGroupChat.value = null;
  }

  function clearError() {
    error.value = null;
  }

  return {
    // State
    groupChats,
    currentGroupChat,
    messages,
    currentMessages,
    loading,
    error,
    // Computed
    totalGroupChats,
    // Actions
    fetchGroupChats,
    fetchGroupChat,
    fetchGroupChatMessages,
    createGroupChat,
    updateGroupChat,
    deleteGroupChat,
    sendGroupMessage,
    addParticipant,
    removeParticipant,
    addMessage,
    clearMessages,
    clearCurrentGroupChat,
    clearError
  };
});
