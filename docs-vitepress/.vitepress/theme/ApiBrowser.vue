<template>
  <div class="api-browser">
    <div v-if="loading" class="loading">Loading API documentation...</div>
    <div v-else-if="error" class="error">{{ error }}</div>
    <div v-else-if="apiData">
      <div class="namespaces">
        <h2>Namespaces</h2>
        <div v-for="ns in namespaces" :key="ns.self.url" class="namespace-section">
          <h3>{{ ns.self.displayName }}</h3>
          
          <div v-if="getClasses(ns).length > 0" class="type-section">
            <h4>Classes</h4>
            <ul>
              <li v-for="type in getClasses(ns)" :key="type.commentId">
                <a :href="`#${type.commentId}`" @click="selectedType = type">
                  {{ type.name }}
                </a>
                <span v-if="type.payloadInfo.isStatic" class="badge static">static</span>
                <span v-if="type.payloadInfo.isAbstract" class="badge abstract">abstract</span>
              </li>
            </ul>
          </div>

          <div v-if="getEnums(ns).length > 0" class="type-section">
            <h4>Enums</h4>
            <ul>
              <li v-for="type in getEnums(ns)" :key="type.commentId">
                <a :href="`#${type.commentId}`" @click="selectedType = type">
                  {{ type.name }}
                </a>
              </li>
            </ul>
          </div>
        </div>
      </div>

      <div v-if="selectedType" class="type-details">
        <h2>{{ selectedType.name }}</h2>
        <div v-if="selectedType.summary" class="summary" v-html="formatDocumentation(selectedType.summary)"></div>
        
        <div v-if="selectedType.methods && selectedType.methods.length > 0" class="members-section">
          <h3>Methods</h3>
          <div v-for="method in selectedType.methods" :key="method.commentId" class="member">
            <h4>{{ method.name }}</h4>
            <div v-if="method.summary" class="member-summary" v-html="formatDocumentation(method.summary)"></div>
            <pre class="signature"><code>{{ getMethodSignature(method) }}</code></pre>
          </div>
        </div>

        <div v-if="selectedType.properties && selectedType.properties.length > 0" class="members-section">
          <h3>Properties</h3>
          <div v-for="prop in selectedType.properties" :key="prop.commentId" class="member">
            <h4>{{ prop.name }}</h4>
            <div v-if="prop.summary" class="member-summary" v-html="formatDocumentation(prop.summary)"></div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'

const loading = ref(true)
const error = ref(null)
const apiData = ref(null)
const selectedType = ref(null)

const namespaces = computed(() => {
  if (!apiData.value) return []
  return apiData.value.data || []
})

function getClasses(namespace) {
  return namespace.types.filter(t => 
    t.elementType === 'Class' && 
    t.payloadInfo.accessLevel === 'Public'
  )
}

function getEnums(namespace) {
  return namespace.types.filter(t => 
    t.elementType === 'Enum' && 
    t.payloadInfo.accessLevel === 'Public'
  )
}

function formatDocumentation(text) {
  if (!text) return ''
  // Convert <see cref="T:TypeName" /> to inline code
  return text.replace(/<see cref="T:([^"]+)" \/>/g, '<code>$1</code>')
}

function getMethodSignature(method) {
  const params = method.parameters || []
  const paramStr = params.map(p => `${p.type} ${p.name}`).join(', ')
  return `${method.returnType || 'void'} ${method.name}(${paramStr})`
}

onMounted(async () => {
  try {
    // In VitePress, we need to handle the base URL correctly
    const base = import.meta.env.BASE_URL || '/'
    const response = await fetch(`${base}packages/MJCZone.DapperMatic.json`)
    if (!response.ok) {
      throw new Error('Failed to load API documentation')
    }
    apiData.value = await response.json()
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.api-browser {
  margin-top: 2rem;
}

.loading, .error {
  padding: 1rem;
  border-radius: 4px;
  margin: 1rem 0;
}

.loading {
  background: var(--vp-c-bg-soft);
  color: var(--vp-c-text-2);
}

.error {
  background: var(--vp-c-danger-soft);
  color: var(--vp-c-danger);
}

.namespaces h2 {
  margin-top: 2rem;
  margin-bottom: 1rem;
}

.namespace-section {
  margin-bottom: 2rem;
}

.type-section {
  margin-left: 1rem;
  margin-bottom: 1rem;
}

.type-section h4 {
  margin-top: 1rem;
  margin-bottom: 0.5rem;
}

.type-section ul {
  list-style: none;
  padding-left: 0;
}

.type-section li {
  padding: 0.25rem 0;
}

.badge {
  display: inline-block;
  padding: 0.125rem 0.375rem;
  font-size: 0.75rem;
  border-radius: 3px;
  margin-left: 0.5rem;
}

.badge.static {
  background: var(--vp-c-brand-soft);
  color: var(--vp-c-brand);
}

.badge.abstract {
  background: var(--vp-c-warning-soft);
  color: var(--vp-c-warning);
}

.type-details {
  margin-top: 3rem;
  padding-top: 2rem;
  border-top: 1px solid var(--vp-c-divider);
}

.summary, .member-summary {
  margin: 1rem 0;
  color: var(--vp-c-text-2);
}

.members-section {
  margin-top: 2rem;
}

.member {
  margin-bottom: 1.5rem;
  padding-bottom: 1.5rem;
  border-bottom: 1px solid var(--vp-c-divider-light);
}

.member:last-child {
  border-bottom: none;
}

.signature {
  background: var(--vp-c-bg-soft);
  padding: 0.75rem 1rem;
  border-radius: 4px;
  overflow-x: auto;
}

.signature code {
  font-size: 0.875rem;
}
</style>