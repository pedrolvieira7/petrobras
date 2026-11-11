
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY PetrobrasFInin/PetrobrasFInin.csproj PetrobrasFInin/
RUN dotnet restore PetrobrasFInin/PetrobrasFInin.csproj

COPY PetrobrasFInin/ PetrobrasFInin/
WORKDIR /src/PetrobrasFInin

RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "PetrobrasFInin.dll"]
