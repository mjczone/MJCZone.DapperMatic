<script>
const { ref, defineComponent, computed } = Vue;
export default defineComponent({
  name: "DocsDrawerLeft",
  props: ["config", "leftDrawerOpen", "selectedItem"],
  emits: ["update:leftDrawerOpen", "update:selectedItem"],
  setup(props, ctx) {
    const leftDrawerOpen = computed({
      get: () => props.leftDrawerOpen,
      set: (v) => ctx.emit("update:leftDrawerOpen", v),
    });
    const selectedItem = computed({
      get: () => props.selectedItem,
      set: (v) => ctx.emit("update:selectedItem", v),
    });

    // load expanded sidebar items from local storage
    const __expanded = Quasar.LocalStorage.getItem("sidebarItemsExpanded");
    const expandedItems = ref(
      Array.isArray(__expanded)
        ? __expanded
        : props.config?.content?.expanded || []
    );
    function expandedUpdated(v) {
      Quasar.LocalStorage.setItem("sidebarItemsExpanded", v);
    }

    return {
      config: props.config,
      leftDrawerOpen,
      selectedItem,
      expandedItems,
      expandedUpdated,
    };
  },
});
</script>

<template>
  <q-drawer
    class="left-drawer"
    show-if-above
    v-model="leftDrawerOpen"
    side="left"
    bordered
  >
    <div
      class="drawer-section-label text-h3 text-weight-regular q-px-md q-pt-md"
    >
      <span>{{ config.content.label || "Content" }}</span>
    </div>
    <div class="q-pa-md q-pt-none q-gutter-sm">
      <q-tree
        dense
        node-key="id"
        no-connectors
        no-selection-unset
        :nodes="config.content.items || []"
        v-model:expanded="expandedItems"
        v-model:selected="selectedItem"
        @update:expanded="expandedUpdated"
      />
    </div>
    <div
      class="drawer-section-label text-h3 text-weight-regular q-px-md q-pt-md"
    >
      <span>{{ config.packages.label || "Packages" }}</span>
    </div>
    <div class="q-pa-md q-pt-none q-gutter-sm">
      <q-tree
        dense
        node-key="id"
        no-connectors
        no-selection-unset
        :nodes="config.packages.items || []"
        v-model:expanded="expandedItems"
        v-model:selected="selectedItem"
        @update:expanded="expandedUpdated"
      />
    </div>
    <div
      class="drawer-section-label text-h3 text-weight-regular q-px-md q-pt-md"
    >
      <span>{{ config.about.label || "About" }}</span>
    </div>
    <div class="q-pa-md q-pt-none q-gutter-sm">
      <q-tree
        dense
        node-key="id"
        no-connectors
        no-selection-unset
        :nodes="config.about.items || []"
        v-model:expanded="expandedItems"
        v-model:selected="selectedItem"
        @update:expanded="expandedUpdated"
      />
    </div>
  </q-drawer>
</template>
