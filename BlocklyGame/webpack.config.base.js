const webpack = require('webpack');
const path = require('path');
const VueLoaderPlugin = require('vue-loader/lib/plugin');

module.exports = {
  target: 'web',  
  context: __dirname,
  entry: {
    app: [
      'babel-polyfill',
      path.resolve(__dirname, 'Assets/js/app.js')
    ]
  },
  output: {
    filename: '[name].js',
    path: path.resolve(__dirname, 'Assets/js'),
  },
  resolve: {
    alias: {
      vue$: 'vue/dist/vue.esm.js',
      jQuery: 'jquery/src/jquery',
      $: 'jquery/src/jquery'
    },
    extensions: [".js", ".json", '.vue']
  },
  module: {
    rules: [{
        test: /\.js$/,
        loaders: [
          'babel-loader'
        ],
        include: [
          /js/
        ],
        exclude: [
          /node_modules/
        ]       
      },
      {
        test: /\.vue$/,
        loaders: 'vue-loader',
        options: {
          compilerOptions: {
            preserveWhitespace: false
          }
        }
      }   
    ]
  },
  plugins: [
    new VueLoaderPlugin()  
  ]
};
