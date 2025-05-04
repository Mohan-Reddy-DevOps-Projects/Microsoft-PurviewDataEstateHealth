namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using AspNetCore.Authorization;
using Configurations;
using Loggers;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

public class CertificateAuthorizationHandlerRequirement : IAuthorizationRequirement;

public class CertificateAuthorizationHandler(
    IHttpContextAccessor httpContextAccessor,
    IOptions<AllowListedCertificateConfiguration> certConfig,
    IOptions<EnvironmentConfiguration> environmentConfig,
    IDataEstateHealthRequestLogger logger)
    : AuthorizationHandler<CertificateAuthorizationHandlerRequirement>
{
    private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private readonly IOptions<AllowListedCertificateConfiguration> certConfig = certConfig ?? throw new ArgumentNullException(nameof(certConfig));
    private readonly IOptions<EnvironmentConfiguration> environmentConfig = environmentConfig ?? throw new ArgumentNullException(nameof(environmentConfig));
    private readonly IDataEstateHealthRequestLogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private static readonly Dictionary<string, CertificateSet> controllerToCertificateSetMap;

    /// <inheritdoc />
    static CertificateAuthorizationHandler()
    {
        const string controllerPostfix = "Controller";
        controllerToCertificateSetMap = typeof(DataPlaneController).Assembly
            .GetTypes()
            .Select(t => (TypeName: t.Name, Attr: t.GetCustomAttribute<CertificateConfigAttribute>()))
            .Where(t => t.Attr != null)
            .Distinct()
            .ToDictionary(
                t => t.TypeName.EndsWith(controllerPostfix)
                    ? t.TypeName[..^controllerPostfix.Length]
                    : t.TypeName,
                t => t.Attr.CertificateSet);
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CertificateAuthorizationHandlerRequirement requirement)
    {
        var httpContext = this.httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            this.logger.LogWarning("HttpContext is null during authorization check");
            context.Fail();
            return Task.CompletedTask;
        }

        var clientCert = httpContext.Connection.ClientCertificate;

        switch (clientCert)
        {
            case null when this.environmentConfig.Value.IsDevelopmentEnvironment():
                context.Succeed(requirement);
                return Task.CompletedTask;
            case null:
                this.logger.LogWarning("Client certificate is null during authorization check");
                context.Fail();
                return Task.CompletedTask;
        }

        if (httpContext.Request.RouteValues["controller"] is not string controllerName)
        {
            this.logger.LogWarning("Controller name not found in route values");
            context.Fail();
            return Task.CompletedTask;
        }

        this.logger.LogDebug("Checking certificate mapping for controller: {ControllerName}", controllerName);

        if (!controllerToCertificateSetMap.TryGetValue(controllerName, out var certificateSet))
        {
            this.logger.LogWarning("No certificate set mapping found for controller: {ControllerName}", controllerName);
            context.Fail();
            return Task.CompletedTask;
        }

        var allowListedSubjectNames = this.certConfig.Value.GetAllowListedSubjectNames(certificateSet);

        string simpleName = clientCert.GetNameInfo(X509NameType.SimpleName, false);

        if (allowListedSubjectNames.Contains(simpleName, StringComparer.OrdinalIgnoreCase))
        {
            this.logger.LogInformation("Certificate validation succeeded.");
            context.Succeed(requirement);
        }
        else
        {
            this.logger.LogWarning("Certificate subject '{Subject}' is not allowed for controller '{Controller}'",
                simpleName, controllerName);
            context.Fail();
        }

        return Task.CompletedTask;
    }
}