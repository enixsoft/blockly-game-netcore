const webpack = require('webpack');
const merge = require('webpack-merge');
const baseConfig = require('./webpack.config.base');
const path = require('path');
const FriendlyErrorsPlugin = require('friendly-errors-webpack-plugin');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');

const devMode = process.env.NODE_ENV !== 'production';
const DEV_HOST = 'localhost';
const DEV_PORT = 3000;

module.exports = merge(baseConfig, {
  mode: 'development',
  entry: {
    app: [
      'babel-polyfill',
      path.resolve(__dirname, 'Assets/js/app.js')
    ]
  },
  output: {
    filename: '[name].js',
    path: path.resolve(__dirname, 'Assets/HMR'),
    publicPath: `http://${DEV_HOST}:${DEV_PORT}/`
  },
  devServer: {
    contentBase: './Assets/HMR',
    hot: true,
    host: DEV_HOST,
    port: DEV_PORT,
    compress: true,
    headers: {
      'Access-Control-Allow-Origin': '*'
    }
  },
  devtool: '#cheap-module-eval-source-map',

  plugins: [
    new CleanWebpackPlugin({
      cleanOnceBeforeBuildPatterns: path.resolve(__dirname, 'Assets/HMR')
    }),
    new webpack.HotModuleReplacementPlugin(),
    new webpack.NoEmitOnErrorsPlugin(),
    new FriendlyErrorsPlugin(),

    new MiniCssExtractPlugin({
      filename: devMode ? '[name].css' : '[name].[hash].css',
      chunkFilename: devMode ? '[id].css' : '[id].[hash].css',
      publicPath: 'Assets/sass'
    }),
  ],
  module: {
    rules: [{
      test: /\.scss/,
      use: [
        {
          loader: MiniCssExtractPlugin.loader,
          options: {
            hmr: process.env.NODE_ENV === 'development',
          },
        },
        'css-loader',
        'postcss-loader',
        'sass-loader'
      ]
    },
    {
      test: /\.css/,
      use: [
        {
          loader: MiniCssExtractPlugin.loader,
          options: {
            hmr: process.env.NODE_ENV === 'development',
          },
        },
        'css-loader',
        'postcss-loader'
      ]
    },
    {
      test: /\.(png|jpg|jpeg|svg|woff|woff2|ttf|eot|ico)$/,
      loader: 'url-loader',
      options: {
        name: '[name].[ext]?[hash]',
        limit: 10000,
      },
    }]
  }
});
