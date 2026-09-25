FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Ray.BackendApi/Ray.BackendApi.csproj", "Ray.BackendApi/"]
COPY ["Ray.Managers/Ray.Managers.csproj", "Ray.Managers/"]
COPY ["Ray.Repositories/Ray.Repositories.csproj", "Ray.Repositories/"]
COPY ["Ray.Dtos/Ray.Dtos.csproj", "Ray.Dtos/"]
COPY ["Ray.Model/Ray.Model.csproj", "Ray.Model/"]
COPY ["Ray.Common/Ray.Common.csproj", "Ray.Common/"]
COPY ["Ray.Utils/Ray.Utils.csproj", "Ray.Utils/"]

RUN dotnet restore "Ray.BackendApi/Ray.BackendApi.csproj"

COPY . .
WORKDIR "/src/Ray.BackendApi"
RUN rm -f /src/global.json && dotnet publish "Ray.BackendApi.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ray.BackendApi.dll"]