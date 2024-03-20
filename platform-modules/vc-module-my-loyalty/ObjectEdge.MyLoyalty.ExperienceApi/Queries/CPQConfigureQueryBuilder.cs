using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphQL;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using ObjectEdge.MyLoyalty.ExperienceApi.Aggregates;
using ObjectEdge.MyLoyalty.ExperienceApi.Schemas;
using VirtoCommerce.ExperienceApiModule.Core.BaseQueries;
using VirtoCommerce.ExperienceApiModule.Core.Extensions;

namespace ObjectEdge.MyLoyalty.ExperienceApi.Queries
{
    public class CPQConfigureQueryBuilder : QueryBuilder<CPQConfigureQuery, CPQConfigureAggregate, CPQConfigureType>
    {
        protected override string Name => "cpqConfigure";

        public CPQConfigureQueryBuilder(IMediator mediator, IAuthorizationService authorizationService) : base(mediator, authorizationService) { }

        protected override CPQConfigureQuery GetRequest(IResolveFieldContext<object> context)
        {
            var request = base.GetRequest(context);

            if (string.IsNullOrEmpty(request.UserId))
            {
                request.UserId = context.GetCurrentUserId();
            }
            return request;
        }

    }

}
