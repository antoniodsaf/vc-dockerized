using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ObjectEdge.MyLoyalty.ExperienceApi.Aggregates;
using VirtoCommerce.ExperienceApiModule.Core.Infrastructure;

namespace ObjectEdge.MyLoyalty.ExperienceApi.Queries
{


    public class BalanceQueryHandler : IQueryHandler<BalanceQuery, BalanceAggregate>
    {

        public  Task<BalanceAggregate> Handle(BalanceQuery request, CancellationToken cancellationToken)
        {
            var result = new BalanceAggregate();
            result.Balance =  777;
            return Task.FromResult(result);
        }
    }
}
