# Using ABCpdf 13+ in a Linux Container

With Linux support now added for version 13 of ABCpdf we have put together an example project to show how to run ABCpdf as a containerized microservice.

## Project Notes

This project was initially generated using the ASP.NET Core Web API template using Visual Studio 2022 with the default options of Docker and OpenAPI support enabled.

### Linux Runtime Container considerations

ABCpdf uses Windows and Linux native code - that's what makes it so fast. This means that the native ABCpdf modules have some prerequisite linux libraries. We have made some Ubuntu 22.04 based docker images with all the needed libraries over at
[Docker Hub](https://hub.docker.com/repository/docker/abcpdf/ubuntu-22.04-aspnet).

### Further Reading

Please refer to [the latest ABCpdf linux documentation](https://www.websupergoo.com/helppdfnet/default.htm?page=source%2f2-getting_started%2f6-platforms.htm) for further information.

**NB: You will need to paste in your Professional License key into the Program.cs file as indicated below:**

```C#
if(!XSettings.InstallLicense("[-- PASTE YOUR LICENSE CODE HERE --]")) {
    throw new Exception("License failed installation.");
}
```
