using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using UsuariosApp.Application.Helpers;
using UsuariosApp.Application.InterfaceSecurities;
using UsuariosApp.Application.InterfaceService;
using UsuariosApp.Application.Services;
using UsuariosApp.Application.Validations;
using UsuariosApp.Domain.InterfaceRepository;
using UsuariosApp.Domain.InterfaceService;
using UsuariosApp.InfraStructure.Context;
using UsuariosApp.InfraStructure.Repositories;
using UsuariosApp.InfraStructure.UnidadesTrabalhos;

namespace UsuariosApp.Settings.InjecoesDependencias
{
   public static class InjecaoDependencia
    {
        public static IServiceCollection RegistrarServicos(this IServiceCollection services)
        {
            // Application
            services.AddScoped<IUsuarioService, UsuarioService>();

            // Infraestrutura
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IUnidadeTrabalho, UnidadeTrabalho>();
            services.AddDbContext<UsuarioDbContext>();

            // Outros serviços (como senha hash, validação, etc)
            services.AddScoped<ISenhaHasher, BCryptSenhaHasher>();
            services.AddValidatorsFromAssemblyContaining<UsuarioValidator>();


            return services;
        }
    }
}
