using BuildingBlocks.Common.Exceptions.Handler;
using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Services;
using Carter;
using Catalog;

var builder = WebApplication.CreateBuilder(args);

var catalogAssembly = typeof(CatalogModule).Assembly;

builder.Services.AddCarterWithAssemblies(catalogAssembly);

builder.Services.AddMediatRWithAssemblies(catalogAssembly);

builder.Services.AddCatalogModule(builder.Configuration);

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.MapCarter();

app.UseExceptionHandler(options => { });

app.UseCatalogModule();

app.Run();