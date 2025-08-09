import DefaultTheme from 'vitepress/theme'
import ApiBrowser from './ApiBrowser.vue'
import './custom.css'

export default {
  extends: DefaultTheme,
  enhanceApp({ app }) {
    // Register custom components globally
    app.component('ApiBrowser', ApiBrowser)
  }
}