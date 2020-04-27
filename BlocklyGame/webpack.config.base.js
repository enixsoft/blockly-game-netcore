const webpack = require('webpack');
const path = require('path');
const VueLoaderPlugin = require('vue-loader/lib/plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');

const fontAwesome = 'node_modules/@fortawesome/fontawesome-free/webfonts';
const simpleLineIcons = 'node_modules/simple-line-icons/fonts';

module.exports = {
  target: 'web',
  context: __dirname,
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
    new VueLoaderPlugin(),
    new CopyWebpackPlugin([{ from: path.resolve(__dirname, fontAwesome), to: path.resolve(__dirname, 'wwwroot/fonts') }]),
    new CopyWebpackPlugin([{ from: path.resolve(__dirname, simpleLineIcons), to: path.resolve(__dirname, 'wwwroot/fonts') }]),    
  ]
};
