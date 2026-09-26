FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Rino.FrontendApi/Rino.FrontendApi.csproj", "Rino.FrontendApi/"]
COPY ["Rino.Managers/Rino.Managers.csproj", "Rino.Managers/"]
COPY ["Rino.Repositories/Rino.Repositories.csproj", "Rino.Repositories/"]
COPY ["Rino.Dtos/Rino.Dtos.csproj", "Rino.Dtos/"]
COPY ["Rino.Model/Rino.Model.csproj", "Rino.Model/"]
COPY ["Rino.Common/Rino.Common.csproj", "Rino.Common/"]
COPY ["Rino.Utils/Rino.Utils.csproj", "Rino.Utils/"]

RUN dotnet restore "Rino.FrontendApi/Rino.FrontendApi.csproj"

COPY . .
WORKDIR "/src/Rino.FrontendApi"
RUN rm -f /src/global.json && dotnet publish "Rino.FrontendApi.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Rino.FrontendApi.dll"]
