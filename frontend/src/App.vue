<script setup lang="ts">
import { RouterView } from 'vue-router';
</script>

<template>
  <div class="app">
    <header class="header">
      <nav class="nav">
        <h1 class="logo">AI-SDK-AUTOGEN</h1>
        <div class="nav-links">
          <router-link to="/" class="nav-link">Home</router-link>
          <router-link to="/agents" class="nav-link">Agents</router-link>
          <router-link to="/conversations" class="nav-link">Conversations</router-link>
          <router-link to="/code" class="nav-link">Code</router-link>
          <router-link to="/groups" class="nav-link">Groups</router-link>
          <router-link to="/teams" class="nav-link">Teams</router-link>
        </div>
      </nav>
    </header>
    <main class="main">
      <RouterView />
    </main>
    <footer class="footer">
      <p>AI-SDK-AUTOGEN - Multi-Agent System with Microsoft AutoGen. Built with Vue 3 + .NET 9.</p>
    </footer>
  </div>
</template>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif;
  background: #f5f5f5;
  color: #333;
}

.app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.header {
  background: #fff;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  padding: 1rem 2rem;
  position: sticky;
  top: 0;
  z-index: 100;
}

.nav {
  max-width: 1400px;
  margin: 0 auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.logo {
  color: #667eea;
  font-size: 1.5rem;
  font-weight: 600;
}

.nav-links {
  display: flex;
  gap: 2rem;
}

.nav-link {
  color: #666;
  text-decoration: none;
  font-weight: 500;
  transition: color 0.2s;
  position: relative;
}

.nav-link:hover,
.nav-link.router-link-active {
  color: #667eea;
}

.nav-link.router-link-active::after {
  content: '';
  position: absolute;
  bottom: -1.5rem;
  left: 0;
  right: 0;
  height: 2px;
  background: #667eea;
}

.main {
  flex: 1;
  width: 100%;
}

.footer {
  background: #fff;
  border-top: 1px solid #e0e0e0;
  padding: 1.5rem 2rem;
  text-align: center;
  color: #666;
  font-size: 0.9rem;
}
</style>
