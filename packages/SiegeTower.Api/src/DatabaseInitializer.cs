using Npgsql;

namespace SiegeTower.Api;

public static class DatabaseInitializer
{
	const string DatabaseName = "siegetower";

	public static NpgsqlConnection CreateApplicationConnection(IConfiguration configuration)
	{
		var connectionString = new NpgsqlConnectionStringBuilder
		{
			Host = configuration["Database:Host"] ?? "localhost",
			Port = int.Parse(configuration["Database:Port"] ?? "5432"),
			Username = configuration["Database:Username"] ?? "siegetower",
			Password = configuration["Database:Password"] ?? "siegetower",
			Database = DatabaseName
		}.ConnectionString;

		return new NpgsqlConnection(connectionString);
	}

	public static async Task InitializeAsync(IConfiguration configuration, CancellationToken cancellationToken = default)
	{
		var host = configuration["Database:Host"] ?? "localhost";
		var port = configuration["Database:Port"] ?? "5432";
		var username = configuration["Database:Username"] ?? "siegetower";
		var password = configuration["Database:Password"] ?? "siegetower";

		for (var attempt = 1; ; attempt++)
		{
			try
			{
				await EnsureDatabaseAsync(host, port, username, password, cancellationToken);
				return;
			}
			catch (NpgsqlException) when (attempt < 30)
			{
				await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
			}
		}
	}

	static async Task EnsureDatabaseAsync(
		string host,
		string port,
		string username,
		string password,
		CancellationToken cancellationToken)
	{
		var maintenanceConnectionString = new NpgsqlConnectionStringBuilder
		{
			Host = host,
			Port = int.Parse(port),
			Username = username,
			Password = password,
			Database = "postgres"
		}.ConnectionString;

		await using (var maintenanceConnection = new NpgsqlConnection(maintenanceConnectionString))
		{
			await maintenanceConnection.OpenAsync(cancellationToken);
			await using var command = new NpgsqlCommand(
				"SELECT 1 FROM pg_database WHERE datname = @databaseName",
				maintenanceConnection);
			command.Parameters.AddWithValue("databaseName", DatabaseName);

			if (await command.ExecuteScalarAsync(cancellationToken) is null)
			{
				await using var createDatabase = new NpgsqlCommand(
					$"CREATE DATABASE {QuoteIdentifier(DatabaseName)}",
					maintenanceConnection);
				await createDatabase.ExecuteNonQueryAsync(cancellationToken);
			}
		}

		var applicationConnectionString = new NpgsqlConnectionStringBuilder(maintenanceConnectionString)
		{
			Database = DatabaseName
		}.ConnectionString;

		await using var connection = new NpgsqlConnection(applicationConnectionString);
		await connection.OpenAsync(cancellationToken);

		if (!await IsUpToDateAsync(connection, cancellationToken))
		{
			await ForceUpgradeAsync(connection, cancellationToken);
		}
	}

	static async Task<bool> IsUpToDateAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
	{
		await using var tableCommand = new NpgsqlCommand(
			"""
			SELECT EXISTS (
				SELECT 1
				FROM information_schema.tables
				WHERE table_schema = 'public' AND table_name = 'release'
			)
			""",
			connection);

		if ((bool)(await tableCommand.ExecuteScalarAsync(cancellationToken) ?? false) is false)
		{
			return false;
		}

		await using var releaseCommand = new NpgsqlCommand(
			"""
			SELECT EXISTS (
				SELECT 1
				FROM release
				WHERE version = @version
				  AND version_major = @majorVersion
				  AND version_minor = @minorVersion
				  AND version_patch = @patchVersion
			)
			""",
			connection);
		releaseCommand.Parameters.AddWithValue("version", Releases.Current.Version);
		releaseCommand.Parameters.AddWithValue("majorVersion", (long)Releases.Current.MajorVersion);
		releaseCommand.Parameters.AddWithValue("minorVersion", (long)Releases.Current.MinorVersion);
		releaseCommand.Parameters.AddWithValue("patchVersion", (long)Releases.Current.PatchVersion);

		return (bool)(await releaseCommand.ExecuteScalarAsync(cancellationToken) ?? false);
	}

	static async Task ForceUpgradeAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
	{
		await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
		await using var command = new NpgsqlCommand(
			"""
			DROP TABLE IF EXISTS task;
			DROP TABLE IF EXISTS release;

			CREATE TABLE release (
				version TEXT PRIMARY KEY,
				version_major BIGINT NOT NULL,
				version_minor BIGINT NOT NULL,
				version_patch BIGINT NOT NULL
			);

			INSERT INTO release (version, version_major, version_minor, version_patch)
			VALUES (@version, @majorVersion, @minorVersion, @patchVersion);

			CREATE TABLE task (
				id UUID PRIMARY KEY,
				name TEXT NOT NULL,
				description TEXT NOT NULL
			);
			""",
			connection,
			transaction);
		command.Parameters.AddWithValue("version", Releases.Current.Version);
		command.Parameters.AddWithValue("majorVersion", (long)Releases.Current.MajorVersion);
		command.Parameters.AddWithValue("minorVersion", (long)Releases.Current.MinorVersion);
		command.Parameters.AddWithValue("patchVersion", (long)Releases.Current.PatchVersion);
		await command.ExecuteNonQueryAsync(cancellationToken);
		await transaction.CommitAsync(cancellationToken);
	}

	static string QuoteIdentifier(string identifier) => $"\"{identifier.Replace("\"", "\"\"")}\"";
}
