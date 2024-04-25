namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Services.Lock;
public interface IThreadLockService
{
    public Task WaitAsync(LockName lockName);

    public void Wait(LockName lockName);

    public void Release(LockName lockName);
}
