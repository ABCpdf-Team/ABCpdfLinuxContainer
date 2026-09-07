namespace ABCpdfLinuxContainer;

public static class ABCpdfLicenseInstaller
{
	public static void Install(string envVarName) => Install(envVarName, secretsFilePath: null);

	public static void Install(string envVarName, string? secretsFilePath)
	{
		var key = Environment.GetEnvironmentVariable(envVarName);

		if (string.IsNullOrWhiteSpace(key) && secretsFilePath is not null && File.Exists(secretsFilePath))
		{
			var line = Array.Find(File.ReadAllLines(secretsFilePath), l => l.StartsWith($"{envVarName}="));
			if (line is not null)
				key = line[(envVarName.Length + 1)..].Trim().Trim('"');
		}

		if (string.IsNullOrWhiteSpace(key))
			throw new InvalidOperationException($"ABCpdf license key is not configured. Please set the '{envVarName}' environment variable.");

		if (!XSettings.InstallLicense(key, throwOnFailure: false))
			throw new InvalidOperationException("ABCpdf license is invalid. Please verify the configured license key.");

		Console.WriteLine($"ABCpdf license installed: {XSettings.LicenseDescription}");
	}
}
