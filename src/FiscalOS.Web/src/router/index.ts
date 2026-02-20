import { AuthService } from '@/services/authService';
import { Client, ClientConfig } from '@/services/client';
import { useUserStore } from '@/stores/userStore';
import { createRouter, createWebHistory } from 'vue-router';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/public',
      component: () => import('../components/PublicLayout.vue'),
      redirect: '/public/login',
      beforeEnter: () => {
        const userStore = useUserStore();

        if (userStore.user) {
          return { path: '/' };
        }

        return true;
      },
      children: [
        {
          path: 'login',
          component: () => import('../views/LoginView.vue'),
        },
      ],
    },
    {
      path: '/',
      component: () => import('../components/ProtectedLayout.vue'),
      beforeEnter: async () => {
        const userStore = useUserStore();

        if (userStore.user === null) {
          return { path: '/public/login' };
        }

        const isExpired = userStore.user.expiresAtInSeconds < Date.now() / 1000;

        if (isExpired) {
          const clientConfig = new ClientConfig(
            { Authorization: `Bearer ${userStore.user.token}` },
            true
          );
          const client = new Client(clientConfig);
          const authService = new AuthService(client);
          const refreshResult = await authService.refreshToken();

          if (refreshResult.err) {
            userStore.logUserOut();
            return { path: '/public/login' };
          }

          userStore.logUserIn(refreshResult.val.accessToken);
        }

        return true;
      },
      children: [
        {
          path: '/',
          component: () => import('../views/HomeView.vue'),
        },
      ],
    },
  ],
});

export default router;
