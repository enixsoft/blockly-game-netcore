const webpack = require('webpack');
const merge = require('webpack-merge');
const path = require('path');
const baseConfig = require('./webpack.config.base');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const OptimizeCssAssetsPlugin = require('optimize-css-assets-webpack-plugin');
const TerserPlugin = require('terser-webpack-plugin');

module.exports = merge(baseConfig, {
    mode: 'production',
    devtool: 'cheap-module-source-map', 
    output: {
        filename: '[name].[contentHash].bundle.js',
        path: path.resolve(__dirname, "wwwroot/dist")
    },
    optimization: {
        minimizer: [
            new OptimizeCssAssetsPlugin(),
            new TerserPlugin({
                terserOptions: {
                    compress: {
                      drop_console: true,
                    }
                }
            })
        ]
    },
    plugins: [
        new CleanWebpackPlugin(), new MiniCssExtractPlugin({filename: "[name].[contentHash].css"})
    ],
    module: {
        rules: [
            {
                test: /\.scss$/,
                use: [
                    MiniCssExtractPlugin.loader, 
                    "css-loader",
                    'resolve-url-loader',  
                    "sass-loader"  
                ]
            },
            {
                test: /\.(png|jpg|jpeg|svg|woff|woff2|ttf|eot|ico)$/,
                loader: 'url-loader',
                options: {
                  name: '[name].[ext]?[hash]',
                  limit: 10000,
                },
              }
        ]
    }
});
