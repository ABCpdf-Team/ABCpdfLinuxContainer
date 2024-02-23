# ABCpdf in a Linux Container

Linux has been supported since version 13 of ABCpdf. Here is an example project to show how to run ABCpdf as a containerized microservice using a Docker image based on the official Microsoft ASP.NET Core (in turn based on Ubuntu 22.04 LTS). This allows in-container debugging in Visual Studio 2022 and later. You may use this as a template for your own ABCpdf-powered microservice.

This project was initially generated using the ASP.NET Core Web API template using Visual Studio 2022 with the default options of Docker and OpenAPI support enabled. It uses the minimal API model to expose a test endpoint.

## Building and Running the Application

### Pre-requisites

* Visual Studio 2022 or Later
* [Docker Desktop](https://www.docker.com/) or [Docker Community Edition](https://docs.docker.com/engine/install/)

### Running the Application

Clone the ABCpdfLinuxContainer repository and open the solution in Visual Studio 2022.

Select Docker from the Debugging toolbar dropdown if it isn't already selected. You may be prompted to start Docker Desktop which you should do - it has to be running to run the project in a container.

!["Docker Debug Toolbar"](.img/docker-debug-toolbar.png)

The first time the solution is opened and Docker debugging is selected you will have to wait some time while Visual Studio performs the background task of "Warming up Docker debugging". This is performing a preliminary build of the Dockerfile including pulling the required images from Docker Hub. To see what it is doing and when it has finished check the "Output" tab and "Container Tools" from the dropdown.

**Be prepared for this to take 5 or more minutes the first time.** This is the only time you have to wait this long as the build is cached making the subsequent development workflow very fast.

While you wait you should find your ABCpdf license key and paste it into the Program.cs file where indicated near the top of the file.

```C#
if(!XSettings.InstallLicense("[-- PASTE YOUR LICENSE CODE HERE --]")) {
    throw new Exception("License failed installation.");
}
```

Once the Dockerfile has been built as indicated in the Container Tools Output window you can run the application from the debug toolbar as Docker:

This will spin up a container to run the application launch your default browser to the OpenAPI swagger page.

### Trying It Out

The Swagger UI will show one GET endpoint of `/htmltopdf/` which simply implements [AddImageHtml()](https://www.websupergoo.com/helppdfnet/default.htm?page=source%2f5-abcpdf%2fdoc%2f1-methods%2faddimagehtml.htm) on the text.

Expand the section for this endpoint and click the "Try it out" button and enter some HTML like the following.

```html
<p><strong>Hello</strong> <em>world</em> &#128578;
```

!["Swagger Interface"](.img/SwaggerInterface.png)

Now you should see the byte array contents of a PDF document displayed as text in the "Response Body" text area which is not very useful!

To actually view the generated PDF copy the link in the "Request URL" and paste it into the address bar of your browser. It will be something like the following but with a randomly generated port:

```bash
http://localhost:5521/htmltopdf?htmlString=%3Cb%3EHello%3C%2Fb%3E%20%3Cem%3Eworld%3C%2Fem%3E
```
This should load up a PDF in the browser as follows:
!["PDF Test OUtput"](.img/PDFoutput.png)

### Language Support

The abcpdf Docker image used have support for a number of language character sets. You may test this with the application using the following Ukrainian, Arabic and Hebrew language examples:

```html
<p><strong>Привіт</strong> <em>Світ</em></p>
<p><strong>مرحبا</strong> <em>بالعالم</em></p>
<p><strong>שלום</strong> <em>עולם</em></p>
```

## Installing Additional Languages

### Noto Fonts

For other languages you will need to install additional fonts and/or language pack resources in the Dockerfile.

A good balance for CJK languages is to simply add the installation of the [Google's Noto fonts](https://fonts.google.com/noto) CJK package to the Dockerfile:

```Dockerfile
FROM abcpdf/mcr-aspnet:8.0-jammy AS base
WORKDIR /app
EXPOSE 8080
RUN apt-get update && apt-get install -y fonts-noto-cjk
RUN fc-cache -f -v
```

There are [additional Noto languages packages here](https://packages.debian.org/sid/fonts-noto).

### Language Pack Installation

Alternatively you may install the relevant language packs using following commands to the runtime Dockerfile:

```Dockerfile
FROM abcpdf/mcr-aspnet:8.0-jammy AS base
WORKDIR /app
EXPOSE 8080
RUN apt-get update
# Japanese
RUN apt-get install -y language-pack-ja install japan*
# Chinese
RUN apt-get install -y language-pack-zh* chinese*
# Korean
RUN apt-get install -y language-pack-ko install korean*
```

Other languages may be similarly installed. See [the Ubuntu language pack pages](https://packages.ubuntu.com/search?keywords=language-pack) to find your desired language pack.

## ABCpdf Runtime Docker Images

The .NET 8.0 runtime image used in this project is [abcpdf/mcr-aspnet:8.0-jammy](https://hub.docker.com/repository/docker/abcpdf/mcr-aspnet/general) which is based on the [official Microsoft ASP.NET Core Runtime](https://hub.docker.com/_/microsoft-dotnet-aspnet/) but also includes the requisite libraries required by the linux-native components of ABCpdf as well as a basic set of fonts.

The Dockerfiles used to create the Docker Hub Docker images are [available here](https://hub.docker.com/repositories/abcpdf). You may use these to roll-your-own image.

## Container Security Considerations

### Base Image

Due to it's frequent security update cycle we recommend that you use images based on the latest Ubuntu LTS version. We also recommend that you use the latest LTS version of .NET.

we also suggest that add an `RUN apt-get upgrade -y` step in the runtime phase of your own Dockerfile.

### Non-root user

The Dockerfile we use in this project makes use of the 'app' USER as specified in the [ASP.NET Core Runtime images](https://hub.docker.com/_/microsoft-dotnet-aspnet/). This ensures that root access is unavailable in the deployed container in production.

### Reducing Container Attack Surface

Due to ABCpdf.NET and ABCChrome requiring linux-native components it is currently problematic to provide a [chiseled Ubuntu](https://github.com/canonical/chisel) image due to the limited number of [libraries that have so far been sliced](https://github.com/canonical/chisel-releases/tree/ubuntu-22.04/slices). This is however improving all the time but until then you will have to create your own library slices to create your chiseled image.

A better solution may be to use [slim toolkit](https://github.com/slimtoolkit/slim) prior to deployment to reduce the number of unnecessary components, and hence attack surface, in your deployed container. You will need to ensure that the probes that you utilize in your pipeline provide adequate data for the Slim profiler to pick up all of ABCpdf dependencies. More information can be found in the slimtoolkit repo's readme.

## Further Reading

Please refer to [the latest ABCpdf linux documentation](https://www.websupergoo.com/helppdfnet/default.htm?page=source%2f2-getting_started%2f6-platforms.htm) for further information.