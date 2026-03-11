import type { InjectionKey } from 'vue';
import { ClientRequestWithBody, type IClient } from './client';
import { Err, Ok, type Result } from 'ts-results';

type AccountServiceFactoryKeyType = InjectionKey<IAccountServiceFactory>;

export const AccountServiceFactoryKey: AccountServiceFactoryKeyType =
  Symbol('AccountServiceFactory');

export interface IAccountServiceFactory {
  create: (client: IClient) => IAccountService;
}

export class AccountServiceFactory implements IAccountServiceFactory {
  create(client: IClient): IAccountService {
    return new AccountService(client);
  }
}

export interface IAccountService {
  add: (
    providerInstitutionId: string,
    providerAccountId: string,
    providerAccountName: string
  ) => Promise<Result<boolean, Error[]>>;
}

export class AccountService implements IAccountService {
  private readonly client: IClient;
  private readonly endpoints = {
    add: '/api/accounts',
  };

  constructor(client: IClient) {
    this.client = client;
  }

  async add(providerInstitutionId: string, providerAccountId: string, providerAccountName: string) {
    const request = new ClientRequestWithBody(this.endpoints.add, undefined, {
      providerInstitutionId,
      providerAccountId,
      providerAccountName,
    });

    try {
      const res = await this.client.post(request);

      if (res.ok === false) {
        return Err([new Error('Failed to add account.')]);
      }

      return Ok(true);
    } catch (error) {
      console.error(error);
      return Err([new Error('Failed to add account.')]);
    }
  }
}
