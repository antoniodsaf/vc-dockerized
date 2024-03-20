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
    public class CPQConfigureType : ExtendableGraphType<CPQConfigureAggregate>
    {
        public CPQConfigureType(IDynamicPropertyResolverService dynamicPropertyResolverService)
        {
            Field(x => x.LoginSuccess, nullable: false);
            Field(x => x.Url, nullable: true);
        }
    }
}
