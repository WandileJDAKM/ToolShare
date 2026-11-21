# docker build -f src/ToolShare.Web/Dockerfile -t toolshare-web .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/ToolShare.Web/ src/ToolShare.Web/
RUN dotnet restore src/ToolShare.Web/ToolShare.Web.csproj
RUN dotnet publish src/ToolShare.Web/ToolShare.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ToolShare.Web.dll"]
