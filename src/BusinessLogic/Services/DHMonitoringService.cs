namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.JobMonitoring;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using System;
    using System.Threading.Tasks;

    public class DHMonitoringService
    {
        private readonly IRequestContextAccessor requestContextAccessor;
        private readonly IDataEstateHealthRequestLogger logger;
        private readonly DHComputingJobRepository dhComputingJobRepository;

        public DHMonitoringService(
            IRequestContextAccessor requestContextAccessor,
            IDataEstateHealthRequestLogger logger,
            DHComputingJobRepository dhComputingJobRepository)
        {
            this.requestContextAccessor = requestContextAccessor;
            this.logger = logger;
            this.dhComputingJobRepository = dhComputingJobRepository;
        }
        public async Task<DHComputingJobWrapper> GetComputingJobByDQJobId(string jobId)
        {
            var job = await this.dhComputingJobRepository.GetByDQJobId(jobId).ConfigureAwait(false);
            return job ?? throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.MonitoringJob.ToString(), jobId));
        }

        public async Task CreateComputingJob(DHComputingJobWrapper job, string user)
        {
            job.CreateTime = DateTime.UtcNow;
            job.StartTime = DateTime.UtcNow;
            job.OnCreate(user, job.Id);
            await this.dhComputingJobRepository.AddAsync(job).ConfigureAwait(false);
        }

        public async Task UpdateComputingJobStatus(string jobId, DHComputingJobStatus status, string user)
        {
            var job = await this.GetComputingJobById(jobId);
            job.Status = status;
            job.OnUpdate(job, user);
            await this.dhComputingJobRepository.UpdateAsync(job).ConfigureAwait(false);
        }

        public async Task EndComputingJob(string jobId, string user)
        {
            var job = await this.GetComputingJobById(jobId);
            job.EndTime = DateTime.UtcNow;
            job.OnUpdate(job, user);
            await this.dhComputingJobRepository.UpdateAsync(job).ConfigureAwait(false);
        }

        private async Task<DHComputingJobWrapper> GetComputingJobById(string jobId)
        {
            var job = await this.dhComputingJobRepository.GetByIdAsync(jobId).ConfigureAwait(false);
            return job ?? throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.MonitoringJob.ToString(), jobId));
        }
    }
}
