import type { InjectionKey } from 'vue';

type ClientKeyType = InjectionKey<IClientFactory>;

export const ClientFactoryKey: ClientKeyType = Symbol('AuthServiceFactory');

export interface IClientFactory {
  create: (config?: ClientConfig) => IClient;
}

export class ClientFactory implements IClientFactory {
  create(config?: ClientConfig): IClient {
    return new Client(config);
  }
}

export class ClientRequest {
  readonly url: string;
  readonly config?: RequestInit;

  constructor(url: string, config?: RequestInit) {
    this.url = url;
    this.config = config;
  }
}

export class ClientRequestWithBody<T> extends ClientRequest {
  body: T | undefined;

  constructor(url: string, config?: RequestInit, body?: T) {
    super(url, config);
    this.body = body;
  }
}

export type UnauthorizedResponseHandler =
  | ((originalRequest: Request) => Promise<{ response: Response; accessToken: string }>)
  | undefined;

export class ClientConfig {
  authHeader: Record<string, string> | undefined;
  includeCredentials: boolean | undefined;
  unauthorizedResponseHandler: UnauthorizedResponseHandler;

  constructor(
    authHeader?: Record<string, string> | undefined,
    includeCredentials?: boolean | undefined,
    unauthorizedResponseHandler?: UnauthorizedResponseHandler
  ) {
    this.authHeader = authHeader;
    this.includeCredentials = includeCredentials;
    this.unauthorizedResponseHandler = unauthorizedResponseHandler;
  }
}

export interface IClient {
  get: (req: ClientRequest) => Promise<Response>;
  post: <T>(req: ClientRequestWithBody<T>) => Promise<Response>;
  put: <T>(req: ClientRequestWithBody<T>) => Promise<Response>;
  patch: <T>(req: ClientRequestWithBody<T>) => Promise<Response>;
  delete: (req: ClientRequest) => Promise<Response>;
}

export class Client implements IClient {
  private readonly _clientConfig: ClientConfig;

  constructor(clientConfig?: ClientConfig) {
    this._clientConfig = clientConfig ?? new ClientConfig(undefined, true, undefined);
  }

  private async request(url: string, config?: RequestInit): Promise<Response> {
    const headers = {
      ...config?.headers,
      ...this._clientConfig?.authHeader,
    };

    const credentials = this._clientConfig?.includeCredentials
      ? ('include' as RequestCredentials)
      : ('omit' as RequestCredentials);

    const requestConfig = {
      ...config,
      headers: headers,
      credentials: credentials,
    };

    const firstTryRequest = new Request(url, requestConfig);
    const secondTryRequest = new Request(url, requestConfig);
    let response = await fetch(firstTryRequest);

    if (response.status === 401 && this._clientConfig?.unauthorizedResponseHandler) {
      const retryResult = await this._clientConfig.unauthorizedResponseHandler(secondTryRequest);
      this._clientConfig.authHeader = { Authorization: `Bearer ${retryResult.accessToken}` };
      response = retryResult.response;
    }

    return response;
  }

  async get({ url, config }: { url: string; config?: RequestInit }) {
    const requestConfig = { ...config, method: 'GET' };
    return await this.request(url, requestConfig);
  }

  async post<T>(req: ClientRequestWithBody<T>) {
    const requestConfig = {
      ...req?.config,
      method: 'POST',
      body: JSON.stringify(req?.body),
      headers: {
        ...req?.config?.headers,
        'Content-Type': 'application/json',
      },
    };

    return await this.request(req?.url, requestConfig);
  }

  async put<T>(req: ClientRequestWithBody<T>) {
    const requestConfig = {
      ...req?.config,
      method: 'PUT',
      body: JSON.stringify(req?.body),
      headers: {
        ...req?.config?.headers,
        'Content-Type': 'application/json',
      },
    };

    return await this.request(req?.url, requestConfig);
  }

  async patch<T>(req: ClientRequestWithBody<T>) {
    const requestConfig = {
      ...req?.config,
      method: 'PATCH',
      body: JSON.stringify(req?.body),
      headers: {
        ...req?.config?.headers,
        'Content-Type': 'application/json',
      },
    };

    return await this.request(req?.url, requestConfig);
  }

  async delete(req: ClientRequest) {
    const requestConfig = { ...req?.config, method: 'DELETE' };
    return await this.request(req?.url, requestConfig);
  }
}
