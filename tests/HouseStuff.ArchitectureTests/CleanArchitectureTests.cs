using HouseStuff.Application;
using HouseStuff.Domain;
using HouseStuff.Infrastructure;

namespace HouseStuff.ArchitectureTests;

public sealed class CleanArchitectureTests
{
    [Fact]
    public void DomainDoesNotReferenceOtherHouseStuffLayers()
    {
        var references = typeof(DomainAssembly).Assembly.GetReferencedAssemblies();

        Assert.DoesNotContain(references, reference => reference.Name?.StartsWith("HouseStuff.", StringComparison.Ordinal) == true);
    }

    [Fact]
    public void ApplicationDoesNotReferenceOuterLayers()
    {
        var names = typeof(ApplicationAssembly).Assembly.GetReferencedAssemblies().Select(reference => reference.Name).ToArray();

        Assert.DoesNotContain("HouseStuff.Infrastructure", names);
        Assert.DoesNotContain("HouseStuff.Api", names);
    }

    [Fact]
    public void InfrastructureDoesNotReferenceApi()
    {
        var names = typeof(InfrastructureAssembly).Assembly.GetReferencedAssemblies().Select(reference => reference.Name).ToArray();

        Assert.DoesNotContain("HouseStuff.Api", names);
    }
}
