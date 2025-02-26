public partial class Program {
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddScoped<ITransactionWriteService, TransactionWriteService>();
        builder.Services.AddScoped<ITransactionReadService, TransactionReadService>();
        builder.Services.AddSingleton<ITransactionRepository, TransactionRepository>();
        var app = builder.Build();
        app.MapControllers();
        app.Run();
    }
}
