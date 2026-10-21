FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json ./
COPY src/BatuSales.Api/BatuSales.Api.csproj src/BatuSales.Api/
RUN dotnet restore src/BatuSales.Api/BatuSales.Api.csproj
COPY src/BatuSales.Api/ src/BatuSales.Api/
RUN dotnet publish src/BatuSales.Api/BatuSales.Api.csproj -c Release --no-restore -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /publish ./
USER $APP_UID
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "BatuSales.Api.dll"]
