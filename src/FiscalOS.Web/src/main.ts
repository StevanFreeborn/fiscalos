import './assets/css/main.css';

import { createApp } from "vue";
import { createPinia } from "pinia";

import App from "./App.vue";
import router from "./router";
import { ClientFactory, ClientFactoryKey } from "./services/client";
import {  AuthServiceFactory, AuthServiceFactoryKey } from "./services/authService";
import { InstituionServiceFactoryKey, InstitutionServiceFactory } from './services/institutionService';

const app = createApp(App);

app.provide(ClientFactoryKey, new ClientFactory());
app.provide(AuthServiceFactoryKey, new AuthServiceFactory());
app.provide(InstituionServiceFactoryKey, new InstitutionServiceFactory());

app.use(createPinia());
app.use(router);

app.mount("#app");
