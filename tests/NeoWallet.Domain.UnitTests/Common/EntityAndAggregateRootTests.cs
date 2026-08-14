using FluentAssertions;
using NeoWallet.Domain.Common;

namespace NeoWallet.Domain.UnitTests.Common;

public sealed class EntityAndAggregateRootTests
{
    private sealed class TestEntity : Entity<Guid>
    {
        public TestEntity(Guid id) : base(id) { }
    }

    private sealed class AnotherEntity : Entity<Guid>
    {
        public AnotherEntity(Guid id) : base(id) { }
    }

    [Fact]
    public void Entity_Equality_SameIdSameType_ShouldBeEqual()
    {
        var id = Guid.NewGuid();
        var e1 = new TestEntity(id);
        var e2 = new TestEntity(id);

        e1.Should().Be(e2);
        (e1 == e2).Should().BeTrue();
        (e1 != e2).Should().BeFalse();
        e1.GetHashCode().Should().Be(e2.GetHashCode());
    }

    [Fact]
    public void Entity_Equality_DifferentId_ShouldNotBeEqual()
    {
        var e1 = new TestEntity(Guid.NewGuid());
        var e2 = new TestEntity(Guid.NewGuid());

        e1.Should().NotBe(e2);
        (e1 == e2).Should().BeFalse();
        (e1 != e2).Should().BeTrue();
    }

    [Fact]
    public void Entity_Equality_DifferentTypeSameId_ShouldNotBeEqual()
    {
        var id = Guid.NewGuid();
        var e1 = new TestEntity(id);
        var e2 = new AnotherEntity(id);

        e1.Equals(e2).Should().BeFalse();
    }

    [Fact]
    public void Entity_Equality_NullComparisons_ShouldHandleProperly()
    {
        var e1 = new TestEntity(Guid.NewGuid());
        TestEntity? e2 = null;

        e1.Equals(null).Should().BeFalse();
        (e1 == e2).Should().BeFalse();
        (e2 == null).Should().BeTrue();
        (null == e2).Should().BeTrue();
    }
}
