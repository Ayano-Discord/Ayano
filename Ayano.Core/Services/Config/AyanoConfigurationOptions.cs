// namespace Ayano.Services.Config;
//
// using System.Collections.Generic;
// using System.Reflection;
// using Serilog;
//
// /// <summary>
// ///     A class which holds configuration information bound from an AppSettings or UserSecrets file.
// ///     To be used with IOptions and configured in ConfigureServices in a Startup.cs file.<br />
// ///     <para>
// ///         More info about <b>Options Pattern</b> can be found on Microsoft Docs
// ///         <a href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0">Options pattern in ASP.NET Ayano.Core</a>
// ///     </para>
// ///     <br />
// ///     Properties within this class correlate to the name of the key or sub-key property in the configuration file:
// ///     Ex. Below, the "Persistence" key correlates to the Persistence property in this class with the same name
// ///     <code>
// /// {
// /// /* Root */
// /// "Ayano":
// /// {
// ///     /* Sub-Key property name */
// ///     "Persistence": {...}
// /// }
// /// }
// /// </code>
// /// </summary>
// public class AyanoConfigurationOptions
// {
// 	/// <summary>
// 	///     The name of the root configuration options property in the file
// 	/// </summary>
// 	public const string SectionKey = "Ayano";
//
// 	/// <summary>
// 	///     Property for controlling the level of Logging
// 	/// </summary>
// 	public string LogLevel { get; set; } = "Info";
//
// 	/// <summary>
// 	///     Property for determining whether to use global or guild specific slash commands.
// 	///     Leave null for global slash commands.
// 	/// </summary>
// 	public ulong? SlashCommandsGuildId { get; set; }
//
// 	/// <summary>
// 	///		Property for holding the DSN url for Sentry and it's related services.
// 	/// </summary>
// 	public string? SentryDSN { get; set; }
// 	
// 	/// <summary>
// 	/// Property for holding a Ravy API (https://ravy.org/api) key.
// 	/// </summary>
// 	public string? RavyAPIKey { get; set; }
// 	
// 	public AyanoRedisOptions Redis { get; set; } = new();
// 	
// 	/// <summary>
// 	///     Property for holding Persistence options (property name matching sub-key property in configuration file)
// 	/// </summary>
// 	public AyanoPersistenceOptions Persistence { get; set; } = new();
//
// 	/// <summary>
// 	///     Property for holding Discord Developer Api options (property name matching sub-key property in configuration file)
// 	/// </summary>
// 	public AyanoDiscordOptions Discord { get; set; } = new();
//
// 	/// <summary>
// 	///     Property for holding Discord Developer Api options (property name matching sub-key property in configuration file)
// 	/// </summary>
// 	public AyanoE621Options E621 { get; set; } = new();
// }
//
// /// <summary>
// /// Class which holds configuration information for Redis persistence.
// /// </summary>
// public class AyanoRedisOptions
// {
// 	/// <summary>
// 	/// The hostname of the Redis server. Defaults to localhost.
// 	/// </summary>
// 	public string Host { get; set; } = "localhost";
// 	
// 	/// <summary>
// 	/// The port of the Redis server. Defaults to 6379.
// 	/// </summary>
// 	public int Port { get; set; } = 6379;
// 	
// 	/// <summary>
// 	/// Optional password for the Redis server.
// 	/// </summary>
// 	public string? Password { get; set; }
// 	
// 	/// <summary>
// 	/// Optional database index for the Redis server.
// 	/// </summary>
// 	public int Database { get; set; }
// }
//
// /// <summary>
// ///     Class which holds configuration information for the Database Connection properties
// ///     <para>Note: Ayano by default uses PostgresSQL, so the class is templated based off connection string convention for PostgreSQL</para>
// ///     <para>
// ///         A pre-configured <b>docker-compose.yml</b> file can be found
// ///         <a href="https://files.velvetthepanda.dev/docker/postgres/docker-compose.yml">here</a>
// ///     </para>
// ///     <para>Default Username and Password: "Ayano".</para>
// /// </summary>
// public class AyanoPersistenceOptions
// {
//     public string Host     { get; set; } = "localhost";
//     public string Port     { get; set; } = "5432";
//     public string Database { get; set; } = string.Empty;
//     public string Username { get; set; } = "postgres";
//     public string Password { get; set; } = string.Empty;
//
//     /// <summary>
//     ///     Convenience method to compose the ConnectionString based on the provided Persistence options
//     ///     <see cref="AyanoPersistenceOptions" />
//     /// </summary>
//     /// <returns>The full connection string given the options, or invalid/incomplete connection string if pieces of configuration file are left blank</returns>
//     public string GetConnectionString() =>
//         $"Server={Host};"       +
//         $"Port={Port};"         +
//         $"Database={Database};" +
//         $"Username={Username};" +
//         $"Password={Password};" +
//         "Include Error Detail = true";
// }
//
// /// <summary>
// ///     Class which holds configuration information for the Discord Developer Api properties
// /// </summary>
// public class AyanoDiscordOptions
// {
//     public int    Shards       { get; set; } = 1;
//     public string ClientId     { get; set; } = string.Empty;
//     public string ClientSecret { get; set; } = string.Empty;
//     public string BotToken     { get; set; } = string.Empty;
// }
//
// /// <summary>
// ///     Class which holds configuration information for the E621 Api properties.
// ///     99% of content will be accessible without this, but <i>some</i> content may be on the public blacklist. Use at your own discretion.
// /// </summary>
// public class AyanoE621Options
// {
//     public string ApiKey   { get; set; } = string.Empty;
//     public string Username { get; set; } = string.Empty;
// }