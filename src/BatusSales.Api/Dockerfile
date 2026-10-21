FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo
COPY src/BatusSales.Api/BatusSales.Api.csproj src/BatusSales.Api/
RUN dotnet restore src/BatusSales.Api/BatusSales.Api.csproj
COPY src/BatusSales.Api/ src/BatusSales.Api/
RUN dotnet publish src/BatusSales.Api/BatusSales.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "BatusSales.Api.dll"]
