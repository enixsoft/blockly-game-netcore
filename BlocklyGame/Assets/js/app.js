import Vue from 'vue';
import App from './components/App';
import '../sass/app.scss';

export const bus = new Vue();

new Vue({
	el: '#app',
	components: {  
		App
	}   
});
