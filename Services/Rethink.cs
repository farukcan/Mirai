using RethinkDb.Driver;
using RethinkDb.Driver.Ast;
using RethinkDb.Driver.Linq;
using RethinkDb.Driver.Net;
using Log = Serilog.Log;

namespace Mirai.Services
{
    public class Rethink : IHostedService, IDisposable
    {
        RethinkDB R = RethinkDb.Driver.RethinkDB.R;
        Connection? connection;
        public static Rethink? Instance;
        public Rethink(){
            Log.Information("RethinkDB is instantiated");
            Instance = this;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            string? host = Environment.GetEnvironmentVariable("RETHINKDB_HOST") ?? "127.0.0.1";
            int port = int.Parse(Environment.GetEnvironmentVariable("RETHINKDB_PORT") ?? "28015");
            connection = await R.Connection()
                .Hostname(host) 
                .Port(port) 
                .Timeout(60)
                .ConnectAsync();
            Log.Information("RethinkDB is connected");
        }
        public RethinkQueryable<T> Linq<T>(string db,string table){
            return R.Db(db).Table<T>(table,connection);
        }
        public Rethink Begin(out RethinkDB r){
            r = R;
            return this;
        }
        public async Task<dynamic> End(ReqlExpr e){
            return await e.RunAsync(connection);
        }
        public async Task<dynamic> End<T>(ReqlExpr e){
            return await e.RunAsync<T>(connection);
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            connection?.Close();
            await Task.Yield();
        }

        public void Dispose()
        {
            connection?.Dispose();
        }
    }
}