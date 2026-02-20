<script setup lang="ts">
defineProps<{
  msg: string
}>()
</script>

<template>
  <div class="hello-world">
    <h1 class="title">{{ msg }}</h1>
    <p class="subtitle">
      A modern supply chain consensus platform built with Vue 3 and .NET 9
    </p>
  </div>
</template>

<style scoped>
.hello-world {
  padding: 2rem 0;
}

.title {
  font-size: 2.5rem;
  font-weight: 700;
  color: #2c3e50;
  margin-bottom: 1rem;
}

.subtitle {
  font-size: 1.25rem;
  color: #666;
  max-width: 600px;
  margin: 0 auto;
  line-height: 1.6;
}
</style>
