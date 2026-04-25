FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

RUN apt-get update \
	&& apt-get install -y --no-install-recommends libgssapi-krb5-2 \
	&& rm -rf /var/lib/apt/lists/*

COPY OpenLayersDotnetTest/OpenLayersDotnetTest.csproj OpenLayersDotnetTest/
RUN dotnet restore OpenLayersDotnetTest/OpenLayersDotnetTest.csproj

FROM restore AS development
WORKDIR /src/OpenLayersDotnetTest

ENV ASPNETCORE_HTTP_PORTS=8080 \
	DOTNET_USE_POLLING_FILE_WATCHER=1

EXPOSE 8080

CMD ["dotnet", "watch", "run", "--no-launch-profile"]

FROM restore AS build
COPY . .
WORKDIR /src/OpenLayersDotnetTest
RUN dotnet publish OpenLayersDotnetTest.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
	&& apt-get install -y --no-install-recommends libgssapi-krb5-2 \
	&& rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OpenLayersDotnetTest.dll"]
