import { defineConfig } from 'vitepress'

export default defineConfig({
  title: 'DapperMatic',
  description: 'IDbConnection extension methods for DDL operations across multiple database providers',
  
  themeConfig: {
    logo: '/favicon.ico',
    
    nav: [
      { text: 'Guide', link: '/guide/getting-started' },
      { text: 'API Reference', link: '/api/' },
      { text: 'GitHub', link: 'https://github.com/mjczone/MJCZone.DapperMatic' }
    ],

    sidebar: {
      '/guide/': [
        {
          text: 'Getting Started',
          collapsed: false,
          items: [
            { text: 'Installation', link: '/guide/installation' },
            { text: 'Providers', link: '/guide/providers' },
            { text: 'Models', link: '/guide/models' },
            { text: 'Data Annotations', link: '/guide/data-annotations' },
            { text: 'Configuration', link: '/guide/configuration' }
          ]
        },
        {
          text: 'Usage',
          collapsed: false,
          items: [
            { text: 'Extension Methods', link: '/guide/extension-methods/' },
            { text: 'General Methods', link: '/guide/extension-methods/general-methods' },
            { text: 'Schema Methods', link: '/guide/extension-methods/schema-methods' },
            { text: 'Table Methods', link: '/guide/extension-methods/table-methods' },
            { text: 'Column Methods', link: '/guide/extension-methods/column-methods' },
            { text: 'Primary Key Methods', link: '/guide/extension-methods/primary-key-constraint-methods' },
            { text: 'Check Constraint Methods', link: '/guide/extension-methods/check-constraint-methods' },
            { text: 'Default Constraint Methods', link: '/guide/extension-methods/default-constraint-methods' },
            { text: 'Foreign Key Methods', link: '/guide/extension-methods/foreign-key-constraint-methods' },
            { text: 'Unique Constraint Methods', link: '/guide/extension-methods/unique-constraint-methods' },
            { text: 'Index Methods', link: '/guide/extension-methods/index-methods' },
            { text: 'View Methods', link: '/guide/extension-methods/view-methods' },
            { text: 'Testing', link: '/guide/testing' }
          ]
        },
        {
          text: 'About',
          collapsed: true,
          items: [
            { text: 'Roadmap', link: '/guide/roadmap' }
          ]
        }
      ],
      '/api/': [
        {
          text: 'API Reference',
          items: [
            { text: 'Overview', link: '/api/' },
            { text: 'MJCZone.DapperMatic', link: '/api/mjczone-dappermatic' }
          ]
        }
      ]
    },

    socialLinks: [
      { icon: 'github', link: 'https://github.com/mjczone/MJCZone.DapperMatic' }
    ],

    search: {
      provider: 'local'
    }
  }
})