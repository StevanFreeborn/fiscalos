import './assets/css/main.css';

import { createApp } from 'vue';
import { createPinia } from 'pinia';

import App from './App.vue';
import router from './router';
import { ClientFactory, ClientFactoryKey } from './services/client';
import { AuthServiceFactory, AuthServiceFactoryKey } from './services/authService';
import {
  InstitutionServiceFactoryKey,
  InstitutionServiceFactory,
} from './services/institutionService';
import {
  TransactionServiceFactory,
  TransactionServiceFactoryKey,
} from './services/transactionService';
import { AccountServiceFactory, AccountServiceFactoryKey } from './services/accountService';

const app = createApp(App);

app.provide(ClientFactoryKey, new ClientFactory());
app.provide(AuthServiceFactoryKey, new AuthServiceFactory());
app.provide(InstitutionServiceFactoryKey, new InstitutionServiceFactory());
app.provide(TransactionServiceFactoryKey, new TransactionServiceFactory());
app.provide(AccountServiceFactoryKey, new AccountServiceFactory());

app.use(createPinia());
app.use(router);

app.mount('#app');
