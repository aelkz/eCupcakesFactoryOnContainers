FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src

COPY src/Common.Utils/ /src/Common.Utils/
COPY src/signalr-proxy-kafka/ /src/signalr-proxy-kafka/
COPY src/Services/Service.Decorating/DecoratorService.csproj /src/Services/HostedService.Decorator/

RUN dotnet restore /src/Services/HostedService.Decorator/DecoratorService.csproj

WORKDIR /src/Services/HostedService.Decorator
COPY src/Services/Service.Decorating/ .

RUN dotnet build ./DecoratorService.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish ./DecoratorService.csproj -c Release -o /app

#FROM build as unittest
#WORKDIR /src/src/Services/Ordering/Ordering.UnitTests

FROM base AS final

RUN mkdir -p /usr/local/etc/openssl
RUN cd /usr/local/etc/openssl
WORKDIR /usr/local/etc/openssl
RUN curl https://curl.haxx.se/ca/cacert.pem -o cert.pem

WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "DecoratorService.dll"]