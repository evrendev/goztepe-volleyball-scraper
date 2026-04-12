import { createRouter, createWebHistory } from "vue-router";
import FixtureView from "@/views/FixtureView.vue";
import StandingsView from "@/views/StandingsView.vue";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "home",
      redirect: { name: "fixture" },
    },
    {
      path: "/fikstur",
      name: "fixture",
      component: FixtureView,
    },
    {
      path: "/puan-durumu",
      name: "standings",
      component: StandingsView,
    },
    {
      // Catch-all route for invalid URLs - redirect to home
      path: "/:pathMatch(.*)*",
      name: "not-found",
      redirect: { name: "home" },
    },
  ],
});

export default router;
