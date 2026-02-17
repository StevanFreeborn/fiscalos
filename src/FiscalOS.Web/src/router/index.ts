import { useUserStore } from "@/stores/userStore";
import { createRouter, createWebHistory } from "vue-router";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/public',
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
          component: () => import('../views/LoginView.vue')
        }
      ],
    },
    {
      path: '/',
      beforeEnter: () => {
        // TODO: Handle expired token
        // Try to refresh
        //  if refresh fails log out
        //    redirect to login page
        //  if refresh succeeds
        //    log user in and allow
        //    to pass
        const userStore = useUserStore();

        if (userStore.user === null) {
          return { path: '/public/login' }
        }

        return true;
      },
      component: () => import('../views/HomeView.vue'),
    }
  ],
});

export default router;
