using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphQL.Types;
using GraphQL;
using ObjectEdge.MyLoyalty.ExperienceApi.Aggregates;
using VirtoCommerce.ExperienceApiModule.Core.BaseQueries;


namespace ObjectEdge.MyLoyalty.ExperienceApi.Queries
{
    public class CPQConfigureQuery : Query<CPQConfigureAggregate>
    {
        public string? Model { get; set; }
        public string? UserId { get; set; }

        public string? ProductLine { get; set; }
        public string? Segment { get; set; }

        //model: automaticPanel
        //product_line: electricalEquipment
        //segment: instaTec

        public override IEnumerable<QueryArgument> GetArguments()
        {
            yield return Argument<StringGraphType>(nameof(Model));
            yield return Argument<StringGraphType>(nameof(UserId));
            yield return Argument<StringGraphType>(nameof(ProductLine));
            yield return Argument<StringGraphType>(nameof(Segment));
        }

        public override void Map(IResolveFieldContext context)
        {
            Model = context.GetArgument<string>(nameof(Model));
            UserId = context.GetArgument<string>(nameof(UserId));
            ProductLine = context.GetArgument<string>(nameof(ProductLine));
            Segment = context.GetArgument<string>(nameof(Segment));

        }
    }
}
