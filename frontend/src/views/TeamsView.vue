<script setup lang="ts">
import { ref } from 'vue';
import apiService from '../services/api';

const teamName = ref('');
const task = ref('');
const pattern = ref<'RoundRobin' | 'Broadcast' | 'Supervised'>('RoundRobin');
const participantNames = ref<string[]>([]);
const newParticipant = ref('');
const result = ref<any>(null);

async function addParticipant() {
  if (newParticipant.value && !participantNames.value.includes(newParticipant.value)) {
    participantNames.value.push(newParticipant.value);
    newParticipant.value = '';
  }
}

function removeParticipant(name: string) {
  participantNames.value = participantNames.value.filter(n => n !== name);
}

async function runTeam() {
  if (!teamName.value || !task.value) {
    alert('Please enter team name and task');
    return;
  }

  try {
    result.value = await apiService.runTeam(
      teamName.value,
      task.value,
      pattern.value,
      participantNames.value
    );
  } catch (error) {
    console.error('Failed to run team:', error);
    alert('Failed to run team');
  }
}
</script>

<template>
  <div class="teams-view">
    <header class="page-header">
      <h1>AutoGen Teams</h1>
    </header>

    <div class="teams-layout">
      <div class="team-config">
        <h2>Create & Run Team</h2>

        <div class="form-group">
          <label>Team Name</label>
          <input v-model="teamName" type="text" placeholder="My Team" />
        </div>

        <div class="form-group">
          <label>Pattern</label>
          <select v-model="pattern">
            <option value="RoundRobin">Round Robin</option>
            <option value="Broadcast">Broadcast</option>
            <option value="Supervised">Supervised</option>
          </select>
        </div>

        <div class="form-group">
          <label>Participants</label>
          <div class="participants-input">
            <input
              v-model="newParticipant"
              @keyup.enter="addParticipant"
              type="text"
              placeholder="Agent name (e.g., agent1)"
            />
            <button class="btn-add" @click="addParticipant">Add</button>
          </div>
          <div class="participants-list">
            <span
              v-for="name in participantNames"
              :key="name"
              class="participant-tag"
            >
              {{ name }}
              <button class="btn-remove" @click="removeParticipant(name)">×</button>
            </span>
          </div>
        </div>

        <div class="form-group">
          <label>Task</label>
          <textarea
            v-model="task"
            rows="4"
            placeholder="Describe the task for the team..."
          ></textarea>
        </div>

        <button class="btn-run" @click="runTeam">Run Team</button>
      </div>

      <div class="team-results">
        <h2>Results</h2>
        <div v-if="!result" class="results-placeholder">
          Configure a team and run to see results
        </div>
        <div v-else class="results-content">
          <h3>{{ result.teamName }}</h3>
          <p class="task"><strong>Task:</strong> {{ result.task }}</p>
          <p class="status"><strong>Status:</strong> {{ result.status }}</p>

          <div class="messages">
            <h4>Messages</h4>
            <div v-for="(msg, i) in result.messages" :key="i" class="message">
              <strong>{{ msg.agentName }}:</strong> {{ msg.content }}
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.teams-view {
  max-width: 1400px;
  margin: 0 auto;
  padding: 2rem;
}

.page-header h1 {
  font-size: 2rem;
  color: #333;
  margin-bottom: 2rem;
}

.teams-layout {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 2rem;
}

.team-config, .team-results {
  background: #fff;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.team-config h2, .team-results h2 {
  margin: 0 0 1.5rem 0;
  color: #333;
}

.form-group {
  margin-bottom: 1.25rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: #333;
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

.participants-input {
  display: flex;
  gap: 0.5rem;
}

.participants-input input {
  flex: 1;
}

.btn-add {
  background: #667eea;
  color: white;
  border: none;
  padding: 0.75rem 1rem;
  border-radius: 6px;
  cursor: pointer;
}

.participants-list {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-top: 0.75rem;
}

.participant-tag {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.25rem 0.75rem;
  background: #f0f0f0;
  border-radius: 16px;
  font-size: 0.85rem;
}

.btn-remove {
  background: none;
  border: none;
  cursor: pointer;
  font-size: 1.2rem;
  color: #999;
}

.btn-remove:hover {
  color: #f44336;
}

.btn-run {
  width: 100%;
  background: #4caf50;
  color: white;
  border: none;
  padding: 1rem;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  font-size: 1rem;
}

.btn-run:hover {
  background: #45a049;
}

.results-placeholder {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 200px;
  color: #999;
  font-style: italic;
}

.results-content h3 {
  margin: 0 0 1rem 0;
  color: #333;
}

.task, .status {
  margin: 0.5rem 0;
  color: #666;
}

.messages {
  margin-top: 1.5rem;
  padding-top: 1.5rem;
  border-top: 1px solid #eee;
}

.messages h4 {
  margin: 0 0 1rem 0;
  color: #333;
}

.message {
  padding: 0.75rem;
  background: #f5f5f5;
  border-radius: 6px;
  margin-bottom: 0.5rem;
}
</style>
