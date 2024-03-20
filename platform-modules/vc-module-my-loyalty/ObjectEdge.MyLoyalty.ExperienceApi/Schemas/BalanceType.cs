using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ObjectEdge.MyLoyalty.ExperienceApi.Aggregates;
using VirtoCommerce.ExperienceApiModule.Core.Schemas;
using VirtoCommerce.ExperienceApiModule.Core.Services;

namespace ObjectEdge.MyLoyalty.ExperienceApi.Schemas
{
    public class BalanceType : ExtendableGraphType<BalanceAggregate>
    {
        public BalanceType(IDynamicPropertyResolverService dynamicPropertyResolverService) {
               Field(x => x.Balance, nullable : false);
        }
    }
}
