#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RestOlympe_Server/RestOlympe_Server.csproj", "RestOlympe_Server/"]
RUN dotnet restore "./RestOlympe_Server/RestOlympe_Server.csproj"
COPY . .
WORKDIR "/src/RestOlympe_Server"
RUN dotnet build "./RestOlympe_Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RestOlympe_Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy the self-signed certificate
COPY ["../certificate.pfx", "/https/"]

# Set environment variables to configure ASP.NET Core to use the certificate
ENV ASPNETCORE_URLS=${ASPNETCORE_URLS}
ENV ASPNETCORE_HTTPS_PORT=${ASPNETCORE_HTTPS_PORT}
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=${ASPNETCORE_Kestrel__Certificates__Default__Password}
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=${ASPNETCORE_Kestrel__Certificates__Default__Path}

ENTRYPOINT ["dotnet", "RestOlympe_Server.dll"]