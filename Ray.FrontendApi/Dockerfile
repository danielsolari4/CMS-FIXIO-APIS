#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Ray.FrontendApi/Ray.FrontendApi.csproj", "Ray.FrontendApi/"]
RUN dotnet restore "Ray.FrontendApi/Ray.FrontendApi.csproj"
COPY . .
WORKDIR "/src/Ray.FrontendApi"
RUN dotnet build "Ray.FrontendApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ray.FrontendApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ray.FrontendApi.dll"]