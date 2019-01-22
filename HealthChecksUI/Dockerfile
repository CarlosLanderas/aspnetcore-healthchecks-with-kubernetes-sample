FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["HealthChecksUI/HealthChecksUI.csproj", "HealthChecksUI/"]
RUN dotnet restore "HealthChecksUI/HealthChecksUI.csproj"
COPY . .
WORKDIR "/src/HealthChecksUI"
RUN dotnet build "HealthChecksUI.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "HealthChecksUI.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "HealthChecksUI.dll"]