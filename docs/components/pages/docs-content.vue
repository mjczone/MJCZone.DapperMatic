<script>
const { defineComponent, computed, onMounted } = Vue;

export default defineComponent({
  name: "DocsContent",
  props: ["config", "data"],
  setup(props, ctx) {
    const contentId = computed(() => {
      return (props.data || {}).contentId || "";
    });

    function normalizeHtml(html) {
      const div = document.createElement("div");
      div.innerHTML = html;
      return div.innerHTML;
    }

    const html = computed(() => {
      const h = (props.data || {}).html || "";
      return normalizeHtml(h);
    });

    onMounted(() => {
      if (hljs) {
        hljs.highlightAll();
      }

      // extract the h1 tag and set it as the page title
      const h1 = document.querySelector("h1");
      if (h1) {
        document.title = `${h1.innerText} - ${props.config.title}`;
      }
    });

    return {
      config: props.config,
      html: html,
    };
  },
});
</script>

<template>
  <div class="page content-page">
    <div v-html="html"></div>
  </div>
</template>
