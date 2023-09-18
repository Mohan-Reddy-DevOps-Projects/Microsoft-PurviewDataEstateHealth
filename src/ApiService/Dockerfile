#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0-cbl-mariner2.0
WORKDIR /apiservice
EXPOSE 80
EXPOSE 443
COPY . .

COPY openssl.cnf /etc/ssl/openssl.cnf

# Install the public AME root certificate in the trusted CA folder
COPY ameroot.crt /etc/pki/ca-trust/source/anchors/ameroot.crt
RUN update-ca-trust

ENTRYPOINT ["dotnet", "Microsoft.Azure.Purview.DataEstateHealth.ApiService.dll"]
