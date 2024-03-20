using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphQL.Types;
using VirtoCommerce.ExperienceApiModule.Core.Schemas;

namespace ObjectEdge.MyLoyalty.ExperienceApi.Schemas
{
    class helloSchema : ObjectGraphType
    {
        public helloSchema() {

            Name = "hello";

            Field<StringGraphType>("hello", resolve: context => "helloooo!");
        }
    }
}
