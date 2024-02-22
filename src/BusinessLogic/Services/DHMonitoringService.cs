namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.OData.Edm;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.JobMonitoring;
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

        public async Task CreateComputingJob(DHComputingJobWrapper job)
        {
            job.CreateTime = Date.Now;
            await this.dhComputingJobRepository.AddAsync(job).ConfigureAwait(false);
        }

        public async Task UpdateComputingJobStatus(string jobId, string status)
        {
            var job = await this.GetComputingJobById(jobId);
            job.Status = status;
            await this.dhComputingJobRepository.UpdateAsync(job).ConfigureAwait(false);
        }

        public async Task StartComputingJob(string jobId, DHComputingJobStatus status)
        {
            var job = await this.GetComputingJobById(jobId);
            job.StartTime = Date.Now;
            await this.dhComputingJobRepository.UpdateAsync(job).ConfigureAwait(false);
        }

        public async Task EndComputingJob(string jobId, DHComputingJobStatus status)
        {
            var job = await this.GetComputingJobById(jobId);
            job.EndTime = Date.Now;
            await this.dhComputingJobRepository.UpdateAsync(job).ConfigureAwait(false);
        }

        private async Task UpdateComputingJob(DHComputingJobWrapper job)
        {
            await this.dhComputingJobRepository.UpdateAsync(job).ConfigureAwait(false);
        }

        private async Task<DHComputingJobWrapper> GetComputingJobById(string jobId)
        {
            var job = await this.dhComputingJobRepository.GetByIdAsync(jobId).ConfigureAwait(false);
            return job ?? throw new ComputingJobNotFoundException();
        }


    }
}
