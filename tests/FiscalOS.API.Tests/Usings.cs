global using System.IdentityModel.Tokens.Jwt;
global using System.Net;
global using System.Net.Http.Headers;
global using System.Net.Http.Json;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.Json;

global using AwesomeAssertions.Execution;
global using AwesomeAssertions.Primitives;

global using FiscalOS.API.Tests.Assertions;
global using FiscalOS.API.Tests.Infra;
global using FiscalOS.Core.Authentication;
global using FiscalOS.Core.Identity;
global using FiscalOS.Core.Security;
global using FiscalOS.Infra.Authentication;
global using FiscalOS.Infra.Data;

global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.AspNetCore.TestHost;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;

global using Xunit.Sdk;