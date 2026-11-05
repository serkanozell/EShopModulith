using Basket;
using BuildingBlocks.Common.Exceptions.Handler;
using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Services;
using BuildingBlocks.Messaging.Extensions;
using Carter;
using Catalog;

var builder = WebApplication.CreateBuilder(args);

var catalogAssembly = typeof(CatalogModule).Assembly;
var basketAssembly = typeof(BasketModule).Assembly;

builder.Services.AddCarterWithAssemblies(catalogAssembly, basketAssembly);

builder.Services.AddMediatRWithAssemblies(catalogAssembly, basketAssembly);

builder.Services.AddCatalogModule(builder.Configuration);

builder.Services.AddBasketModule(builder.Configuration);

builder.Services.AddRedisCaching(builder.Configuration);

builder.Services.AddMassTransitWithAssemblies(builder.Configuration, catalogAssembly, basketAssembly);

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.MapCarter();

app.UseExceptionHandler(options => { });

app.UseCatalogModule();

app.UseBasketModule();

app.Run();