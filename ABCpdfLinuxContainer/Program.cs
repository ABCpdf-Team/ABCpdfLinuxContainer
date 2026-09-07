using ABCpdfLinuxContainer;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
// Local testing only: a git-ignored .secrets file (see .secrets.example) at the repo root - one
// level above the project's content root - can supply the key when the environment variable isn't set.
var repoRoot = Directory.GetParent(builder.Environment.ContentRootPath)?.FullName ?? builder.Environment.ContentRootPath;
ABCpdfLicenseInstaller.Install("ABCPDF_LICENSE_KEY", Path.Combine(repoRoot, ".secrets"));
#else
ABCpdfLicenseInstaller.Install("ABCPDF_LICENSE_KEY");
#endif

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if(app.Environment.IsDevelopment()) {
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.MapHealthChecks("/health");

app.MapGet("/htmltopdf", (string htmlOrUrl) => {
	using Doc doc = new();
	if (htmlOrUrl.StartsWith("http"))
		doc.AddImageUrl(htmlOrUrl);
	else
		doc.AddImageHtml(htmlOrUrl);
	return Results.File(doc.GetData(), contentType: "application/pdf", fileDownloadName: "mypage.pdf");
})
.WithDescription("Renders the submitted HTML or URL into a PDF file.")
.Produces<byte[]>(StatusCodes.Status200OK, "application/pdf");

app.Run();
