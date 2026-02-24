import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router';

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    name: 'Home',
    component: () => import('../views/HomeView.vue')
  },
  {
    path: '/agents',
    name: 'Agents',
    component: () => import('../views/AgentsView.vue')
  },
  {
    path: '/agents/:id',
    name: 'AgentDetail',
    component: () => import('../views/AgentDetailView.vue')
  },
  {
    path: '/conversations',
    name: 'Conversations',
    component: () => import('../views/ConversationsView.vue')
  },
  {
    path: '/conversations/:id',
    name: 'ConversationDetail',
    component: () => import('../views/ConversationDetailView.vue')
  },
  {
    path: '/code',
    name: 'CodeExecution',
    component: () => import('../views/CodeExecutionView.vue')
  },
  {
    path: '/groups',
    name: 'GroupChats',
    component: () => import('../views/GroupChatsView.vue')
  },
  {
    path: '/groups/:id',
    name: 'GroupChatDetail',
    component: () => import('../views/GroupChatDetailView.vue')
  },
  {
    path: '/teams',
    name: 'Teams',
    component: () => import('../views/TeamsView.vue')
  }
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
});

export default router;
