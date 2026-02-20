<script setup lang="ts">
import { ref, onMounted } from 'vue'
import axios from 'axios'
import HelloWorld from '../components/HelloWorld.vue'

const healthStatus = ref<string>('Checking...')
const isHealthy = ref<boolean | null>(null)

onMounted(async () => {
  try {
    const response = await axios.get('/api/health')
    healthStatus.value = response.data.status || 'Healthy'
    isHealthy.value = true
  } catch (error) {
    healthStatus.value = 'Backend unavailable'
    isHealthy.value = false
  }
})
</script>

<template>
  <div class="home">
    <HelloWorld msg="Welcome to SupplyConsensus" />

    <div class="health-check">
      <h2>Backend Health Status</h2>
      <div class="status" :class="{ healthy: isHealthy, unhealthy: isHealthy === false }">
        <span class="status-indicator"></span>
        <span class="status-text">{{ healthStatus }}</span>
      </div>
    </div>

    <div class="features">
      <h2>Features</h2>
      <div class="feature-grid">
        <div class="feature-card">
          <h3>Vue 3 Frontend</h3>
          <p>Modern reactive UI built with Vue 3 Composition API and TypeScript.</p>
        </div>
        <div class="feature-card">
          <h3>.NET 9 Backend</h3>
          <p>High-performance API built with .NET 9 and minimal APIs.</p>
        </div>
        <div class="feature-card">
          <h3>Supply Chain</h3>
          <p>Consensus-driven supply chain management platform.</p>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.home {
  text-align: center;
}

.health-check {
  margin: 2rem 0;
  padding: 1.5rem;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.health-check h2 {
  margin-bottom: 1rem;
  color: #333;
}

.status {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.5rem;
  border-radius: 20px;
  font-weight: 500;
  background: #f0f0f0;
}

.status.healthy {
  background: #e8f5e9;
  color: #2e7d32;
}

.status.unhealthy {
  background: #ffebee;
  color: #c62828;
}

.status-indicator {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: currentColor;
}

.features {
  margin-top: 3rem;
}

.features h2 {
  margin-bottom: 1.5rem;
  color: #333;
}

.feature-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1.5rem;
}

.feature-card {
  background: #fff;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  text-align: left;
}

.feature-card h3 {
  color: #42b883;
  margin-bottom: 0.5rem;
}

.feature-card p {
  color: #666;
  line-height: 1.6;
}
</style>
