namespace ABCpdfLinuxContainer.Tests;

public sealed class HtmlToPdfEndpointTests(HtmlToPdfContainerFixture fixture) : IClassFixture<HtmlToPdfContainerFixture>
{
	[Fact]
	public async Task Health_endpoint_reports_healthy()
	{
		var response = await fixture.Client.GetAsync("/health", TestContext.Current.CancellationToken);

		Assert.True(response.IsSuccessStatusCode);
	}

	[Fact]
	public async Task Htmltopdf_returns_pdf_containing_the_requested_text()
	{
		// No hyphens: Chrome can wrap/hyphenate at a "-" during layout, and text extraction then drops it.
		var marker = $"integrationtest{Guid.NewGuid():N}";
		var html = Uri.EscapeDataString($"<html><body><h1>{marker}</h1></body></html>");

		var ct = TestContext.Current.CancellationToken;
		var response = await fixture.Client.GetAsync($"/htmltopdf?htmlOrUrl={html}", ct);
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
		var (stdout, _) = await fixture.Container.GetLogsAsync(ct: TestContext.Current.CancellationToken);

		Assert.Contains("ABCpdf license installed:", stdout);
	}
}
