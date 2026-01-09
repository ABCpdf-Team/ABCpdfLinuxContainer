using WebSupergoo.ABCpdf13;

var builder = WebApplication.CreateBuilder(args);

// Set the license from dotnet secrets
var abcPdfLicense = builder.Configuration["ABCpdf:LicenseKey"];

if (!XSettings.InstallLicense(abcPdfLicense)) {
	throw new Exception($"License failed installation: {abcPdfLicense}");
}

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if(app.Environment.IsDevelopment()) {
	app.UseSwagger();
	app.UseSwaggerUI();
}

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
