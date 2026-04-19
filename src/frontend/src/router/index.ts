import { createRouter, createWebHistory } from "vue-router";
import FixtureView from "@/views/volleyball/FixtureView.vue";
import StandingsView from "@/views/volleyball/StandingsView.vue";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "home",
      redirect: { name: "volleyball-fixture" },
    },
    {
      path: "/voleybol",
      name: "volleyball",
      redirect: { name: "volleyball-fixture" },
      children: [
        {
          path: "fikstur",
          name: "volleyball-fixture",
          component: FixtureView,
        },
        {
          path: "puan-durumu",
          name: "volleyball-standings",
          component: StandingsView,
        },
      ],
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
