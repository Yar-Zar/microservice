global using Dapper;
global using Microsoft.Data.SqlClient;
global using Shared.Contracts.Models;
global using Microsoft.Extensions.Configuration;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using OrderService.Infrastructure.Data;
global using OrderService.Infrastructure.Repositories;
global using OrderService.Infrastructure.Common;
global using OrderService.Domain.Entities;
global using Microsoft.EntityFrameworkCore.Storage;
global using System.Linq.Expressions;
global using StackExchange.Redis;
global using IDatabase = StackExchange.Redis.IDatabase;
global using OrderService.Application.Common.Cache;
global using OrderService.Application.Interfaces.IRepository;
global using OrderService.Infrastructure.Common.CacheProvider;
global using System.Text.Json;
global using System.Data;


