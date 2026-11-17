using Bindito.Core;
using Timberborn.TemplateInstantiation;

namespace Jove.ClimbLadder
{
    [Context("Game")]
    public class ClimbLadderConfigurator : Configurator
    {
        protected override void Configure()
        {
            Bind<RopeLadderLink>().AsTransient();
            MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
        }

        static TemplateModule ProvideTemplateModule()
        {
            TemplateModule.Builder builder = new TemplateModule.Builder();
            builder.AddDecorator<RopeLadderSpec, RopeLadderLink>();
            return builder.Build();
        }
    }
}
