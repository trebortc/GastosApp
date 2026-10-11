using GastosApp.Domain.Entities;
using GastosApp.Domain.Enums;
using GastosApp.Domain.Exceptions;

namespace GastosApp.UnitTests.Domain;

public class ExpenseGroupTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    private static ExpenseGroup CreateGroup()
        => ExpenseGroup.Create(Guid.NewGuid(), OwnerId, "Grupo Test", null, "MXN");

    [Fact]
    public void ExpenseGroup_Create_ShouldAddOwnerAsMember()
    {
        var group = CreateGroup();
        Assert.Single(group.Members);
        Assert.Equal(GroupMemberRole.Owner, group.Members[0].Role);
        Assert.Equal(OwnerId, group.Members[0].UserId);
    }

    [Fact]
    public void ExpenseGroup_AddMember_ByOwner_ShouldSucceed()
    {
        var group = CreateGroup();
        var newUser = Guid.NewGuid();
        group.AddMember(newUser, OwnerId);
        Assert.Equal(2, group.Members.Count);
    }

    [Fact]
    public void ExpenseGroup_AddMember_ByNonAdmin_ShouldThrowForbiddenException()
    {
        var group = CreateGroup();
        var nonAdmin = Guid.NewGuid();
        Assert.Throws<ForbiddenException>(() => group.AddMember(Guid.NewGuid(), nonAdmin));
    }

    [Fact]
    public void ExpenseGroup_AddDuplicateMember_ShouldThrowDomainException()
    {
        var group = CreateGroup();
        Assert.Throws<DomainException>(() => group.AddMember(OwnerId, OwnerId));
    }

    [Fact]
    public void ExpenseGroup_RemoveOwner_ShouldThrowDomainException()
    {
        var group = CreateGroup();
        Assert.Throws<DomainException>(() => group.RemoveMember(OwnerId, OwnerId));
    }

    [Fact]
    public void ExpenseGroup_Deactivate_ByOwner_ShouldSetIsActiveFalse()
    {
        var group = CreateGroup();
        group.Deactivate(OwnerId);
        Assert.False(group.IsActive);
    }

    [Fact]
    public void ExpenseGroup_Create_WithEmptyName_ShouldThrowDomainException()
    {
        Assert.Throws<DomainException>(() =>
            ExpenseGroup.Create(Guid.NewGuid(), OwnerId, "", null, "MXN"));
    }
}
