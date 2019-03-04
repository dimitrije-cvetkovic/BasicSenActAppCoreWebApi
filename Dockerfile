FROM microsoft/dotnet:2.2.1-aspnetcore-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
RUN git clone https://github.com/dimitrije-cvetkovic/BasicSenActAppCoreWebApi.git ./

RUN dotnet restore "BasicSenActAppCoreWebApi/BasicSenActAppCoreWebApi.csproj"
WORKDIR /src/BasicSenActAppCoreWebApi

FROM build AS publish
RUN dotnet publish "BasicSenActAppCoreWebApi.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "BasicSenActAppCoreWebApi.dll"]