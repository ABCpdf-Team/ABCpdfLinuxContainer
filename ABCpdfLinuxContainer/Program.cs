using ABCpdfLinuxContainer;
using WebSupergoo.ABCpdf14;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
// .secrets (see .secrets.example) lives at the repo root, one level above the project's content root.
var repoRoot = Directory.GetParent(builder.Environment.ContentRootPath)?.FullName ?? builder.Environment.ContentRootPath;
var secretsFilePath = Path.Combine(repoRoot, ".secrets");
#else
string? secretsFilePath = null;
#endif

ABCpdfLicenseInstaller.InstallAndValidate(Environment.GetEnvironmentVariable, secretsFilePath, File.Exists, File.ReadAllLines);

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
