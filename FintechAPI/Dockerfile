# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FintechAPI/Fintech_API.csproj", "FintechAPI/"]
COPY ["CommonLibrary/Common.Settings/Common.Settings.csproj", "CommonLibrary/Common.Settings/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["CommonLibrary/Common.Authorization/Common.Authorization.csproj", "CommonLibrary/Common.Authorization/"]
COPY ["Model/Entity/Entity.csproj", "Model/Entity/"]
COPY ["CommonLibrary/Common.UnitOfWork/Common.UnitOfWork.csproj", "CommonLibrary/Common.UnitOfWork/"]
COPY ["CommonLibrary/Common.Constant/Common.Constant.csproj", "CommonLibrary/Common.Constant/"]
COPY ["CommonLibrary/Common.Utils/Common.Utils.csproj", "CommonLibrary/Common.Utils/"]
COPY ["Model/Model/Model.csproj", "Model/Model/"]
COPY ["DomainService/DomainService.csproj", "DomainService/"]
RUN dotnet restore "./FintechAPI/Fintech_API.csproj"
COPY . .
WORKDIR "/src/FintechAPI"
RUN dotnet build "./Fintech_API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Fintech_API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fintech_API.dll"]