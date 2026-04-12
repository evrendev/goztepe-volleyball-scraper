import { createRouter, createWebHistory } from "vue-router";
import FixtureView from "@/views/FixtureView.vue";
import StandingsView from "@/views/StandingsView.vue";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      redirect: "/fikstur",
    },
    {
      path: "/fikstur",
      name: "fixture",
      component: FixtureView,
    },
    {
      path: "/standings",
      name: "standings",
      component: StandingsView,
    },
  ],
});

export default router;
