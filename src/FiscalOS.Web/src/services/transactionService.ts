import type { InjectionKey } from 'vue';
import { ClientRequest, type IClient } from './client';
import { Err, Ok, type Result } from 'ts-results';

type TransactionServiceFactoryKeyType = InjectionKey<ITransactionServiceFactory>;

export const TransactionServiceFactoryKey: TransactionServiceFactoryKeyType =
  Symbol('AuthServiceFactory');

export interface ITransactionServiceFactory {
  create: (client: IClient) => ITransactionService;
}

export class TransactionServiceFactory implements ITransactionServiceFactory {
  create(client: IClient): ITransactionService {
    return new TransactionService(client);
  }
}

export interface ITransactionService {
  get: (pageNumber?: number, pageSize?: number) => Promise<Result<Page<Transaction>, Error[]>>;
}

export class TransactionService implements ITransactionService {
  private readonly client: IClient;
  private readonly endpoints = {
    get: '/api/transactions',
  };

  constructor(client: IClient) {
    this.client = client;
  }

  async get(pageNumber: number = 1, pageSize: number = 500) {
    const queryParams = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
    });
    const url = this.endpoints.get + '?' + queryParams.toString();
    const request = new ClientRequest(url);

    try {
      const response = await this.client.get(request);

      if (response.ok === false) {
        return Err([new Error('Failed to retrieve transactions.')]);
      }

      const data = await response.json();
      return Ok(data as Page<Transaction>);
    } catch (error) {
      console.error(error);
      return Err([new Error('Failed to retrieve transactions.')]);
    }
  }
}

export type Page<T> = {
  pageNumber: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  items: T[];
};

export type Transaction = {
  id: string;
  merchantName: string;
  description: string;
  amount: number;
  date: string;
};
