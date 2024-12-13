<template>
  <q-layout view="hHh LpR fff" class="bg-transparent">
    <docs-header
      :title="$store.state.config.title"
      :repository-url="$store.state.config.repositoryUrl"
      :version="$store.state.config.version"
      @toggle-left-drawer="toggleLeftDrawer"
    ></docs-header>
    <docs-drawer
      :config="$store.state.config"
      v-model:left-drawer-open="leftDrawerOpen"
      v-model:selected-item="selectedPath"
    ></docs-drawer>
    <q-page-container>
      <q-page v-if="displayComponent" :style-fn="qPageStyleFn">
        <component
          :is="displayComponent"
          :config="$store.state.config"
          :data="data"
        ></component>
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script>
const { ref, defineComponent, computed, watch } = Vue;
const { useStore } = Vuex;

export default defineComponent({
  name: "App",
  components: {
    "docs-header": docsComponent("./components/docs-header.vue"),
    "docs-drawer": docsComponent("./components/docs-drawer.vue"),
    "docs-package": docsComponent("./components/pages/docs-package.vue"),
    "docs-content": docsComponent("./components/pages/docs-content.vue"),
  },
  setup() {
    const store = useStore();

    const leftDrawerOpen = ref(false);
    function toggleLeftDrawer() {
      leftDrawerOpen.value = !leftDrawerOpen.value;
    }

    const data = ref({});
    const displayComponent = computed(() => {
      if (!data.value) {
        return null;
      } else if ((data.value.packageId || "").length > 0) {
        return "docs-package";
      } else if ((data.value.contentId || "").length > 0) {
        return "docs-content";
      }
      return null;
    });

    function normalizePath(path) {
      return path
        .replace(/^\//, "")
        .replace(/^#/, "")
        .replace(/^\//, "")
        .trimEnd("/");
    }

    async function loadPackageData(packageId) {
      packageId = normalizePath(packageId);

      // the package id can be a/b/c, so only want the first part to load via fetch
      const parts = packageId.split("/");
      data.value = null;
      try {
        const response = await fetch(`./packages/${parts[0]}.json`);
        const pkg = await response.json();
        pkg.label = parts[0];
        store.commit("addPackage", pkg);

        data.value = { packageId, package: pkg };
      } catch (error) {
        console.error(error);

        // load 404 page
        await loadContentData("404");
      }
    }

    async function loadContentData(contentId) {
      contentId = normalizePath(contentId);

      if (!!!contentId) {
        contentId = "index";
      }
      // console.log("loadContentData", contentId);
      data.value = null;
      try {
        const response = await fetch(`./content/${contentId}.md`);
        if (!response.ok) {
          if (response.status === 404 && contentId !== "404") {
            // load 404 page
            await loadContentData("404");
            return;
          }
        }
        const markdown = await response.text();
        data.value = {
          contentId,
          html: markdownToHtml(markdown, {
            repositoryUrl: store.state.config.repositoryUrl,
          }),
        };
      } catch (error) {
        console.error(error);
      }
    }

    /**
     * Current path
     *
     * This is a reactive variable that holds the current path and
     * is always of the format a/b/c without the hash or leading slash
     * or trailing slash.
     */
    const currentPath = ref(normalizePath(window.location.hash || ""));
    window.addEventListener("hashchange", () => {
      currentPath.value = normalizePath(window.location.hash || "");
      if (currentPath.value !== selectedPath.value) {
        selectedPath.value = currentPath.value;
      }
    });
    function updateLocation(path) {
      window.location.hash = `#/${normalizePath(path)}`;
    }

    /**
     * Selected path
     *
     * This is a reactive variable that holds the selected path and
     * is always of the format a/b/c without the hash or leading slash
     * or trailing slash.
     *
     * When this path changes, the browser redirects to the new path,
     * which in turn updates the current path.
     */
    const selectedPath = ref(currentPath.value);
    watch(
      () => selectedPath.value,
      (path) => {
        const item = normalizePath(path);
        // update the hash location
        updateLocation(item);

        // load the relevant data
        if (item === "packages") {
          // loadPackagesData(item);
        } else if (item.startsWith("packages/")) {
          // strip packages/ from the path
          loadPackageData(item.replace(/^packages\//, ""));
        } else {
          loadContentData(item);
        }
      },
      { immediate: true }
    );

    function qPageStyleFn(offset, height) {
      // console.log("qPageStyleFn", offset, height);
      // return { minHeight: offset + "px" };
      return {};
    }

    return {
      qPageStyleFn,
      displayComponent,
      data,
      leftDrawerOpen,
      selectedPath,
      toggleLeftDrawer,
    };
  },
});
</script>
