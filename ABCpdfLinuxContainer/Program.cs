using WebSupergoo.ABCpdf13;

// Set your license code below - preferably using secrets in production
if(!XSettings.InstallLicense("[-- PASTE YOUR LICENSE CODE HERE --]")) {
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

app.MapGet("/htmltopdf", (string htmlString) => {
	using Doc doc = new();
	doc.AddImageHtml(htmlString);
	return Results.File(doc.GetData(), contentType: "application/pdf");
})
.WithOpenApi()
.WithDescription("Renders the submitted html string into a PDF file.")
.Produces<byte[]>(StatusCodes.Status200OK, "application/pdf");

app.Run();
