
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["./Apis/WebAPI/WebAPI.csproj", "Apis/WebAPI/"]
COPY ["./Apis/Infrastructures/Infrastructures.csproj", "Apis/Infrastructures/"]
COPY ["./Apis/Application/Application.csproj", "Apis/Application/"]
COPY ["./Apis/Domain/Domain.csproj", "Apis/Domain/"]
RUN dotnet restore "./Apis/WebAPI/WebAPI.csproj"
COPY . .
WORKDIR "/src/Apis/WebAPI"
RUN dotnet build "WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebAPI.dll"]