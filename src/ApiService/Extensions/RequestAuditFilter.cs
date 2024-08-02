// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Extensions
{
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using Microsoft.Extensions.Logging;
    using OpenTelemetry.Audit.Geneva;
    using System;

    public class RequestAuditFilter(IRequestHeaderContext requestHeaderContext, IDataEstateHealthRequestLogger logger) : IActionFilter
    {

        public void OnActionExecuted(ActionExecutedContext context)
        {
            logger.LogInformation("RequestAuditFilter.OnActionExecuted");
            if (context != null)
            {
                this.Audit(context);
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

        private void Audit(ActionExecutedContext context)
        {
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                logger.LogInformation("RequestAuditFilter.Audit start");
                var httpContext = context.HttpContext;
                var requestUri = httpContext.Request.GetDisplayUrl();
                var operationName = context.ActionDescriptor.DisplayName;

                var audit = new AuditRecord()
                {
                    OperationName = operationName,
                    OperationType = OperationType.Read,
                    OperationResult = OperationResult.Success,
                    OperationResultDescription = "Audit log",
                    OperationAccessLevel = "Read",
                    CallerAgent = "DEHService",
                    CallerIpAddress = "1.1.1.1"
                };

                switch (httpContext.Request.Method)
                {
                    case "GET":
                        audit.OperationType = OperationType.Read;
                        audit.OperationAccessLevel = "Read";
                        audit.AddCallerAccessLevel(audit.OperationAccessLevel);
                        break;
                    case "POST":
                        audit.OperationType = OperationType.Create;
                        audit.OperationAccessLevel = "Write";
                        audit.AddCallerAccessLevel(audit.OperationAccessLevel);
                        break;
                    case "PUT":
                    case "PATCH":
                        audit.OperationType = OperationType.Update;
                        audit.OperationAccessLevel = "Write";
                        audit.AddCallerAccessLevel(audit.OperationAccessLevel);
                        break;
                    case "DELETE":
                        audit.OperationType = OperationType.Delete;
                        audit.OperationAccessLevel = "Delete";
                        audit.AddCallerAccessLevel(audit.OperationAccessLevel);
                        break;
                }
                audit.AddOperationCategory(OperationCategory.ResourceManagement);
                audit.AddCallerAccessLevel(audit.OperationAccessLevel);
                audit.AddCallerIdentity(CallerIdentityType.TenantId, requestHeaderContext.TenantId.ToString(), "audit");
                audit.AddTargetResource("CorrelationId", requestHeaderContext.CorrelationId);
                logger.LogInformation("RequestAuditFilter.Audit start to write audit logs");
                logger.LogAudit(audit);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AuditFailure: " + ex.Message);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }
    }
}
