using GastosApp.Domain.Common;
using GastosApp.Domain.Exceptions;

namespace GastosApp.UnitTests.Domain;

public class EntityTests
{
    private sealed class TestEntity : Entity
    {
        public TestEntity(Guid id) : base(id) { }
    }

    [Fact]
    public void Entity_WithValidId_ShouldSetId()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);
        Assert.Equal(id, entity.Id);
    }

    [Fact]
    public void Entity_WithEmptyGuid_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new TestEntity(Guid.Empty));
    }

    [Fact]
    public void TwoEntities_WithSameId_ShouldBeEqual()
    {
        var id = Guid.NewGuid();
        var a = new TestEntity(id);
        var b = new TestEntity(id);
        Assert.Equal(a, b);
    }

    [Fact]
    public void TwoEntities_WithDifferentIds_ShouldNotBeEqual()
    {
        var a = new TestEntity(Guid.NewGuid());
        var b = new TestEntity(Guid.NewGuid());
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void DomainException_ShouldCarryMessage()
    {
        var ex = new DomainException("Error de prueba");
        Assert.Equal("Error de prueba", ex.Message);
    }

    [Fact]
    public void NotFoundException_ShouldFormatMessage()
    {
        var id = Guid.NewGuid();
        var ex = new NotFoundException("Expense", id);
        Assert.Contains("Expense", ex.Message);
        Assert.Contains(id.ToString(), ex.Message);
    }

    [Fact]
    public void ForbiddenException_ShouldHaveDefaultMessage()
    {
        var ex = new ForbiddenException();
        Assert.False(string.IsNullOrWhiteSpace(ex.Message));
    }
}
