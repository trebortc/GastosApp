using GastosApp.Domain.Entities;
using GastosApp.Domain.Enums;
using GastosApp.Domain.Exceptions;

namespace GastosApp.UnitTests.Domain;

public class SyncOperationTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static SyncOperation CreatePending()
        => SyncOperation.Create(Guid.NewGuid(), UserId,
            SyncOperationType.CreateExpense, "{\"amount\":100}");

    [Fact]
    public void SyncOperation_Create_ShouldBePending()
    {
        var op = CreatePending();
        Assert.Equal(SyncOperationStatus.Pending, op.Status);
        Assert.Equal(0, op.AttemptCount);
    }

    [Fact]
    public void SyncOperation_MarkSyncing_ShouldIncrementAttemptCount()
    {
        var op = CreatePending();
        op.MarkSyncing();
        Assert.Equal(SyncOperationStatus.Syncing, op.Status);
        Assert.Equal(1, op.AttemptCount);
    }

    [Fact]
    public void SyncOperation_MarkSynced_ShouldClearError()
    {
        var op = CreatePending();
        op.MarkSyncing();
        op.MarkSynced();
        Assert.Equal(SyncOperationStatus.Synced, op.Status);
        Assert.Null(op.ErrorMessage);
    }

    [Fact]
    public void SyncOperation_MarkSynced_ThenMarkSyncing_ShouldThrow()
    {
        var op = CreatePending();
        op.MarkSyncing();
        op.MarkSynced();
        Assert.Throws<DomainException>(() => op.MarkSyncing());
    }

    [Fact]
    public void SyncOperation_ResetForRetry_ShouldReturnToPending()
    {
        var op = CreatePending();
        op.MarkSyncing();
        op.MarkFailed("timeout");
        op.ResetForRetry();
        Assert.Equal(SyncOperationStatus.Pending, op.Status);
        Assert.Null(op.ErrorMessage);
    }

    [Fact]
    public void SyncOperation_ResetSynced_ShouldThrow()
    {
        var op = CreatePending();
        op.MarkSyncing();
        op.MarkSynced();
        Assert.Throws<DomainException>(() => op.ResetForRetry());
    }
}
