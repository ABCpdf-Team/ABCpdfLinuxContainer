using WebSupergoo.ABCpdf14;

namespace ABCpdfLinuxContainer;

public static class ABCpdfLicenseInstaller
{
	public const string EnvVarName = "ABCPDF_LICENSE_KEY";

	public static string? ResolveKey(Func<string, string?> getEnvironmentVariable, string? secretsFilePath, Func<string, bool> fileExists, Func<string, string[]> readAllLines)
	{
		var fromEnv = getEnvironmentVariable(EnvVarName);
		if (!string.IsNullOrWhiteSpace(fromEnv))
			return fromEnv;

		// Local testing only: secretsFilePath is null outside DEBUG builds, so this is skipped entirely.
		if (secretsFilePath is not null && fileExists(secretsFilePath))
		{
			var line = Array.Find(readAllLines(secretsFilePath), l => l.StartsWith($"{EnvVarName}="));
			if (line is not null)
				return line[(EnvVarName.Length + 1)..].Trim().Trim('"');
		}

		return null;
	}

	public static void InstallAndValidate(Func<string, string?> getEnvironmentVariable, string? secretsFilePath, Func<string, bool> fileExists, Func<string, string[]> readAllLines)
	{
		var key = ResolveKey(getEnvironmentVariable, secretsFilePath, fileExists, readAllLines);
		if (key is null)
			throw new InvalidOperationException($"ABCpdf license key is not configured. Please set the '{EnvVarName}' environment variable.");

		if (!XSettings.InstallLicense(key, throwOnFailure: false))
			throw new InvalidOperationException("ABCpdf license is invalid. Please verify the configured license key.");

		Console.WriteLine($"ABCpdf license installed: {XSettings.LicenseDescription} (licensed to {XSettings.Licensee})");
	}
}
