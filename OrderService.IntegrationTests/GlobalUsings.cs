global using FluentAssertions;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.Extensions.DependencyInjection;
global using OrderService.Application.DTOs;
global using OrderService.Domain.Entities;
global using OrderService.Infrastructure.Data;
global using System.Net;
global using System.Net.Http.Json;
global using Grpc.Core;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.Data.SqlClient;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Moq;
global using OrderService.Application.Common.Cache;
global using Respawn;
global using Shared.GrpcContracts.Payment;
global using Shared.GrpcContracts.Product;
global using System.Data.Common;
global using Testcontainers.MsSql;
global using OrderService.Infrastructure.Common.CacheProvider;
global using StackExchange.Redis;
global using Testcontainers.Redis;
global using static Shared.GrpcContracts.Payment.PaymentGrpcService;
global using static Shared.GrpcContracts.Product.ProductGrpcService;

[assembly: CollectionBehavior(DisableTestParallelization = true)]