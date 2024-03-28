namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess;
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
        public async Task<BatchResults<DHComputingJobWrapper>> QueryJobsWithFilter(string controlId, DateTime startTime, DateTime endTime)
        {
            using (this.logger.LogElapsed($"Start to enum jobs"))
            {
                var jobs = await this.dhComputingJobRepository.QueryJobsWithFilter(controlId, startTime, endTime).ConfigureAwait(false) ?? [];
                return new BatchResults<DHComputingJobWrapper>(jobs, jobs.Count);
            }
        }

        public async Task<DHComputingJobWrapper> GetComputingJobByDQJobId(string jobId)
        {
            var job = await this.dhComputingJobRepository.GetByDQJobId(jobId).ConfigureAwait(false);
            return job ?? throw new EntityNotFoundException(new ExceptionRefEntityInfo("Fail to get computing job by DQ job id", jobId));
        }

        public async Task<DHComputingJobWrapper> CreateComputingJob(DHComputingJobWrapper job, string user)
        {
            job.CreateTime = DateTime.UtcNow;
            job.StartTime = DateTime.UtcNow;
            job.OnCreate(user, job.Id);
            return await this.dhComputingJobRepository.AddAsync(job).ConfigureAwait(false);
        }

        public async Task UpdateComputingJob(DHComputingJobWrapper job, string user)
        {
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
