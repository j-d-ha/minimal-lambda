using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit3;

namespace MinimalLambda.UnitTests;

public class AutoNSubstituteDataAttribute() : AutoDataAttribute(() =>
    new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true }));

public class InlineAutoNSubstituteDataAttribute : InlineAutoDataAttribute
{
    public InlineAutoNSubstituteDataAttribute(params object[] args) : base(
        new AutoNSubstituteDataAttribute(),
        args) { }
}
