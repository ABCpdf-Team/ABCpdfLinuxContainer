FROM abcpdf/mcr-aspnet:8.0-jammy AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ABCpdfLinuxContainer/ABCpdfLinuxContainer.csproj", "ABCpdfLinuxContainer/"]
RUN dotnet restore "./ABCpdfLinuxContainer/./ABCpdfLinuxContainer.csproj"
COPY . .
WORKDIR "/src/ABCpdfLinuxContainer"
RUN dotnet build "./ABCpdfLinuxContainer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ABCpdfLinuxContainer.csproj" -c $BUILD_CONFIGURATION -o /app/publish -r linux-x64 --self-contained true
# This is requried to ensure ABCChrome can be launched by user "app" in production
RUN chmod o+x /app/publish/ABCChrome117/ABCChrome117

######################################################################################################################################
# In debug mode the /app folder points to the project folder even if we try to copy the artefacts from the publshing stage into it. 
# For projects with more complicated linux-native components like ABCChrome this means we have to grab the requiered artefacts from 
# the 'publish' stage and copy them somewhere in the PATH - so in this case /usr/local/bin for ubuntu.
#
# IMPORTANT
#
#    * For the following debug stage to be used the following property must be set the project file:
#           <PropertyGroup>
#               <DockerfileFastModeStage>debug</DockerfileFastModeStage>
#           </PropertyGroup>
#
#    * The ABCChrome version number WILL need to be chanegd when later versions of ABCpdf updates ABCChrome.
#
#    * The path you copy the native ABCChrome folder must locateable be in the container's PATH.
#
# For more information on debugging in docker with Visual Studio see:
# https://learn.microsoft.com/en-gb/visualstudio/containers/container-build?view=vs-2022#modify-container-image-only-for-debugging
######################################################################################################################################
FROM base AS debug
WORKDIR /app
COPY --from=publish /app/publish/ABCChrome117 /usr/local/bin/ABCChrome117

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ABCpdfLinuxContainer.dll"]

