namespace ABCpdfLinuxContainer.Tests;

public class ABCpdfLicenseInstallerTests : IDisposable
{
	// A per-instance unique name so these tests can never collide with the real ABCPDF_LICENSE_KEY
	// or with each other, even running in parallel.
	readonly string _envVarName = $"TEST_LICENSE_KEY_{Guid.NewGuid():N}";
	readonly string _secretsFilePath = Path.GetTempFileName();

	public void Dispose()
	{
		Environment.SetEnvironmentVariable(_envVarName, null);
		File.Delete(_secretsFilePath);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Install_throws_not_configured_when_env_var_is_missing_or_blank(string? envValue)
	{
		Environment.SetEnvironmentVariable(_envVarName, envValue);

		var ex = Assert.Throws<InvalidOperationException>(() => ABCpdfLicenseInstaller.Install(_envVarName));

		Assert.Contains("not configured", ex.Message);
	}

	[Fact]
	public void Install_throws_not_configured_when_secrets_file_lacks_the_key()
	{
		File.WriteAllText(_secretsFilePath, "SOME_OTHER_KEY=value");

		var ex = Assert.Throws<InvalidOperationException>(() => ABCpdfLicenseInstaller.Install(_envVarName, _secretsFilePath));

		Assert.Contains("not configured", ex.Message);
	}

	[Fact]
	public void Install_ignores_the_secrets_file_when_no_path_is_given()
	{
		File.WriteAllText(_secretsFilePath, $"{_envVarName}=not-a-real-license-key");

		var ex = Assert.Throws<InvalidOperationException>(() => ABCpdfLicenseInstaller.Install(_envVarName));

		Assert.Contains("not configured", ex.Message);
	}

	[Fact]
	public void Install_reads_the_key_from_the_secrets_file_when_the_env_var_is_unset()
	{
		File.WriteAllText(_secretsFilePath, $"{_envVarName}=not-a-real-license-key");

		var ex = Assert.Throws<InvalidOperationException>(() => ABCpdfLicenseInstaller.Install(_envVarName, _secretsFilePath));

		Assert.Contains("invalid", ex.Message);
	}

	[Fact]
	public void Install_throws_invalid_for_a_rejected_env_var_key()
	{
		Environment.SetEnvironmentVariable(_envVarName, "not-a-real-license-key");

		var ex = Assert.Throws<InvalidOperationException>(() => ABCpdfLicenseInstaller.Install(_envVarName));

		Assert.Contains("invalid", ex.Message);
	}

	[Fact]
	public void InstallLicense_throws_for_an_invalid_key_when_throwOnFailure_defaults_true()
	{
		// Documents a real ABCpdf quirk: the single-argument overload throws a raw System.Exception
		// rather than returning false, which is why ABCpdfLicenseInstaller always passes throwOnFailure: false.
		Assert.Throws<Exception>(() => XSettings.InstallLicense("not-a-real-license-key"));
	}
}
