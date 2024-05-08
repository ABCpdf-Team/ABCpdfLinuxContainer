using WebSupergoo.ABCpdf13;

// Set your license code below - preferably using secrets in production
string license = "[-- PASTE YOUR LICENSE CODE HERE --]";
if (!XSettings.InstallLicense(license)) {
	throw new Exception("License failed installation.");
}
Console.WriteLine($"License: {XSettings.LicenseDescription}");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
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
.WithOpenApi()
.WithDescription("Renders the submitted HTML or URL into a PDF file.")
.Produces<byte[]>(StatusCodes.Status200OK, "application/pdf");

app.Run();
