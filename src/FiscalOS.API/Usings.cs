global using System.ComponentModel.DataAnnotations;
global using System.Security.Claims;
global using System.Text.Json.Serialization;

global using FiscalOS.API.Accounts;
global using FiscalOS.API.Auth;
global using FiscalOS.API.Common;
global using FiscalOS.API.Http;
global using FiscalOS.API.Institutions;
global using FiscalOS.API.Transactions;
global using FiscalOS.Core.Authentication;
global using FiscalOS.Core.Identity;
global using FiscalOS.Core.Queuing;
global using FiscalOS.Core.Security;
global using FiscalOS.Infra.Accounts.Plaid;
global using FiscalOS.Infra.Authentication;
global using FiscalOS.Infra.Data;
global using FiscalOS.Infra.DependencyInjection;
global using FiscalOS.Infra.Transactions.Plaid;
global using FiscalOS.ServiceDefaults;

global using Going.Plaid;
global using Going.Plaid.Converters;
global using Going.Plaid.Entity;
global using Going.Plaid.Webhook;

global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;