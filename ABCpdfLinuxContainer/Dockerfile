FROM abcpdf/mcr-aspnet:10.0 AS base

# needed for ABCChrome123
RUN apt-get update -y
RUN apt update --fix-missing 
RUN apt-get upgrade -y
RUN apt install -y curl

USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ABCpdfLinuxContainer/ABCpdfLinuxContainer.csproj", "ABCpdfLinuxContainer/"]
RUN dotnet restore "./ABCpdfLinuxContainer/./ABCpdfLinuxContainer.csproj"
COPY . .
WORKDIR "/src/ABCpdfLinuxContainer"
RUN dotnet build "./ABCpdfLinuxContainer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ABCpdfLinuxContainer.csproj" -c $BUILD_CONFIGURATION -o /app/publish -r linux-x64
# This is required to ensure ABCChrome can be launched by user "app" in production
RUN chmod o+x /app/publish/ABCChrome123/ABCChrome123

######################################################################################################################################
# In Docker debug mode the /app folder points to the local project folder. This means linux-native components will not be found. 
# As a workaround for debugging only - we need copy ABCChrome from the 'publish' stage and copy them somewhere in the PATH such as 
# /usr/local/bin.
#
# IMPORTANT
#
#    * For the following debug stage to be used the following property must be set the project file:
#           <PropertyGroup>
#               <DockerfileFastModeStage>debug</DockerfileFastModeStage>
#           </PropertyGroup>
#
#    * The ABCChrome version number WILL need to be changed when later versions of ABCpdf update ABCChrome.
#
#    * The path you copy the native ABCChrome folder must be locatable in the container's PATH.
#
# For more information on debugging in Docker with Visual Studio see:
# https://learn.microsoft.com/en-gb/visualstudio/containers/container-build?view=vs-2022#modify-container-image-only-for-debugging
######################################################################################################################################
FROM base AS debug
WORKDIR /app
COPY --from=publish /app/publish/ABCChrome123 /usr/local/bin/ABCChrome123

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ABCpdfLinuxContainer.dll"]

