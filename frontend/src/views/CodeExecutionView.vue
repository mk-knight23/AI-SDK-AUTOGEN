<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useExecutionStore } from '../stores/execution';
import type { CodeLanguage, ExecutionStatus } from '../types/models';

const executionStore = useExecutionStore();

const selectedLanguage = ref<CodeLanguage>(CodeLanguage.Python);
const code = ref(`# Write your code here
print("Hello, World!")

# Example: Calculate factorial
def factorial(n):
    if n <= 1:
        return 1
    return n * factorial(n - 1)

print(f"Factorial of 5 is: {factorial(5)}")`);

const timeout = ref(30);

const recentExecutions = computed(() => executionStore.recentExecutions.slice(0, 10));

const languageOptions = [
  { value: CodeLanguage.Python, label: 'Python', icon: '🐍' },
  { value: CodeLanguage.JavaScript, label: 'JavaScript', icon: '📜' },
  { value: CodeLanguage.TypeScript, label: 'TypeScript', icon: '📘' },
  { value: CodeLanguage.CSharp, label: 'C#', icon: '🔷' },
  { value: CodeLanguage.Bash, label: 'Bash', icon: '💻' },
  { value: CodeLanguage.PowerShell, label: 'PowerShell', icon: '💠' }
];

onMounted(async () => {
  await executionStore.fetchRecentExecutions();
});

async function executeCode() {
  await executionStore.executeCode(
    selectedLanguage.value,
    code.value,
    undefined,
    undefined,
    timeout.value
  );
}

function getStatusClass(status: ExecutionStatus): string {
  switch (status) {
    case ExecutionStatus.Completed: return 'completed';
    case ExecutionStatus.Failed: return 'failed';
    case ExecutionStatus.Running: return 'running';
    case ExecutionStatus.Pending: return 'pending';
    case ExecutionStatus.Timeout: return 'timeout';
    default: return '';
  }
}

function formatDuration(duration: string): string {
  const ms = parseInt(duration);
  if (isNaN(ms)) return duration;
  if (ms < 1000) return `${ms}ms`;
  return `${(ms / 1000).toFixed(2)}s`;
}
</script>

<template>
  <div class="code-execution-view">
    <header class="page-header">
      <h1>Code Execution</h1>
    </header>

    <div class="execution-layout">
      <div class="editor-panel">
        <div class="editor-header">
          <select v-model="selectedLanguage" class="language-select">
            <option v-for="lang in languageOptions" :key="lang.value" :value="lang.value">
              {{ lang.icon }} {{ lang.label }}
            </option>
          </select>

          <div class="timeout-control">
            <label>Timeout (s):</label>
            <input v-model.number="timeout" type="number" min="5" max="300" />
          </div>

          <button
            class="btn-execute"
            :disabled="executionStore.loading"
            @click="executeCode"
          >
            {{ executionStore.loading ? 'Running...' : '▶ Run Code' }}
          </button>
        </div>

        <textarea
          v-model="code"
          class="code-editor"
          spellcheck="false"
          placeholder="Write your code here..."
        ></textarea>
      </div>

      <div class="output-panel">
        <div class="output-header">
          <h3>Output</h3>
          <div v-if="executionStore.currentExecution" class="execution-status">
            <span :class="getStatusClass(executionStore.currentExecution.status)">
              {{ executionStore.currentExecution.status }}
            </span>
          </div>
        </div>

        <div class="output-content">
          <div v-if="!executionStore.currentExecution" class="output-placeholder">
            Run code to see output here
          </div>

          <div v-else-if="executionStore.currentExecution.status === ExecutionStatus.Pending" class="output-loading">
            <div class="spinner"></div>
            <p>Preparing execution environment...</p>
          </div>

          <div v-else-if="executionStore.currentExecution.status === ExecutionStatus.Running" class="output-loading">
            <div class="spinner"></div>
            <p>Executing code...</p>
          </div>

          <div v-else class="output-results">
            <div v-if="executionStore.currentExecution.result?.stdout" class="output-section stdout">
              <h4>Output</h4>
              <pre>{{ executionStore.currentExecution.result.stdout }}</pre>
            </div>

            <div v-if="executionStore.currentExecution.result?.stderr" class="output-section stderr">
              <h4>Error Output</h4>
              <pre>{{ executionStore.currentExecution.result.stderr }}</pre>
            </div>

            <div v-if="executionStore.currentExecution.result?.error" class="output-section error">
              <h4>Execution Error</h4>
              <pre>{{ executionStore.currentExecution.result.error }}</pre>
            </div>

            <div v-if="executionStore.currentExecution.result" class="output-meta">
              <div class="meta-item">
                <span>Exit Code:</span>
                <strong :class="{ error: executionStore.currentExecution.result.exitCode !== 0 }">
                  {{ executionStore.currentExecution.result.exitCode }}
                </strong>
              </div>
              <div class="meta-item">
                <span>Duration:</span>
                <strong>{{ formatDuration(executionStore.currentExecution.result.duration) }}</strong>
              </div>
              <div class="meta-item">
                <span>Memory:</span>
                <strong>{{ executionStore.currentExecution.result.resourceUsage.memoryUsedMB }} MB</strong>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="recent-executions">
      <h3>Recent Executions</h3>
      <div class="executions-list">
        <div
          v-for="exec in recentExecutions"
          :key="exec.id"
          class="execution-item"
          :class="getStatusClass(exec.status)"
          @click="executionStore.currentExecution = exec"
        >
          <div class="execution-info">
            <span class="execution-language">{{ exec.language }}</span>
            <span class="execution-status-badge" :class="getStatusClass(exec.status)">
              {{ exec.status }}
            </span>
          </div>
          <div class="execution-time">{{ new Date(exec.createdAt).toLocaleString() }}</div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.code-execution-view {
  max-width: 1600px;
  margin: 0 auto;
  padding: 2rem;
}

.page-header {
  margin-bottom: 2rem;
}

.page-header h1 {
  font-size: 2rem;
  color: #333;
}

.execution-layout {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
  height: 600px;
  margin-bottom: 2rem;
}

.editor-panel {
  display: flex;
  flex-direction: column;
  background: #1e1e1e;
  border-radius: 12px;
  overflow: hidden;
}

.editor-header {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0.75rem 1rem;
  background: #2d2d2d;
  border-bottom: 1px solid #3e3e3e;
}

.language-select {
  background: #3e3e3e;
  color: #fff;
  border: 1px solid #4e4e4e;
  border-radius: 6px;
  padding: 0.5rem;
  font-size: 0.9rem;
}

.timeout-control {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #ccc;
  font-size: 0.85rem;
}

.timeout-control input {
  width: 60px;
  background: #3e3e3e;
  border: 1px solid #4e4e4e;
  border-radius: 4px;
  padding: 0.25rem 0.5rem;
  color: #fff;
  text-align: center;
}

.btn-execute {
  margin-left: auto;
  background: #4caf50;
  color: white;
  border: none;
  padding: 0.5rem 1.25rem;
  border-radius: 6px;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.2s;
}

.btn-execute:hover:not(:disabled) {
  background: #45a049;
}

.btn-execute:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.code-editor {
  flex: 1;
  background: #1e1e1e;
  color: #d4d4d4;
  border: none;
  padding: 1rem;
  font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
  font-size: 0.9rem;
  line-height: 1.6;
  resize: none;
}

.output-panel {
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.output-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
  border-bottom: 1px solid #eee;
}

.output-header h3 {
  margin: 0;
  color: #333;
}

.execution-status span {
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
}

.execution-status .completed {
  background: #e8f5e9;
  color: #2e7d32;
}

.execution-status .failed {
  background: #ffebee;
  color: #c62828;
}

.execution-status .running {
  background: #e3f2fd;
  color: #1565c0;
}

.execution-status .pending {
  background: #fff3e0;
  color: #e65100;
}

.output-content {
  flex: 1;
  overflow-y: auto;
  padding: 1rem;
}

.output-placeholder {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
  color: #999;
  font-style: italic;
}

.output-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  gap: 1rem;
  color: #666;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 3px solid #f3f3f3;
  border-top: 3px solid #667eea;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.output-results {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.output-section {
  border-radius: 8px;
  overflow: hidden;
}

.output-section h4 {
  margin: 0 0 0.5rem 0;
  font-size: 0.85rem;
  text-transform: uppercase;
  color: #666;
}

.output-section pre {
  margin: 0;
  padding: 1rem;
  background: #f5f5f5;
  border-radius: 6px;
  font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
  font-size: 0.85rem;
  line-height: 1.5;
  white-space: pre-wrap;
  word-break: break-all;
}

.output-section.stderr pre {
  background: #fff3e0;
}

.output-section.error pre {
  background: #ffebee;
  color: #c62828;
}

.output-meta {
  display: flex;
  gap: 1.5rem;
  padding: 0.75rem;
  background: #f9f9f9;
  border-radius: 6px;
  font-size: 0.85rem;
}

.meta-item {
  display: flex;
  gap: 0.5rem;
}

.meta-item span {
  color: #666;
}

.meta-item strong {
  color: #333;
}

.meta-item strong.error {
  color: #c62828;
}

.recent-executions {
  background: #fff;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.recent-executions h3 {
  margin: 0 0 1rem 0;
  color: #333;
}

.executions-list {
  display: grid;
  gap: 0.75rem;
}

.execution-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem 1rem;
  background: #f5f5f5;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.2s;
  border-left: 3px solid transparent;
}

.execution-item:hover {
  background: #eee;
}

.execution-item.completed {
  border-left-color: #4caf50;
}

.execution-item.failed {
  border-left-color: #f44336;
}

.execution-item.running {
  border-left-color: #2196f3;
}

.execution-item.pending {
  border-left-color: #ff9800;
}

.execution-info {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.execution-language {
  font-family: 'Consolas', 'Monaco', monospace;
  font-size: 0.9rem;
  color: #333;
}

.execution-status-badge {
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.7rem;
  font-weight: 600;
  text-transform: uppercase;
}

.execution-time {
  font-size: 0.8rem;
  color: #999;
}
</style>
