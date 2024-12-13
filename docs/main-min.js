function isScriptLoadedByUrl(e){return Boolean(document.querySelector(`script[src="${e}"]`))}function isStyleLoadedByUrl(e){return Boolean(document.querySelector(`link[href="${e}"]`))}function loadScript(e){return new Promise((function(t,n){let r=null;const o=document.createElement("script");let i="";if("string"==typeof e)i=e;else{if(i=e.src,e.id&&e.id.length){if(r=document.getElementById(e.id),r&&!e.replace)return void t(i);o.id=e.id}o.async=e.async||!1,e.crossorigin&&e.crossorigin.length&&o.setAttribute("crossorigin",e.crossorigin),e.integrity&&e.integrity.length&&o.setAttribute("integrity",e.integrity),e.referrerpolicy&&e.referrerpolicy.length&&o.setAttribute("referrerpolicy",e.referrerpolicy)}o.src=i,isScriptLoadedByUrl(i)?t(i):(o.onload=function(){setTimeout((()=>{t(i)}),0)},o.onerror=function(){setTimeout((()=>{n(o.src)}),0)},r&&r.remove(),document.head.appendChild(o))}))}async function loadScripts(e){const t=[];for(const n of e)try{const e=await loadScript(n);t.push(e)}catch(e){break}return t}function unloadScript(e){if(!e)return;let t=document.getElementById(e);t||(t=document.querySelector(`script[src="${e}"]`)),t&&t.remove()}function loadLink(e){return new Promise((function(t,n){let r=null;const o=document.createElement("link");o.rel="stylesheet",o.type="text/css",o.media="all";let i="";if("string"==typeof e)i=e;else if(i=e.href,e.id&&e.id.length){if(r=document.getElementById(e.id),r&&!e.replace)return void t(i);o.id=e.id}o.href=i,isStyleLoadedByUrl(i)?t(i):(o.onload=function(){setTimeout((()=>{t(i)}),0)},o.onerror=function(){setTimeout((()=>{n(i)}),0)},r&&r.remove(),document.head.appendChild(o))}))}const loadStyle=loadLink;function unloadStyle(e){if(!e)return;let t=document.getElementById(e);t||(t=document.querySelector(`link[href="${e}"]`)),t&&t.remove()}function loadStyles(e){const t=[];return e.forEach((e=>{t.push(loadLink(e))})),Promise.all(t)}async function boot(){await loadStyles(["https://fonts.googleapis.com/css?family=Roboto:100,300,400,500,700,900|Material+Icons","https://cdn.jsdelivr.net/npm/quasar@2.17.4/dist/quasar.prod.css","https://cdn.jsdelivr.net/npm/highlight.js@11.10.0/styles/default.min.css","./docs.css"]),await Promise.all([loadScripts(["https://cdn.jsdelivr.net/npm/vue@3/dist/vue.global.prod.js","https://cdn.jsdelivr.net/npm/vuex@4.1.0/dist/vuex.global.min.js","https://cdn.jsdelivr.net/npm/vue3-sfc-loader/dist/vue3-sfc-loader.js","https://cdn.jsdelivr.net/npm/quasar@2.17.4/dist/quasar.umd.prod.js"]),loadScripts(["https://cdn.jsdelivr.net/npm/lodash@4.17.21/lodash.min.js","https://cdn.jsdelivr.net/gh/highlightjs/cdn-release@11.9.0/build/highlight.min.js",{src:"https://cdn.jsdelivr.net/npm/showdown@2.1.0/dist/showdown.min.js",crossorigin:"anonymous",integrity:"sha256-iOtvu+DCcN3zOEruDJYg0HDgkKJuB8Z0Ia42yQO11kk=",referrerpolicy:"no-referrer"}])])}function createElementFromHtml(e){var t=document.createElement("div");return t.innerHTML=e.trim(),t.firstChild}function extractQueryParam(e,t,n){return new URL(e).searchParams.get(t)||n||null}function docsComponent(e){const t={moduleCache:{vue:Vue},async getFile(e){const t=await fetch(e);if(!t.ok)throw Object.assign(new Error(t.statusText+" "+e),{res:t});return{getContentData:e=>e?t.arrayBuffer():t.text()}},addStyle(e){const t=Object.assign(document.createElement("style"),{textContent:e}),n=document.head.getElementsByTagName("style")[0]||null;document.head.insertBefore(t,n)}};return Vue.defineAsyncComponent((()=>window["vue3-sfc-loader"].loadModule(e,t)))}let markdownConverter=null;function markdownToHtml(e,t){markdownConverter||(showdown.setOption("emoji","true"),showdown.setOption("strikethrough","true"),showdown.setOption("tables","true"),showdown.setOption("tablesHeaderId","true"),showdown.setOption("backslashEscapesHTMLTags","true"),showdown.setOption("excludeTrailingPunctuationFromURLs","true"),showdown.setOption("parseImgDimensions","true"),markdownConverter=new showdown.Converter);try{let n=markdownConverter.makeHtml(e);n=n.replace(/\[x\]/g,'<i class="q-icon notranslate material-icons" aria-hidden="true" role="presentation" style="font-size: 18px;">check_box</i>'),n=n.replace(/\[\s\]/g,'<i class="q-icon notranslate material-icons" aria-hidden="true" role="presentation" style="font-size: 18px;">check_box_outline_blank</i>');const r=(new DOMParser).parseFromString(n,"text/html");return r.querySelectorAll("a").forEach((e=>{let n=(window.location.origin+window.location.pathname).trimEnd("/"),r=e.getAttribute("href");r=r.replace(/^\//,""),r.startsWith("src/")&&t&&t.repositoryUrl&&(r=r.replace("src/",`${t.repositoryUrl.trimEnd("/")}/blob/main/src/`)),r.startsWith("http")&&(e.setAttribute("target","_blank"),e.setAttribute("rel","noopener noreferrer")),r.startsWith("#")||r.startsWith("http")||(r=r.replace(/^#/,""),r=`${n}/#/${r}`),e.setAttribute("href",r)})),r.querySelectorAll("p > img").forEach((e=>{const t=e.getAttribute("alt"),n=e.getAttribute("title"),r=e.getAttribute("src");let o=e.getAttribute("height")||extractQueryParam(r,"h");o&&o.length>0&&!o.endsWith("px")&&!o.endsWith("vh")&&!o.endsWith("%")&&(o+="px");let i=e.getAttribute("width")||extractQueryParam(r,"w");i&&i.length>0&&!i.endsWith("px")&&!i.endsWith("vw")&&!i.endsWith("%")&&(i+="px");let s="";i&&i.length>0&&(s+=`width: ${i};`),o&&o.length>0&&(s+=`height: ${o};`);const a=`\n        <div class="q-img q-img--menu" role="img" style="${s}">\n          <div style="padding-bottom: 66.6864%;"></div>\n          <div class="q-img__container absolute-full">\n            <img class="q-img__image q-img__image--with-transition q-img__image--loaded" \n                  loading="lazy" fetchpriority="auto" aria-hidden="true" draggable="false"\n                  ${i&&i.length>0?`width="${i}"`:""}\n                  ${o&&o.length>0?`height="${o}"`:""}\n                  src="${r}" \n                  title="${n}" \n                  alt="${t}" \n                  style="object-fit: cover; object-position: 50% 50%;"\n              />\n          </div>\n          <div class="q-img__content absolute-full q-anchor--skip">${n?`<div class="absolute-bottom text-subtitle1 text-center">${n}</div>`:""}</div>\n        </div>`;e.parentElement.replaceWith(createElementFromHtml(a))})),n=r.body.innerHTML,n}catch(e){return""}}boot().then((()=>{var e=document.createElement("div");e.id="q-app",e.appendChild(document.createElement("docs-layout")),document.body.prepend(e),window["sfc-loader-options"]={moduleCache:{vue:Vue},async getFile(e){const t=await fetch(e);if(!t.ok)throw Object.assign(new Error(t.statusText+" "+e),{res:t});return{getContentData:e=>e?t.arrayBuffer():t.text()}},addStyle(e){const t=Object.assign(document.createElement("style"),{textContent:e}),n=document.head.getElementsByTagName("style")[0]||null;document.head.insertBefore(t,n)}},fetch("./docs.config.json").then((e=>e.json())).then((e=>{"VERSION_NUMBER"===e.version&&(e.version="v0.0.1-dev");const t=new Vuex.Store({state:{config:e,packages:[]},mutations:{addPackage(e,t){e.packages||(e.packages=[]),e.packages.find((e=>e.name===t.name))||e.packages.push(t)}}}),n=Vue.createApp({components:{docsLayout:docsComponent("./components/docs-layout.vue")},setup:()=>({})});n.use(t),n.use(Quasar,{config:{}}),n.mount("#q-app")}))})).catch((e=>{}));