using Readiculous.Data;
using Readiculous.Data.Interfaces;
using Readiculous.Data.Repositories;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using Readiculous.Services.Services;
using Readiculous.WebApp.Authentication;
using Readiculous.WebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Supabase;

namespace Readiculous.WebApp
{
    // Other services configuration
    internal partial class StartupConfigurer
    {
        /// <summary>
        /// Configures the other services.
        /// </summary>
        private void ConfigureOtherServices()
        {
            // Framework
            this._services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            this._services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            // Common
            this._services.AddScoped<TokenProvider>();
            this._services.TryAddSingleton<TokenProviderOptionsFactory>();
            this._services.TryAddSingleton<TokenValidationParametersFactory>();
            this._services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services
            this._services.TryAddSingleton<TokenValidationParametersFactory>();
            this._services.AddScoped<IUserService, UserService>();
            this._services.AddScoped<IGenreService, GenreService>();
            this._services.AddScoped<IBookService, BookService>();

            // Repositories
            this._services.AddScoped<IUserRepository, UserRepository>();
            this._services.AddScoped<IGenreRepository, GenreRepository>();
            this._services.AddScoped<IBookRepository, BookRepository>();
            this._services.AddScoped<IFavoriteBookRepository, FavoriteBookRepository>();

            // Manager Class
            this._services.AddScoped<SignInManager>();

            // Supabase
            this._services.Configure<SupabaseOptions>(Configuration.GetSection("Supabase"));
            this._services.AddScoped<Client>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var options = sp.GetRequiredService<IOptions<SupabaseOptions>>().Value;

                var client = new Client(
                    config["Supabase:Url"],              
                    config["Supabase:ServiceRoleKey"],   
                    options                              
                );

                client.InitializeAsync().Wait();
                return client;
            });

            this._services.AddHttpClient();
        }
    }
}
