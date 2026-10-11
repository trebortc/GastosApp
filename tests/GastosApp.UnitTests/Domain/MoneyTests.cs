using GastosApp.Domain.Exceptions;
using GastosApp.Domain.ValueObjects;

namespace GastosApp.UnitTests.Domain;

public class MoneyTests
{
    [Fact]
    public void Money_WithValidValues_ShouldCreate()
    {
        var money = new Money(100m, "MXN");
        Assert.Equal(100m, money.Amount);
        Assert.Equal("MXN", money.Currency);
    }

    [Fact]
    public void Money_ShouldNormalizeCurrencyToUpperCase()
    {
        var money = new Money(50m, "mxn");
        Assert.Equal("MXN", money.Currency);
    }

    [Fact]
    public void Money_WithNegativeAmount_ShouldThrowDomainException()
    {
        Assert.Throws<DomainException>(() => new Money(-1m, "MXN"));
    }

    [Fact]
    public void Money_WithInvalidCurrency_ShouldThrowDomainException()
    {
        Assert.Throws<DomainException>(() => new Money(100m, "MX"));
        Assert.Throws<DomainException>(() => new Money(100m, ""));
    }

    [Fact]
    public void Money_Add_SameCurrency_ShouldReturnSum()
    {
        var a = new Money(100m, "MXN");
        var b = new Money(50m, "MXN");
        Assert.Equal(new Money(150m, "MXN"), a.Add(b));
    }

    [Fact]
    public void Money_Add_DifferentCurrency_ShouldThrowDomainException()
    {
        var a = new Money(100m, "MXN");
        var b = new Money(50m, "USD");
        Assert.Throws<DomainException>(() => a.Add(b));
    }

    [Fact]
    public void Money_Equality_SameValues_ShouldBeEqual()
    {
        var a = new Money(100m, "MXN");
        var b = new Money(100m, "MXN");
        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact]
    public void Money_Equality_DifferentValues_ShouldNotBeEqual()
    {
        var a = new Money(100m, "MXN");
        var b = new Money(200m, "MXN");
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Money_ShouldRoundToTwoDecimals()
    {
        var money = new Money(10.999m, "MXN");
        Assert.Equal(11.00m, money.Amount);
    }
}
