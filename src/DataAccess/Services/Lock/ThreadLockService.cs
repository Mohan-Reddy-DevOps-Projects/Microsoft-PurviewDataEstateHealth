namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Services.Lock;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;

public class ThreadLockService : IThreadLockService
{
    private IDataEstateHealthRequestLogger logger;
    private Dictionary<LockName, SemaphoreSlim> lockObjs;

    public ThreadLockService(IDataEstateHealthRequestLogger logger)
    {
        this.logger = logger;
        this.initializeLockObjs();
    }

    public void Release(LockName lockName)
    {
        var lockObj = this.getLockObj(lockName);
        this.logger.LogInformation($"Start release thread lock: {lockName}, availableCount: {lockObj.CurrentCount}");
        lockObj.Release();
        this.logger.LogInformation($"End release thread lock: {lockName}, availableCount: {lockObj.CurrentCount}");
    }

    public async Task WaitAsync(LockName lockName)
    {
        var lockObj = this.getLockObj(lockName);
        this.logger.LogInformation($"Start WaitAsync thread lock: {lockName}, availableCount: {lockObj.CurrentCount}");
        await lockObj.WaitAsync();
        this.logger.LogInformation($"End WaitAsync thread lock: {lockName}, availableCount: {lockObj.CurrentCount}");
    }

    public void Wait(LockName lockName)
    {
        var lockObj = this.getLockObj(lockName);
        this.logger.LogInformation($"Start Wait thread lock: {lockName}, availableCount: {lockObj.CurrentCount}");
        lockObj.Wait();
        this.logger.LogInformation($"End Wait thread lock: {lockName}, availableCount: {lockObj.CurrentCount}");
    }

    private void initializeLockObjs()
    {
        this.lockObjs = new Dictionary<LockName, SemaphoreSlim>()
        {
            { LockName.DEHServerlessQueryLock, new SemaphoreSlim(5, 5) }
        };
    }

    private SemaphoreSlim getLockObj(LockName lockName)
    {
        var lockObj = this.lockObjs.GetValueOrDefault(lockName, null);
        if (lockObj == null)
        {
            throw new Exception($"{lockName} is not configured.");
        }
        return lockObj;
    }
}
