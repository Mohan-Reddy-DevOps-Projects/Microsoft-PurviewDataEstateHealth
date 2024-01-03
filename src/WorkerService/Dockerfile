FROM mcr.microsoft.com/dotnet/aspnet:8.0-cbl-mariner2.0
WORKDIR /App
COPY . .

# Install the public AME root certificate in the trusted CA folder
COPY ameroot.crt /etc/pki/ca-trust/source/anchors/ameroot.crt
RUN update-ca-trust

#runs .NET in user mode
USER $APP_UID

ENTRYPOINT ["dotnet", "Microsoft.Azure.Purview.DataEstateHealth.WorkerService.dll"]
