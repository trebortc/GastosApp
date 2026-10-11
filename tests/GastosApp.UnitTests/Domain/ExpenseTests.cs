using GastosApp.Domain.Entities;
using GastosApp.Domain.Enums;
using GastosApp.Domain.Exceptions;

namespace GastosApp.UnitTests.Domain;

public class ExpenseTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid CategoryId = Guid.NewGuid();

    private static Expense CreatePersonal(decimal amount = 100m, string currency = "MXN")
        => Expense.Create(Guid.NewGuid(), UserId, null, CategoryId, UserId,
            ExpenseType.Expense, amount, currency, "Test", DateTimeOffset.UtcNow);

    [Fact]
    public void Expense_Personal_ShouldSetIsPersonalTrue()
    {
        var expense = CreatePersonal();
        Assert.True(expense.IsPersonal);
        Assert.False(expense.IsGroupExpense);
    }

    [Fact]
    public void Expense_WithBothOwners_ShouldThrowDomainException()
    {
        var groupId = Guid.NewGuid();
        Assert.Throws<DomainException>(() =>
            Expense.Create(Guid.NewGuid(), UserId, groupId, CategoryId, UserId,
                ExpenseType.Expense, 100m, "MXN", "Test", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Expense_WithNoOwner_ShouldThrowDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Expense.Create(Guid.NewGuid(), null, null, CategoryId, UserId,
                ExpenseType.Expense, 100m, "MXN", "Test", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Expense_WithZeroAmount_ShouldThrowDomainException()
    {
        Assert.Throws<DomainException>(() => CreatePersonal(0m));
    }

    [Fact]
    public void Expense_WithNegativeAmount_ShouldThrowDomainException()
    {
        Assert.Throws<DomainException>(() => CreatePersonal(-50m));
    }

    [Fact]
    public void Expense_Delete_ByCreator_ShouldMarkAsDeleted()
    {
        var expense = CreatePersonal();
        expense.Delete(UserId);
        Assert.True(expense.IsDeleted);
    }

    [Fact]
    public void Expense_Delete_ByOtherUser_ShouldThrowForbiddenException()
    {
        var expense = CreatePersonal();
        Assert.Throws<ForbiddenException>(() => expense.Delete(Guid.NewGuid()));
    }

    [Fact]
    public void Expense_DeleteTwice_ShouldThrowDomainException()
    {
        var expense = CreatePersonal();
        expense.Delete(UserId);
        Assert.Throws<DomainException>(() => expense.Delete(UserId));
    }

    [Fact]
    public void Expense_Update_ByCreator_ShouldChangeFields()
    {
        var expense = CreatePersonal();
        var newCategory = Guid.NewGuid();
        expense.Update(newCategory, ExpenseType.Income, 200m, "USD", "Updated", DateTimeOffset.UtcNow, UserId);
        Assert.Equal(newCategory, expense.CategoryId);
        Assert.Equal(200m, expense.Money.Amount);
        Assert.Equal("USD", expense.Money.Currency);
    }

    [Fact]
    public void Expense_Money_ShouldRoundToTwoDecimals()
    {
        var expense = CreatePersonal(100.999m);
        Assert.Equal(101.00m, expense.Money.Amount);
    }
}
