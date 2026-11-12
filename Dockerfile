FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY PetrobrasFInin/PetrobrasFInin.csproj PetrobrasFInin/
COPY PetrobrasFInin/global.json PetrobrasFInin/
RUN cd PetrobrasFInin && dotnet restore PetrobrasFInin.csproj

COPY PetrobrasFInin/ PetrobrasFInin/
WORKDIR /src/PetrobrasFInin
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
EXPOSE 8080
ENTRYPOINT ["dotnet", "PetrobrasFInin.dll"]
