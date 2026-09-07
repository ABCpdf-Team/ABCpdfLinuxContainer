using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using Microsoft.Extensions.Logging;

namespace ABCpdfLinuxContainer.Tests;

// Builds the image and starts the container once and shares it across every test in
// HtmlToPdfEndpointTests via IClassFixture, rather than per test method.
public sealed class HtmlToPdfContainerFixture : IAsyncLifetime
{
	const string EnvVarName = "ABCPDF_LICENSE_KEY";

	IFutureDockerImage? _image;
	IContainer? _container;

	public HttpClient Client { get; private set; } = null!;
	public IContainer Container => _container!;

	public async ValueTask InitializeAsync()
	{
		var secretsFilePath = Path.Combine(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath, ".secrets");

		// Checked first so a missing key fails immediately instead of after building the image.
		var licenseKey = ResolveLicenseKey(secretsFilePath);

		// The default wait-strategy timeout is 1 hour: if the container never becomes healthy (a
		// crash, a networking issue, a license failure, etc.) that reads as a hang in CI rather than
		// a fast, clear failure. This only bounds the post-startup health wait, not the image build.
		TestcontainersSettings.WaitStrategyTimeout = TimeSpan.FromSeconds(90);

		var logger = new ConsoleLogger();

		_image = new ImageFromDockerfileBuilder()
			.WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
			.WithDockerfile("ABCpdfLinuxContainer/Dockerfile")
			.WithDeleteIfExists(true)
			.WithLogger(logger)
			.Build();
		await _image.CreateAsync();

		_container = new ContainerBuilder(_image)
			.WithPortBinding(8080, true)
			.WithEnvironment(EnvVarName, licenseKey)
			.WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/health")))
			.WithLogger(logger)
			.Build();
		await _container.StartAsync();

		Client = new HttpClient { BaseAddress = new Uri($"http://localhost:{_container.GetMappedPublicPort(8080)}") };

		ABCpdfLicenseInstaller.Install(EnvVarName, secretsFilePath);
	}

	public async ValueTask DisposeAsync()
	{
		Client?.Dispose();
		if (_container is not null)
			await _container.DisposeAsync();
	}

	// Mirrors Program.cs: the env var wins, falling back to a git-ignored .secrets file (see
	// .secrets.example) at the repo root so these tests can run locally without exporting it.
	static string ResolveLicenseKey(string secretsFilePath)
	{
		var key = Environment.GetEnvironmentVariable(EnvVarName);
		if (!string.IsNullOrWhiteSpace(key))
			return key;

		if (File.Exists(secretsFilePath))
		{
			var line = Array.Find(File.ReadAllLines(secretsFilePath), l => l.StartsWith($"{EnvVarName}="));
			if (line is not null)
				return line[(EnvVarName.Length + 1)..].Trim().Trim('"');
		}

		throw new InvalidOperationException($"{EnvVarName} must be set (as an environment variable or in a .secrets file at the repo root) to run these integration tests.");
	}

	// Writes straight to stdout with no buffering/provider setup, so progress is visible live in CI
	// instead of only after the whole test run completes.
	sealed class ConsoleLogger : ILogger
	{
		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
			Console.WriteLine($"[testcontainers:{logLevel}] {formatter(state, exception)}{(exception is null ? "" : $" - {exception}")}");
	}
}
