<script>
const { defineComponent, ref, computed, onMounted } = Vue;

export default defineComponent({
  name: "DocsPackage",
  props: ["config", "data"],
  setup(props, ctx) {
    const baseUrl = computed(() => {
      const x = window.location.origin + window.location.pathname;
      return x.replace(/\/+$/, "");
    });
    const packageId = computed(() => {
      return (props.data || {}).packageId || "";
    });
    const pkg = computed(() => {
      return (props.data || {}).package || {};
    });
    const pathParts = computed(() => {
      return packageId.value
        .split("/")
        .filter((p) => p.trim().length > 0)
        .map((p) => decodeURIComponent(p));
    });
    const namespaceElements = computed(() => {
      return _.sortBy(
        pkg.value.data.filter((f) => f.elementType === "Namespace"),
        (o) => o.self.displayName
      );
    });
    function getElementsOfType(ns, elementType) {
      const nsItem = pkg.value.data.find(
        (f) => f.elementType === "Namespace" && f.self.displayName === ns
      );
      if (!nsItem) {
        return [];
      }
      return _.sortBy(
        nsItem.types.filter(
          (f) =>
            f.elementType === elementType &&
            f.payloadInfo.accessLevel === "Public"
        ),
        (o) =>
          `${o.payloadInfo.isStatic ? 0 : o.payloadInfo.isAbstract ? 1 : 2}-${
            o.self.displayName
          }`
      );
    }
    function getTextAsHtml(text) {
      // transform <see cref="T:System.Data.IDbConnection" /> to <code>System.Data.IDbConnection</code>
      text = (text || "").replace(
        /<see cref="T:([^"]+)" \/>/g,
        "<code>$1</code>"
      );

      return text;
    }
    const expanded = ref([]);

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
      baseUrl,
      expanded,
      packageId,
      pathParts,
      pkg,
      namespaceElements,
      // functions
      getElementsOfType,
      getTextAsHtml,
      orderBy: (a, b) => {
        return _.sortBy(a, b);
      },
    };
  },
});
</script>

<template>
  <div class="page package-page">
    <h1>
      Package
      <a :href="`${baseUrl}/#/packages/${pathParts[0]}`"
        ><code>{{ pathParts[0] }}</code></a
      >
    </h1>
    <div v-if="pathParts.length === 1">
      <h2>Namespaces</h2>
      <q-markup-table dense flat square>
        <thead>
          <tr>
            <th>Namespace</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="d in namespaceElements" :key="d.self.displayName">
            <td>
              <a
                :href="`${baseUrl}/#/packages/${pathParts[0]}/ns/${d.self.displayName}/`"
                >{{ d.self.displayName }}</a
              >
            </td>
          </tr>
        </tbody>
      </q-markup-table>
    </div>
    <div v-if="pathParts.length === 3 && pathParts[1] === 'ns'">
      <h2>
        Namespace
        <a :href="`${baseUrl}/#/packages/${pathParts[0]}/ns/${pathParts[2]}`"
          ><code>{{ pathParts[2] }}</code></a
        >
      </h2>
      <template v-if="getElementsOfType(pathParts[2], 'Class').length > 0">
        <h3>Classes</h3>
        <q-markup-table dense flat square>
          <thead>
            <tr>
              <th>Name</th>
              <th>Description</th>
              <th>Info</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="d in getElementsOfType(pathParts[2], 'Class')"
              :key="d.name"
            >
              <td>
                <a
                  :href="`${baseUrl}/#/packages/${pathParts[0]}/ns/${pathParts[2]}/t/${d.name}/`"
                  >{{ d.name }}</a
                >
              </td>
              <td v-html="getTextAsHtml(d.summary)"></td>
              <td>
                <q-badge v-if="d.payloadInfo.isStatic" color="dark"
                  >Static</q-badge
                >
                <q-badge v-if="d.payloadInfo.isAbstract" color="grey"
                  >Abstract</q-badge
                >
              </td>
            </tr>
          </tbody>
        </q-markup-table>
      </template>
      <template v-if="getElementsOfType(pathParts[2], 'Interface').length > 0">
        <h3>Interfaces</h3>
        <q-markup-table dense flat square>
          <thead>
            <tr>
              <th>Name</th>
              <th>Description</th>
              <th>Info</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="d in getElementsOfType(pathParts[2], 'Interface')"
              :key="d.self.displayName"
            >
              <td>
                <a
                  :href="`${baseUrl}/#/packages/${pathParts[0]}/ns/${pathParts[2]}/t/${d.self.displayName}/`"
                  >{{ d.self.displayName }}</a
                >
              </td>
              <td v-html="getTextAsHtml(d.summary)"></td>
              <td>
                <q-badge v-if="d.payloadInfo.isStatic" color="dark"
                  >Static</q-badge
                >
              </td>
            </tr>
          </tbody>
        </q-markup-table>
      </template>
      <template v-if="getElementsOfType(pathParts[2], 'Record').length > 0">
        <h3>Records</h3>
        <q-markup-table dense flat square>
          <thead>
            <tr>
              <th>Name</th>
              <th>Description</th>
              <th>Info</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="d in getElementsOfType(pathParts[2], 'Record')"
              :key="d.self.displayName"
            >
              <td>
                <a
                  :href="`${baseUrl}/#/packages/${pathParts[0]}/ns/${pathParts[2]}/t/${d.self.displayName}/`"
                  >{{ d.self.displayName }}</a
                >
              </td>
              <td v-html="getTextAsHtml(d.summary)"></td>
              <td>
                <q-badge v-if="d.payloadInfo.isStatic" color="dark"
                  >Static</q-badge
                >
                <q-badge v-if="d.payloadInfo.isAbstract" color="grey"
                  >Abstract</q-badge
                >
              </td>
            </tr>
          </tbody>
        </q-markup-table>
      </template>
      <template v-if="getElementsOfType(pathParts[2], 'Struct').length > 0">
        <h3>Structs</h3>
        <q-markup-table dense flat square>
          <thead>
            <tr>
              <th>Name</th>
              <th>Description</th>
              <th>Info</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="d in getElementsOfType(pathParts[2], 'Struct')"
              :key="d.self.displayName"
            >
              <td>
                <a
                  :href="`${baseUrl}/#/packages/${pathParts[0]}/ns/${pathParts[2]}/t/${d.self.displayName}/`"
                  >{{ d.self.displayName }}</a
                >
              </td>
              <td v-html="getTextAsHtml(d.summary)"></td>
              <td>
                <q-badge v-if="d.payloadInfo.isStatic" color="dark"
                  >Static</q-badge
                >
                <q-badge v-if="d.payloadInfo.isAbstract" color="grey"
                  >Abstract</q-badge
                >
              </td>
            </tr>
          </tbody>
        </q-markup-table>
      </template>
      <template v-if="getElementsOfType(pathParts[2], 'Enum').length > 0">
        <h3>Enums</h3>
        <q-markup-table dense flat square>
          <thead>
            <tr>
              <th>Name</th>
              <th>Description</th>
              <th>Info</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="d in getElementsOfType(pathParts[2], 'Enum')"
              :key="d.self.displayName"
            >
              <td>
                <a
                  :href="`${baseUrl}/#/packages/${pathParts[0]}/ns/${pathParts[2]}/t/${d.self.displayName}/`"
                  >{{ d.self.displayName }}</a
                >
              </td>
              <td v-html="getTextAsHtml(d.summary)"></td>
              <td>
                <template
                  v-for="m in d.enumMembers.filter((e) => e.name !== 'value__')"
                  :key="m.name"
                >
                  <q-badge class="q-mr-xs" color="grey">{{ m.name }}</q-badge>
                </template>
              </td>
            </tr>
          </tbody>
        </q-markup-table>
      </template>
    </div>
    <div
      v-if="
        pathParts.length === 5 && pathParts[1] === 'ns' && pathParts[3] === 't'
      "
      v-for="type in getElementsOfType(pathParts[2], 'Class')
        .concat(
          getElementsOfType(pathParts[2], 'Interface'),
          getElementsOfType(pathParts[2], 'Record'),
          getElementsOfType(pathParts[2], 'Struct'),
          getElementsOfType(pathParts[2], 'Enum')
        )
        .filter(
          (f) =>
            f.name === pathParts[4] &&
            (f.namespace || {}).displayName === pathParts[2]
        )"
      :key="type.self.displayName"
    >
      <h2>
        Type
        <a
          :href="`${baseUrl}/#/packages/${pathParts[0]}/ns/${pathParts[2]}/${pathParts[3]}/${pathParts[4]}`"
          ><code>{{ type.name }}</code></a
        >
        ( Namespace
        <a :href="`${baseUrl}/#/packages/${pathParts[0]}/ns/${pathParts[2]}`"
          ><code>{{ type.namespace.displayName }}</code></a
        >
        )
      </h2>
      <p v-html="getTextAsHtml(type.summary)"></p>
      <template v-if="(type.enumMembers || []).length > 0">
        <h3>Enum Members</h3>
        <q-markup-table dense flat square>
          <thead>
            <tr>
              <th>Name</th>
              <th>Value</th>
              <th>Description</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="(m, mi) in type.enumMembers.filter(
                (e) => e.name !== 'value__'
              )"
              :key="m.name"
            >
              <td>{{ m.name }}</td>
              <td>{{ mi }}</td>
              <td v-html="getTextAsHtml(m.summary)"></td>
            </tr>
          </tbody>
        </q-markup-table>
      </template>
      <template v-if="(type.properties || []).length > 0">
        <h3>Properties</h3>
        <q-markup-table dense flat square>
          <thead>
            <tr>
              <th>Name</th>
              <th>Type</th>
              <th>Description</th>
              <th>Info</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in type.properties" :key="p.name">
              <td>{{ p.name }}</td>
              <td>
                <code>{{ p.returnType }}</code>
              </td>
              <td v-html="getTextAsHtml(p.summary)"></td>
              <td>
                <q-badge v-if="p.payloadInfo.isStatic" color="dark"
                  >Static</q-badge
                >
                <q-badge v-if="p.payloadInfo.isAbstract" color="grey"
                  >Abstract</q-badge
                >
                <q-badge v-if="p.payloadInfo.isOverride" color="grey"
                  >Override</q-badge
                >
                <q-badge v-if="p.payloadInfo.isVirtual" color="grey"
                  >Virtual</q-badge
                >
                <q-badge v-if="p.payloadInfo.isReadOnlyField" color="grey"
                  >ReadOnly</q-badge
                >
              </td>
            </tr>
          </tbody>
        </q-markup-table>
      </template>
      <template v-if="(type.fields || []).length > 0">
        <h3>Fields</h3>
        <q-markup-table dense flat square>
          <thead>
            <tr>
              <th>Name</th>
              <th>Description</th>
              <th>Info</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="p in orderBy(
                type.fields.filter(
                  (f) => f.payloadInfo.accessLevel === 'Public'
                ),
                (x) => x.name
              )"
              :key="p.name"
            >
              <td>{{ p.name }}</td>
              <td v-html="getTextAsHtml(p.summary)"></td>
              <td>
                <q-badge v-if="p.payloadInfo.isStatic" color="dark"
                  >Static</q-badge
                >
                <q-badge v-if="p.payloadInfo.isAbstract" color="grey"
                  >Abstract</q-badge
                >
                <q-badge v-if="p.payloadInfo.isOverride" color="grey"
                  >Override</q-badge
                >
                <q-badge v-if="p.payloadInfo.isVirtual" color="grey"
                  >Virtual</q-badge
                >
                <q-badge v-if="p.payloadInfo.isReadOnlyField" color="grey"
                  >ReadOnly</q-badge
                >
              </td>
            </tr>
          </tbody>
        </q-markup-table>
      </template>
      <template v-if="(type.methods || []).length > 0">
        <h3>Methods</h3>
        <q-markup-table dense flat square>
          <thead>
            <tr>
              <th>Name</th>
              <th>Return Type</th>
              <th>Parameters</th>
              <th>Description</th>
              <th>Info</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="m in orderBy(type.methods, (x) => x.name)" :key="m.name">
              <td colspan="2" v-show="expanded.includes(m.self.displayName)">
                <div>
                  <code>{{ m.returnType }}</code> {{ m.self.displayName }}
                </div>
                <ul>
                  <li v-for="p in m.parameters" :key="p.name">
                    <code>{{ p.name }}</code
                    >: {{ p.text }}
                  </li>
                </ul>
              </td>
              <td v-show="!expanded.includes(m.self.displayName)">
                {{ m.title }}
              </td>
              <td v-show="!expanded.includes(m.self.displayName)">
                <code>{{ m.returnType }}</code>
              </td>
              <td>
                <q-toggle
                  dense
                  size="xs"
                  :modelValue="expanded.includes(m.self.displayName)"
                  @update:model-value="
                    (v) =>
                      v
                        ? expanded.push(m.self.displayName)
                        : expanded.splice(
                            expanded.indexOf(m.self.displayName),
                            1
                          )
                  "
                  :label="
                    expanded.includes(m.self.displayName) ? 'Hide' : 'Show'
                  "
                ></q-toggle>
              </td>
              <td v-html="getTextAsHtml(m.summary)"></td>
              <td>
                <q-badge v-if="m.payloadInfo.isStatic" color="dark"
                  >Static</q-badge
                >
                <q-badge v-if="m.payloadInfo.isAbstract" color="grey"
                  >Abstract</q-badge
                >
                <q-badge v-if="m.payloadInfo.isOverride" color="grey"
                  >Override</q-badge
                >
                <q-badge v-if="m.payloadInfo.isVirtual" color="grey"
                  >Virtual</q-badge
                >
                <q-badge v-if="m.payloadInfo.isExtensionMethod" color="grey"
                  >Extension</q-badge
                >
              </td>
            </tr>
          </tbody>
        </q-markup-table>
      </template>
    </div>
  </div>
</template>
