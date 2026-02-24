import { Err, Ok, Result } from 'ts-results';
import { type InjectionKey } from 'vue';
import { ClientRequest, ClientRequestWithBody, type IClient } from './client';

type InstitutionServiceFactoryKeyType = InjectionKey<IInstitutionServiceFactory>;

export const InstituionServiceFactoryKey: InstitutionServiceFactoryKeyType =
  Symbol('AuthServiceFactory');

export interface IInstitutionServiceFactory {
  create: (client: IClient) => IInstitutionService;
}

export class InstitutionServiceFactory implements InstitutionServiceFactory {
  create(client: IClient): IInstitutionService {
    return new InstitutionService(client);
  }
}

export interface IInstitutionService {
  createLinkToken: () => Promise<Result<LinkTokenResponse, Error[]>>;
  connect: (publicToken: string, plaidInstitutionId: string) => Promise<Result<boolean, Error[]>>;
  getInstitutions: () => Promise<Result<Institution[], Error[]>>;
  getAvailableAccounts: (institutionId: string) => Promise<Result<AvailableAccount[], Error[]>>;
}

export class InstitutionService implements IInstitutionService {
  private readonly client: IClient;
  private readonly endpoints = {
    link: '/api/institutions/link',
    available(id: string) {
      return `/api/institutions/${id}/available`;
    },
    connect: '/api/institutions/connect',
    institutions: '/api/institutions',
  };

  constructor(client: IClient) {
    this.client = client;
  }

  async createLinkToken() {
    const request = new ClientRequestWithBody(this.endpoints.link);

    try {
      const res = await this.client.post(request);

      if (res.ok === false) {
        return Err([new Error('Failed to create link token')]);
      }

      const body = await res.json();
      return Ok(body as LinkTokenResponse);
    } catch (e) {
      console.error(e);
      return Err([new Error('Failed to create link token')]);
    }
  }

  async connect(publicToken: string, plaidInstitutionId: string) {
    const request = new ClientRequestWithBody(this.endpoints.connect, undefined, {
      publicToken,
      plaidInstitutionId,
    });

    try {
      const res = await this.client.post(request);

      if (res.status === 400) {
        const body = await res.json();
        const validationErrors = body.errors as Record<string, string[]>;
        const errors = Object.values(validationErrors)
          .flat()
          .map(e => new Error(e));

        return Err(errors);
      }

      if (res.status === 401) {
        return Err([new Error('Please sign in and try again.')]);
      }

      if (res.status === 409) {
        return Err([new Error('You have already linked this institution.')]);
      }

      return Ok(true);
    } catch (e) {
      console.error(e);
      return Err([new Error('Failed to connect institution')]);
    }
  }

  async getInstitutions() {
    const request = new ClientRequest(this.endpoints.institutions);

    try {
      const res = await this.client.get(request);

      if (res.ok === false) {
        return Err([new Error('Failed to get institutions')]);
      }

      const data = await res.json();

      return Ok(data.institutions as Institution[]);
    } catch (e) {
      console.error(e);
      return Err([new Error('Failed to get institutions')]);
    }
  }

  async getAvailableAccounts(institutionId: string) {
    const request = new ClientRequest(this.endpoints.available(institutionId));

    try {
      const res = await this.client.get(request);

      if (res.ok === false) {
        return Err([new Error('Failed to get available accounts')]);
      }

      const data = await res.json();

      return Ok(data.accounts as AvailableAccount[]);
    } catch (e) {
      console.error(e);
      return Err([new Error('Failed to get available accounts')]);
    }
  }
}

type LinkTokenResponse = {
  linkToken: string;
};

type Account = {
  id: string;
  name: string;
}

export type Institution = {
  id: string;
  name: string;
  accounts: Account[];
};

export type AvailableAccount = {
  providerInstitutionId: string;
  providerId: string;
  providerName: string;
  currentBalance: number;
  availableBalance: number;
  currencyCode: string;
};
