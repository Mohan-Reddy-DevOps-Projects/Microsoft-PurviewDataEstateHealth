FROM mcr.microsoft.com/dotnet/aspnet:7.0-cbl-mariner2.0
WORKDIR /workerservice
COPY . .

# Install the public AME root certificate in the trusted CA folder
COPY ameroot.crt /etc/pki/ca-trust/source/anchors/ameroot.crt
RUN update-ca-trust

ENTRYPOINT ["dotnet", "Microsoft.Azure.Purview.Share.WorkerService.dll"]
