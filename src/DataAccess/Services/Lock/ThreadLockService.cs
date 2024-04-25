namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Services.Lock;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;

public class ThreadLockService : IThreadLockService
{
    private IDataEstateHealthRequestLogger logger;
    private Dictionary<LockName, Semaphore> lockObjs;

    public ThreadLockService(IDataEstateHealthRequestLogger logger)
    {
        this.logger = logger;
        this.initializeLockObjs();
    }

    public void Release(LockName lockName)
    {
        this.logger.LogInformation($"Start release thread lock: {lockName}");
        this.getLockObj(lockName).Release();
        this.logger.LogInformation($"End release thread lock: {lockName}");
    }

    public void WaitOne(LockName lockName)
    {
        this.logger.LogInformation($"Start WaitOne thread lock: {lockName}");
        this.getLockObj(lockName).WaitOne();
        this.logger.LogInformation($"End WaitOne thread lock: {lockName}");
    }

    private void initializeLockObjs()
    {
        this.lockObjs = new Dictionary<LockName, Semaphore>()
        {
            { LockName.DEHServerlessQueryLock, new Semaphore(1, 1) }
        };
    }

    private Semaphore getLockObj(LockName lockName)
    {
        var lockObj = this.lockObjs.GetValueOrDefault(lockName, null);
        if (lockObj == null)
        {
            throw new Exception($"{lockName} is not configured.");
        }
        return lockObj;
    }
}
