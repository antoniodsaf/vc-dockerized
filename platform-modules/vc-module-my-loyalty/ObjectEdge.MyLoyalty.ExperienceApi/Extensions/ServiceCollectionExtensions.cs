using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using GraphQL.Server;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.ExperienceApiModule.Core.Infrastructure;
using VirtoCommerce.ExperienceApiModule.Core.Extensions;

namespace ObjectEdge.MyLoyalty.ExperienceApi.Extensions
{

    
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExperienceApi(this IServiceCollection serviceCollection)
        {
            var assemblyMarker = typeof(AssemblyMarker);
            var graphQlBuilder = new CustomGraphQLBuilder(serviceCollection);
            graphQlBuilder.AddGraphTypes(assemblyMarker);
            serviceCollection.AddMediatR(assemblyMarker);
            serviceCollection.AddAutoMapper(assemblyMarker);
            serviceCollection.AddSchemaBuilders(assemblyMarker);

            return serviceCollection;
        }
    }
}
