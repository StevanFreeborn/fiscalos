import { Err, Ok, Result } from 'ts-results';
import { type InjectionKey } from 'vue';
import { ClientRequestWithBody, type IClient } from './client';

type AuthServiceFactoryKeyType = InjectionKey<IAuthServiceFactory>;

export const AuthServiceFactoryKey: AuthServiceFactoryKeyType = Symbol('AuthServiceFactory');

export interface IAuthServiceFactory {
  create: (client: IClient) => IAuthService;
}

export class AuthServiceFactory implements IAuthServiceFactory {
  create(client: IClient): IAuthService {
    return new AuthService(client);
  }
}

export interface IAuthService {
  login: (username: string, password: string) => Promise<Result<LoginResponse, Error[]>>;
  refreshToken: () => Promise<Result<LoginResponse, Error[]>>;
}

export class AuthService implements IAuthService {
  private readonly client: IClient;
  private readonly endpoints = {
    login: '/api/auth/login',
    refreshToken: '/api/auth/refresh',
  };

  constructor(client: IClient) {
    this.client = client;
  }

  async login(username: string, password: string) {
    const request = new ClientRequestWithBody(
      this.endpoints.login,
      undefined,
      new LoginRequest(username, password)
    );

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
        return Err([new Error('Email/Password combination is not valid')]);
      }

      if (res.ok === false) {
        return Err([new Error('Login failed. Please try again.')]);
      }

      const body = await res.json();
      return Ok(body as LoginResponse);
    } catch (e) {
      console.error(e);
      return Err([new Error('Login failed. Please try again.')]);
    }
  }

  async refreshToken() {
    const request = new ClientRequestWithBody(this.endpoints.refreshToken, undefined, undefined);

    try {
      const res = await this.client.post(request);

      if (res.status === 401) {
        return Err([new Error('Refresh and/or access token is not valid')]);
      }

      if (res.ok === false) {
        return Err([new Error('Refreshing token failed')]);
      }

      const body = await res.json();
      return Ok(body as LoginResponse);
    } catch (e) {
      console.error(e);
      return Err([new Error('Refreshing token failed.')]);
    }
  }
}

class BaseAuthRequest {
  readonly username: string;
  readonly password: string;

  constructor(username: string, password: string) {
    this.username = username;
    this.password = password;
  }
}

class LoginRequest extends BaseAuthRequest {
  constructor(username: string, password: string) {
    super(username, password);
  }
}

type LoginResponse = {
  accessToken: string;
};
