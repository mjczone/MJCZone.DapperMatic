// functions: isScriptLoadedByUrl, isStyleLoadedByUrl, loadScript, loadScripts, unloadScript, loadLink, loadStyle, unloadStyle, loadStyles
function isScriptLoadedByUrl(e) {
  return Boolean(document.querySelector(`script[src="${e}"]`));
}
function isStyleLoadedByUrl(e) {
  return Boolean(document.querySelector(`link[href="${e}"]`));
}
function loadScript(e) {
  return new Promise(function (t, n) {
    let o = null;
    const r = document.createElement("script");
    let i = "";
    if ("string" == typeof e) i = e;
    else {
      if (((i = e.src), e.id && e.id.length)) {
        if (((o = document.getElementById(e.id)), o && !e.replace))
          return void t(i);
        r.id = e.id;
      }
      (r.async = e.async || !1),
        e.crossorigin &&
          e.crossorigin.length &&
          r.setAttribute("crossorigin", e.crossorigin),
        e.integrity &&
          e.integrity.length &&
          r.setAttribute("integrity", e.integrity),
        e.referrerpolicy &&
          e.referrerpolicy.length &&
          r.setAttribute("referrerpolicy", e.referrerpolicy);
    }
    (r.src = i),
      isScriptLoadedByUrl(i)
        ? t(i)
        : ((r.onload = function () {
            setTimeout(() => {
              t(i);
            }, 0);
          }),
          (r.onerror = function () {
            setTimeout(() => {
              n(r.src);
            }, 0);
          }),
          o && o.remove(),
          document.head.appendChild(r));
  });
}
async function loadScripts(e) {
  const t = [];
  for (const n of e)
    try {
      const e = await loadScript(n);
      t.push(e);
    } catch (e) {
      break;
    }
  return t;
}
function unloadScript(e) {
  if (!e) return;
  let t = document.getElementById(e);
  t || (t = document.querySelector(`script[src="${e}"]`)), t && t.remove();
}
function loadLink(e) {
  return new Promise(function (t, n) {
    let o = null;
    const r = document.createElement("link");
    (r.rel = "stylesheet"), (r.type = "text/css"), (r.media = "all");
    let i = "";
    if ("string" == typeof e) i = e;
    else if (((i = e.href), e.id && e.id.length)) {
      if (((o = document.getElementById(e.id)), o && !e.replace))
        return void t(i);
      r.id = e.id;
    }
    (r.href = i),
      isStyleLoadedByUrl(i)
        ? t(i)
        : ((r.onload = function () {
            setTimeout(() => {
              t(i);
            }, 0);
          }),
          (r.onerror = function () {
            setTimeout(() => {
              n(i);
            }, 0);
          }),
          o && o.remove(),
          document.head.appendChild(r));
  });
}
const loadStyle = loadLink;
function unloadStyle(e) {
  if (!e) return;
  let t = document.getElementById(e);
  t || (t = document.querySelector(`link[href="${e}"]`)), t && t.remove();
}
function loadStyles(e) {
  const t = [];
  return (
    e.forEach((e) => {
      t.push(loadLink(e));
    }),
    Promise.all(t)
  );
}

async function boot() {
  await loadStyles([
    "https://fonts.googleapis.com/css?family=Roboto:100,300,400,500,700,900|Material+Icons",
    "https://cdn.jsdelivr.net/npm/quasar@2.17.4/dist/quasar.prod.css",
    "https://cdn.jsdelivr.net/npm/highlight.js@11.10.0/styles/default.min.css",
    "./docs.css",
  ]);
  // process loaders in parallel
  await Promise.all([
    loadScripts([
      "https://cdn.jsdelivr.net/npm/vue@3/dist/vue.global.prod.js",
      "https://cdn.jsdelivr.net/npm/vuex@4.1.0/dist/vuex.global.min.js",
      "https://cdn.jsdelivr.net/npm/vue3-sfc-loader/dist/vue3-sfc-loader.js",
      "https://cdn.jsdelivr.net/npm/quasar@2.17.4/dist/quasar.umd.prod.js",
    ]),
    loadScripts([
      "https://cdn.jsdelivr.net/npm/lodash@4.17.21/lodash.min.js",
      "https://cdn.jsdelivr.net/gh/highlightjs/cdn-release@11.9.0/build/highlight.min.js",
      {
        src: "https://cdn.jsdelivr.net/npm/showdown@2.1.0/dist/showdown.min.js",
        crossorigin: "anonymous",
        integrity: "sha256-iOtvu+DCcN3zOEruDJYg0HDgkKJuB8Z0Ia42yQO11kk=",
        referrerpolicy: "no-referrer",
      },
    ]),
  ]);
}

// from: https://stackoverflow.com/questions/1129216/sort-array-of-objects-by-string-property-value
// example: People.sort(dynamicSort("Name"));
function dynamicSort(property) {
  var sortOrder = 1;
  if (property[0] === "-") {
    sortOrder = -1;
    property = property.substr(1);
  }
  return function (a, b) {
    /*
     * next line works with strings and numbers,
     * and you may want to customize it to your needs
     */
    var result =
      a[property] < b[property] ? -1 : a[property] > b[property] ? 1 : 0;
    return result * sortOrder;
  };
}

/**
 * Create a DOM element from an HTML string with a single root element
 */
function createElementFromHtml(html) {
  var div = document.createElement("div");
  div.innerHTML = html.trim();
  return div.firstChild;
}

function extractQueryParam(url, key, defaultValue) {
  return new URL(url).searchParams.get(key) || defaultValue || null;
}

function docsComponent(componentUrl) {
  const options = {
    moduleCache: {
      vue: Vue,
    },
    async getFile(url) {
      const res = await fetch(url);
      if (!res.ok)
        throw Object.assign(new Error(res.statusText + " " + url), { res });
      return {
        getContentData: (asBinary) =>
          asBinary ? res.arrayBuffer() : res.text(),
      };
    },
    addStyle(textContent) {
      const style = Object.assign(document.createElement("style"), {
        textContent,
      });
      const ref = document.head.getElementsByTagName("style")[0] || null;
      document.head.insertBefore(style, ref);
    },
  };
  return Vue.defineAsyncComponent(() =>
    window["vue3-sfc-loader"].loadModule(componentUrl, options)
  );
}

let markdownConverter = null;
function markdownToHtml(markdown, config) {
  if (!markdownConverter) {
    // setup the showdown markdown converter
    showdown.setOption("emoji", "true");
    showdown.setOption("strikethrough", "true");
    showdown.setOption("tables", "true");
    showdown.setOption("tablesHeaderId", "true");
    showdown.setOption("backslashEscapesHTMLTags", "true");
    showdown.setOption("excludeTrailingPunctuationFromURLs", "true");
    showdown.setOption("parseImgDimensions", "true");
    markdownConverter = new showdown.Converter();
  }
  try {
    let html = markdownConverter.makeHtml(markdown);

    // replace [x] with <input type="checkbox" disabled> and
    //         [ ] with <input type="checkbox" disabled checked>
    // html = html.replace(/\[x\]/g, '<input type="checkbox" disabled checked>');
    html = html.replace(
      /\[x\]/g,
      '<i class="q-icon notranslate material-icons" aria-hidden="true" role="presentation" style="font-size: 18px;">check_box</i>'
    );
    // html = html.replace(/\[\s\]/g, '<input type="checkbox" disabled>');
    html = html.replace(
      /\[\s\]/g,
      '<i class="q-icon notranslate material-icons" aria-hidden="true" role="presentation" style="font-size: 18px;">check_box_outline_blank</i>'
    );

    // WARNING: the following is a SPECIFIC implementation for this docs website
    // parse the html string into a DOM object
    // convert all relative links to absolute url hash links
    const parser = new DOMParser();
    const doc = parser.parseFromString(html, "text/html");
    doc.querySelectorAll("a").forEach((a) => {
      let href = a.getAttribute("href");

      // strip slash from the beginning of the href using regex
      href = href.replace(/^\//, "");

      if (href.startsWith("src/") && config && config.repositoryUrl) {
        href = href.replace(
          "src/",
          `${config.repositoryUrl.trimEnd("/")}/blob/main/src/`
        );
      }
      if (href.startsWith("http")) {
        a.setAttribute("target", "_blank");
        a.setAttribute("rel", "noopener noreferrer");
      }
      if (!href.startsWith("#") && !href.startsWith("http")) {
        // strip hash from the beginning of the href using regex
        href = href.replace(/^#/, "");
        // make the href an absolute url hash link
        href = `${window.location.origin}/#/${href}`;
      }

      // set the href
      a.setAttribute("href", href);
    });
    // const replaceImgs = [];
    doc.querySelectorAll("p > img").forEach((img) => {
      const alt = img.getAttribute("alt");
      const title = img.getAttribute("title");
      const src = img.getAttribute("src");
      let h = img.getAttribute("height") || extractQueryParam(src, "h");
      if (
        h &&
        h.length > 0 &&
        !h.endsWith("px") &&
        !h.endsWith("vh") &&
        !h.endsWith("%")
      ) {
        h = h + "px";
      }
      let w = img.getAttribute("width") || extractQueryParam(src, "w");
      if (
        w &&
        w.length > 0 &&
        !w.endsWith("px") &&
        !w.endsWith("vw") &&
        !w.endsWith("%")
      ) {
        w = w + "px";
      }
      let styles = "";
      if (w && w.length > 0) styles += `width: ${w};`;
      if (h && h.length > 0) styles += `height: ${h};`;
      const newHtml = `
        <div class="q-img q-img--menu" role="img" style="${styles}">
          <div style="padding-bottom: 66.6864%;"></div>
          <div class="q-img__container absolute-full">
            <img class="q-img__image q-img__image--with-transition q-img__image--loaded" 
                  loading="lazy" fetchpriority="auto" aria-hidden="true" draggable="false"
                  ${w && w.length > 0 ? `width="${w}"` : ""}
                  ${h && h.length > 0 ? `height="${h}"` : ""}
                  src="${src}" 
                  title="${title}" 
                  alt="${alt}" 
                  style="object-fit: cover; object-position: 50% 50%;"
              />
          </div>
          <div class="q-img__content absolute-full q-anchor--skip">${
            !!title
              ? `<div class="absolute-bottom text-subtitle1 text-center">${title}</div>`
              : ""
          }</div>
        </div>`;
      img.parentElement.replaceWith(createElementFromHtml(newHtml));
    });
    html = doc.body.innerHTML;
    return html;
  } catch (e) {
    console.error("Unable to convert markdown to html", { markdown, error: e });
    return "";
  }
}

boot()
  .then(() => {
    var a = document.createElement("div");
    a.id = "q-app";
    a.appendChild(document.createElement("docs-layout"));
    document.body.prepend(a);
    window["sfc-loader-options"] = {
      moduleCache: {
        vue: Vue,
      },
      async getFile(url) {
        const res = await fetch(url);
        if (!res.ok)
          throw Object.assign(new Error(res.statusText + " " + url), { res });
        return {
          getContentData: (asBinary) =>
            asBinary ? res.arrayBuffer() : res.text(),
        };
      },
      addStyle(textContent) {
        const style = Object.assign(document.createElement("style"), {
          textContent,
        });
        const ref = document.head.getElementsByTagName("style")[0] || null;
        document.head.insertBefore(style, ref);
      },
    };

    fetch("./docs.config.json")
      .then((response) => response.json())
      .then((config) => {
        if (config.version === "VERSION_NUMBER") {
          config.version = "v0.0.1-dev";
        }
        const vuex_store = new Vuex.Store({
          state: {
            config,
            packages: [],
          },
          mutations: {
            addPackage(state, package) {
              if (!state.packages) state.packages = [];
              if (!state.packages.find((p) => p.name === package.name)) {
                state.packages.push(package);
              }
            },
          },
        });
        const app = Vue.createApp({
          components: {
            docsLayout: docsComponent("./components/docs-layout.vue"),
          },
          setup() {
            return {};
          },
        });
        app.use(vuex_store);
        app.use(Quasar, {
          config: {
            // brand: {},
            // ...
          },
        });
        app.mount("#q-app");
      });
  })
  .catch((err) => {
    console.error("Docs failed to load:", err);
  });
