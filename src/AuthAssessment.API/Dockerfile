FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/AuthAssessment.Domain/AuthAssessment.Domain.csproj AuthAssessment.Domain/
COPY src/AuthAssessment.Application/AuthAssessment.Application.csproj AuthAssessment.Application/
COPY src/AuthAssessment.Infrastructure/AuthAssessment.Infrastructure.csproj AuthAssessment.Infrastructure/
COPY src/AuthAssessment.API/AuthAssessment.API.csproj AuthAssessment.API/
RUN dotnet restore AuthAssessment.API/AuthAssessment.API.csproj

COPY src/AuthAssessment.Domain/ AuthAssessment.Domain/
COPY src/AuthAssessment.Application/ AuthAssessment.Application/
COPY src/AuthAssessment.Infrastructure/ AuthAssessment.Infrastructure/
COPY src/AuthAssessment.API/ AuthAssessment.API/
RUN dotnet publish AuthAssessment.API/AuthAssessment.API.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AuthAssessment.API.dll"]
