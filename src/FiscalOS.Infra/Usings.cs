global using System.IdentityModel.Tokens.Jwt;
global using System.IO.Abstractions;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.Json;

global using FiscalOS.Core.Accounts;
global using FiscalOS.Core.Authentication;
global using FiscalOS.Core.Data;
global using FiscalOS.Core.Identity;
global using FiscalOS.Core.Security;
global using FiscalOS.Infra.Accounts.Plaid;
global using FiscalOS.Infra.Authentication;
global using FiscalOS.Infra.Authorization;
global using FiscalOS.Infra.Data;
global using FiscalOS.Infra.Security;

global using Going.Plaid;
global using Going.Plaid.Entity;

global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Authorization.Policy;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Diagnostics;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;