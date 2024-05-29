using DataModel.Repository;

public static class ApplicationBuildExtensions
{

    public static string DefineDbConnection(this IConfiguration config)
    {
        string dbConnectionString = config["DB_CONNECTION"];//Environment.GetEnvironmentVariable("DB_CONNECTION");

        string dbConnectionString2 = config.GetConnectionString("DefaultConnection");
        Console.WriteLine("appsettings AbsanteeDatabase: " + dbConnectionString2);
        if (string.IsNullOrWhiteSpace(dbConnectionString))
        {
            dbConnectionString = dbConnectionString2;
        }
        if (string.IsNullOrWhiteSpace(dbConnectionString))
        {
            throw new ArgumentException("Environment variable DB_CONNECTION or appsettings' property AbsanteeDatabase cannot be null or empty");
        }

        return dbConnectionString;
    }

    public static RabbitMqConfiguration DefineRabbitMqConfiguration(this IConfiguration config)
    {
        var rabbitMqConfig = getRabbitMqConfigFromEnvironmentVariablesEnvironment(config);
        if (rabbitMqConfig == null)
        {
            rabbitMqConfig = config.GetSection("RabbitMq").Get<RabbitMqConfiguration>();
        }
        if (rabbitMqConfig == null)
        {
            throw new ArgumentException("Environment variables RABBITMQ_HOSTNAME, RABBITMQ_USERNAME, RABBITMQ_PASSWORD or appsettings' property RabbitMq cannot be null or empty");
        }

        return rabbitMqConfig;
    }

    private static RabbitMqConfiguration getRabbitMqConfigFromEnvironmentVariablesEnvironment(IConfiguration config)
    {
        string rabbitMqHostname = config["RABBITMQ_HOSTNAME"];
        Console.WriteLine("RABBITMQ_HOSTNAME by config: " + rabbitMqHostname);

        string rabbitMqUsername = config["RABBITMQ_USERNAME"];
        Console.WriteLine("RABBITMQ_USERNAME by config: " + rabbitMqUsername);

        string rabbitMqPassword = config["RABBITMQ_PASSWORD"];
        Console.WriteLine("RABBITMQ_PASSWORD by config: " + rabbitMqPassword);

        string rabbitMqPort = config["RABBITMQ_PORT"] ?? config["RABBITMQ_PORT"];
        Console.WriteLine("RABBITMQ_PORT by config: " + rabbitMqPort);

        if (string.IsNullOrWhiteSpace(rabbitMqHostname) ||
            string.IsNullOrWhiteSpace(rabbitMqUsername) ||
            string.IsNullOrWhiteSpace(rabbitMqPassword) ||
            string.IsNullOrWhiteSpace(rabbitMqPort)) 
            return null;
        else
            return new RabbitMqConfiguration
            {
                Hostname = rabbitMqHostname,
                Username = rabbitMqUsername,
                Password = rabbitMqPassword,
                Port = int.Parse(rabbitMqPort.ToString())
            };
    }
}