namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Services.Lock;
public interface IThreadLockService
{
    public void WaitOne(LockName lockName);

    public void Release(LockName lockName);
}
