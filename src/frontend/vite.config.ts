import { defineConfig, loadEnv } from "vite";
import vue from "@vitejs/plugin-vue";
import tailwindcss from "@tailwindcss/vite";
import { resolve } from "path";

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");

  return {
    plugins: [vue(), tailwindcss()],
    resolve: {
      alias: {
        "@": resolve(__dirname, "src"),
      },
    },
    build: {
      outDir: "../../publish/frontend",
    },
    server: {
      port: 3000,
      proxy: {
        "/api": {
          target: env.VITE_API_BASE_URL || "https://localhost:5000",
          changeOrigin: true,
          secure: false,
        },
      },
    },
  };
});
