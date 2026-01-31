FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Fintech_API.sln", "."]
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

RUN dotnet restore "Fintech_API.sln"

COPY . .
RUN dotnet build "Fintech_API.sln" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "FintechAPI/Fintech_API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fintech_API.dll"]
