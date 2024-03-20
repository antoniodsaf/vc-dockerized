<template>
  <Head>
    <link rel="icon" :href="$cfg.favicon_image" />
  </Head>
  <div
    class="__navbar"
    v-if="customNavbarData?.categories?.items?.find((e) => e.properties.find((e) => e.name === 'in_navbar')?.value)"
  >
    <span>You might be interested in:</span>
    <ul>
      <li v-for="i in customNavbarData.categories.items" :key="i.code">
        <router-link v-if="i.properties.find((e) => e.name === 'in_navbar')?.value" :to="`/catalog/${i.path}`">{{
          i.name
        }}</router-link>
      </li>
    </ul>
  </div>
  <component :is="layout">
    <RouterView />
  </component>

  <PopupHost />
  <NotificationsHost />
</template>

<script setup lang="ts">
import { Head } from "@unhead/vue/components";
import { computedEager } from "@vueuse/core";
import { markRaw, onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { setupBroadcastGlobalListeners } from "@/broadcast";
import { useNavigations, usePagesWithFullCartLoad } from "@/core/composables";
import { useCart } from "@/shared/cart";
import { NotificationsHost } from "@/shared/notification";
import { PopupHost } from "@/shared/popup";
import { MainLayout, SecureLayout, useSearchBar } from "./shared/layout";
import type { Component } from "vue";
import type { CategoryConnection } from "./core/api/graphql/types";
import { getMyCategories } from "./core/api/graphql";

/** NOTE: As an example, here is the code for getting the settings from Liquid work context. */
const _props = withDefaults(defineProps<{ settings?: string }>(), { settings: "{}" });
const _settings = JSON.parse(_props.settings); // eslint-disable-line @typescript-eslint/no-unused-vars

const route = useRoute();
const router = useRouter();
const { hideSearchBar, hideSearchDropdown } = useSearchBar();
const { pagesWithFullCartLoad, registerPagesWithFullCartLoad } = usePagesWithFullCartLoad();
const { fetchMenus } = useNavigations();
const { fetchShortCart } = useCart();
const customNavbarData = ref<{ categories: { items: CategoryConnection["items"] } } | null>(null);
const layouts: Record<NonNullable<typeof route.meta.layout>, Component> = {
  Main: markRaw(MainLayout),
  Secure: markRaw(SecureLayout),
};

const layout = computedEager(() => layouts[route.meta?.layout ?? "Main"]);

router.beforeEach((to) => {
  // Hiding the drop-down list of search results
  hideSearchDropdown();

  // Hiding the search bar on mobile devices
  if (to.name !== "Search") {
    hideSearchBar();
  }
});

registerPagesWithFullCartLoad("Cart", "CheckoutDefaults");

fetchMenus();

/**
 * NOTE: Load the short shopping cart.
 * Except for pages that load a full cart.
 */
if (!pagesWithFullCartLoad.has(route.name!)) {
  fetchShortCart();
}

onMounted(async () => {
  setupBroadcastGlobalListeners();
  customNavbarData.value = await getMyCategories({ storeId: "B2B-store" });
});
</script>

<style lang="scss">
@import "assets/styles/main.scss";
.__navbar {
  background-color: #444;
  height: 30px;
  padding: 2px 44px;
  color: white;
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
}
</style>
