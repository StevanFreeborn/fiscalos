global using System.Globalization;
global using System.IdentityModel.Tokens.Jwt;
global using System.IO.Abstractions;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.Json;

global using AwesomeAssertions.Primitives;

global using FiscalOS.Core.Identity;
global using FiscalOS.Core.Security;
global using FiscalOS.Infra.Authentication;
global using FiscalOS.Infra.Authorization;
global using FiscalOS.Infra.Security;
global using FiscalOS.Infra.Tests.Assertions;
global using FiscalOS.Infra.Tests.Mocks;

global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Authorization.Policy;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;