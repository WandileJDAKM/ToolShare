# docker build -t toolshare .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/ToolShare.Api/ src/ToolShare.Api/
RUN dotnet restore src/ToolShare.Api/ToolShare.Api.csproj
RUN dotnet publish src/ToolShare.Api/ToolShare.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir /data
ENV ASPNETCORE_URLS=http://+:8080 \
    ConnectionStrings__Default="Data Source=/data/toolshare.db" \
    Seed__Demo=true
VOLUME /data
EXPOSE 8080
ENTRYPOINT ["dotnet", "ToolShare.Api.dll"]
