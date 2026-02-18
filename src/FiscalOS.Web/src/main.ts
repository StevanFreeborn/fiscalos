import './assets/css/main.css';

import { createApp } from "vue";
import { createPinia } from "pinia";

import App from "./App.vue";
import router from "./router";
import { ClientFactory, ClientFactoryKey } from "./services/client";
import {  AuthServiceFactory, AuthServiceFactoryKey } from "./services/authService";

const app = createApp(App);

app.provide(ClientFactoryKey, new ClientFactory());
app.provide(AuthServiceFactoryKey, new AuthServiceFactory());

app.use(createPinia());
app.use(router);

app.mount("#app");
