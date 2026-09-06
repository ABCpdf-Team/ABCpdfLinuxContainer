using WebSupergoo.ABCpdf14;

namespace ABCpdfLinuxContainer.Tests;

// Exercises real environment variables and a real temp file rather than injected fakes, so this
// mutates process-wide state (Environment.SetEnvironmentVariable) - see AssemblyInfo.cs for why
// test parallelization is disabled for this assembly.
public class ABCpdfLicenseInstallerTests : IDisposable
{
	readonly string? _originalEnvValue = Environment.GetEnvironmentVariable(ABCpdfLicenseInstaller.EnvVarName);
	readonly string _secretsFilePath = Path.GetTempFileName();

	public void Dispose()
	{
		Environment.SetEnvironmentVariable(ABCpdfLicenseInstaller.EnvVarName, _originalEnvValue);
		File.Delete(_secretsFilePath);
	}

	public static IEnumerable<object?[]> ResolutionScenarios()
	{
		yield return ["from-env", $"{ABCpdfLicenseInstaller.EnvVarName}=from-secrets", "from-env"];
		yield return ["", $"{ABCpdfLicenseInstaller.EnvVarName}=from-secrets", "from-secrets"];
		yield return [null, $"{ABCpdfLicenseInstaller.EnvVarName}=\"from-secrets\"", "from-secrets"];
		yield return [null, "SOME_OTHER_KEY=value", null];
		yield return [null, null, null];
	}

	[Theory]
	[MemberData(nameof(ResolutionScenarios))]
	public void ResolveKey_reflects_the_real_environment_variable_and_secrets_file(string? envValue, string? secretsFileContent, string? expected)
	{
		Environment.SetEnvironmentVariable(ABCpdfLicenseInstaller.EnvVarName, envValue);
		if (secretsFileContent is null)
			File.Delete(_secretsFilePath);
		else
			File.WriteAllText(_secretsFilePath, secretsFileContent);

		var result = ABCpdfLicenseInstaller.ResolveKey(Environment.GetEnvironmentVariable, _secretsFilePath, File.Exists, File.ReadAllLines);

		Assert.Equal(expected, result);
	}

	[Fact]
	public void ResolveKey_ignores_the_secrets_file_when_path_is_null()
	{
		Environment.SetEnvironmentVariable(ABCpdfLicenseInstaller.EnvVarName, null);
		File.WriteAllText(_secretsFilePath, $"{ABCpdfLicenseInstaller.EnvVarName}=from-secrets");

		var result = ABCpdfLicenseInstaller.ResolveKey(Environment.GetEnvironmentVariable, null, File.Exists, File.ReadAllLines);

		Assert.Null(result);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("not-a-real-license-key")]
	public void InstallAndValidate_throws_for_an_unconfigured_or_invalid_key(string? envValue)
	{
		Environment.SetEnvironmentVariable(ABCpdfLicenseInstaller.EnvVarName, envValue);

		Assert.Throws<InvalidOperationException>(() =>
			ABCpdfLicenseInstaller.InstallAndValidate(Environment.GetEnvironmentVariable, null, File.Exists, File.ReadAllLines));
	}

	[Fact]
	public void InstallLicense_throws_for_an_invalid_key_when_throwOnFailure_defaults_true()
	{
		// Documents a real ABCpdf quirk: the single-argument overload throws a raw System.Exception
		// rather than returning false, which is why ABCpdfLicenseInstaller always passes throwOnFailure: false.
		Assert.Throws<Exception>(() => XSettings.InstallLicense("not-a-real-license-key"));
	}
}
