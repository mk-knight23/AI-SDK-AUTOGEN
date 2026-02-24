import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import apiService from '../services/api';
import type { CodeExecution, CodeLanguage, ExecutionStatus } from '../types/models';

export const useExecutionStore = defineStore('execution', () => {
  // State
  const executions = ref<CodeExecution[]>([]);
  const currentExecution = ref<CodeExecution | null>(null);
  const loading = ref(false);
  const error = ref<string | null>(null);

  // Computed
  const recentExecutions = computed(() =>
    [...executions.value].sort((a, b) =>
      new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    )
  );

  const pendingExecutions = computed(() =>
    executions.value.filter((e) => e.status === ExecutionStatus.Pending || e.status === ExecutionStatus.Running)
  );

  const completedExecutions = computed(() =>
    executions.value.filter((e) => e.status === ExecutionStatus.Completed)
  );

  const failedExecutions = computed(() =>
    executions.value.filter((e) =>
      e.status === ExecutionStatus.Failed || e.status === ExecutionStatus.Timeout
    )
  );

  // Actions
  async function fetchExecutions() {
    loading.value = true;
    error.value = null;
    try {
      executions.value = await apiService.getCodeExecutions();
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch executions';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function fetchRecentExecutions(count = 50) {
    loading.value = true;
    error.value = null;
    try {
      executions.value = await apiService.getRecentCodeExecutions(count);
      return executions.value;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch recent executions';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function fetchExecution(id: string) {
    loading.value = true;
    error.value = null;
    try {
      currentExecution.value = await apiService.getCodeExecution(id);
      return currentExecution.value;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch execution';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function executeCode(
    language: CodeLanguage,
    code: string,
    conversationId?: string,
    requestingAgentId?: string,
    timeoutSeconds = 30
  ) {
    loading.value = true;
    error.value = null;
    try {
      const execution = await apiService.executeCode(
        language,
        code,
        conversationId,
        requestingAgentId,
        timeoutSeconds
      );
      executions.value.push(execution);
      currentExecution.value = execution;
      return execution;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to execute code';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  function updateExecutionStatus(id: string, status: ExecutionStatus) {
    const index = executions.value.findIndex((e) => e.id === id);
    if (index >= 0) {
      executions.value[index].status = status;
    }
    if (currentExecution.value?.id === id) {
      currentExecution.value.status = status;
    }
  }

  function updateExecutionResult(id: string, result: CodeExecution['result']) {
    const index = executions.value.findIndex((e) => e.id === id);
    if (index >= 0) {
      executions.value[index].result = result;
    }
    if (currentExecution.value?.id === id) {
      currentExecution.value.result = result;
    }
  }

  function clearCurrentExecution() {
    currentExecution.value = null;
  }

  function clearError() {
    error.value = null;
  }

  return {
    // State
    executions,
    currentExecution,
    loading,
    error,
    // Computed
    recentExecutions,
    pendingExecutions,
    completedExecutions,
    failedExecutions,
    // Actions
    fetchExecutions,
    fetchRecentExecutions,
    fetchExecution,
    executeCode,
    updateExecutionStatus,
    updateExecutionResult,
    clearCurrentExecution,
    clearError
  };
});
