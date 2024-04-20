#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Apis/WebAPI/WebAPI.csproj", "src/"]
COPY ["Apis/Infrastructures/Infrastructures.csproj", "src/"]
COPY ["Apis/Application/Application.csproj", "src/"]
COPY ["Apis/Domain/Domain.csproj", "src/"]
RUN dotnet restore "src/WebAPI.csproj"
COPY . .
WORKDIR /src
RUN dotnet build "WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebAPI.dll"]