using GastosApp.Domain.Entities;
using GastosApp.Domain.Enums;
using GastosApp.Domain.Exceptions;

namespace GastosApp.UnitTests.Domain;

public class CategoryTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void Category_CreatePersonal_ShouldBePersonal()
    {
        var cat = Category.CreatePersonal(Guid.NewGuid(), UserId, "Alimentacion");
        Assert.False(cat.IsSystem);
        Assert.Equal(UserId, cat.OwnerUserId);
    }

    [Fact]
    public void Category_CreateSystem_ShouldBeSystem()
    {
        var cat = Category.CreateSystem(Guid.NewGuid(), "Transporte");
        Assert.True(cat.IsSystem);
        Assert.Null(cat.OwnerUserId);
    }

    [Fact]
    public void Category_RenameSystem_ShouldThrowForbiddenException()
    {
        var cat = Category.CreateSystem(Guid.NewGuid(), "Transporte");
        Assert.Throws<ForbiddenException>(() => cat.Rename("Nuevo nombre"));
    }

    [Fact]
    public void Category_DeleteSystem_ShouldThrowForbiddenException()
    {
        var cat = Category.CreateSystem(Guid.NewGuid(), "Salud");
        Assert.Throws<ForbiddenException>(() => cat.Delete());
    }

    [Fact]
    public void Category_RenamePersonal_ShouldUpdateName()
    {
        var cat = Category.CreatePersonal(Guid.NewGuid(), UserId, "Ocio");
        cat.Rename("Entretenimiento");
        Assert.Equal("Entretenimiento", cat.Name);
    }

    [Fact]
    public void Category_DeletePersonal_ShouldMarkAsDeleted()
    {
        var cat = Category.CreatePersonal(Guid.NewGuid(), UserId, "Ocio");
        cat.Delete();
        Assert.True(cat.IsDeleted);
    }
}
