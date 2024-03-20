using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using VirtoCommerce.ExperienceApiModule.Core.BaseQueries;

namespace ObjectEdge.MyLoyalty.ExperienceApi.Queries
{
    public class helloQuery : Query<ObjectGraphType>
    {
        public override IEnumerable<QueryArgument> GetArguments()
        {
            yield return Argument<StringGraphType>("hellow");
        }

        public override void Map(IResolveFieldContext context)
        {
            
        }
    }
}
