using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using WebSupergoo.ABCpdf14;

namespace ABCpdfLinuxContainer.Tests;

public sealed class HtmlToPdfEndpointTests : IAsyncLifetime
{
	string _licenseKey = "";
	IFutureDockerImage? _image;
	IContainer? _container;
	HttpClient? _client;

	public async ValueTask InitializeAsync()
	{
		// Checked first so a missing key fails immediately instead of after building the image.
		_licenseKey = Environment.GetEnvironmentVariable(ABCpdfLicenseInstaller.EnvVarName) is { Length: > 0 } key
			? key
			: throw new InvalidOperationException($"{ABCpdfLicenseInstaller.EnvVarName} must be set to run these integration tests.");

		_image = new ImageFromDockerfileBuilder()
			.WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
			.WithDockerfile("ABCpdfLinuxContainer/Dockerfile")
			.WithDeleteIfExists(true)
			.Build();
		await _image.CreateAsync();

		_container = new ContainerBuilder(_image)
			.WithPortBinding(8080, true)
			.WithEnvironment(ABCpdfLicenseInstaller.EnvVarName, _licenseKey)
			.WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/health")))
			.Build();
		await _container.StartAsync();

		_client = new HttpClient { BaseAddress = new Uri($"http://localhost:{_container.GetMappedPublicPort(8080)}") };

		ABCpdfLicenseInstaller.InstallAndValidate(_ => _licenseKey, null, _ => false, _ => []);
	}

	public async ValueTask DisposeAsync()
	{
		if (_container is not null)
			await _container.DisposeAsync();
	}

	[Fact]
	public async Task Health_endpoint_reports_healthy()
	{
		var response = await _client!.GetAsync("/health", TestContext.Current.CancellationToken);

		Assert.True(response.IsSuccessStatusCode);
	}

	[Fact]
	public async Task Htmltopdf_returns_pdf_containing_the_requested_text()
	{
		// No hyphens: Chrome can wrap/hyphenate at a "-" during layout, and text extraction then drops it.
		var marker = $"integrationtest{Guid.NewGuid():N}";
		var html = Uri.EscapeDataString($"<html><body><h1>{marker}</h1></body></html>");

		var ct = TestContext.Current.CancellationToken;
		var response = await _client!.GetAsync($"/htmltopdf?htmlOrUrl={html}", ct);
		response.EnsureSuccessStatusCode();
		var pdfBytes = await response.Content.ReadAsByteArrayAsync(ct);

		using var doc = new Doc();
		doc.Read(pdfBytes);
		var text = doc.GetText("Text");

		Assert.Contains(marker, text);
	}

	[Fact]
	public async Task Container_logs_show_the_license_was_installed()
	{
		var (stdout, _) = await _container!.GetLogsAsync(ct: TestContext.Current.CancellationToken);

		Assert.Contains("ABCpdf license installed:", stdout);
	}
}
