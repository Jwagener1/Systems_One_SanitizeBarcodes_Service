namespace Systems_One_SanitizeBarcodes_Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IDbQueryExecutor _executor;
        private readonly QueryExecutionOptions _queryOptions;
        private readonly IHostEnvironment _env;

        public Worker(ILogger<Worker> logger,
            IDbQueryExecutor executor,
            Microsoft.Extensions.Options.IOptions<QueryExecutionOptions> queryOptions,
            IHostEnvironment env)
        {
            _logger = logger;
            _executor = executor;
            _queryOptions = queryOptions.Value;
            _env = env;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            TimeSpan delay = TimeSpan.FromMinutes(Math.Max(1, _queryOptions.IntervalMinutes));
            string queryFilePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, _queryOptions.QueryFile));

            _logger.LogInformation("Worker started. Interval: {interval} minutes. Query file: {file}", delay.TotalMinutes, queryFilePath);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _executor.ExecuteQueryFileAsync(queryFilePath, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error during query execution loop");
                }

                await Task.Delay(delay, stoppingToken);
            }

            _logger.LogInformation("Worker stopping.");
        }
    }
}
