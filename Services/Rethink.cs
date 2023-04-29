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
            await ConnectAsync();
        }
        public Connection.Builder GetConnectionBuilder(){
            string? host = Environment.GetEnvironmentVariable("RETHINKDB_HOST") ?? "127.0.0.1";
            string? user = Environment.GetEnvironmentVariable("RETHINKDB_USER") ?? "admin";
            string? password = Environment.GetEnvironmentVariable("RETHINKDB_PASSWORD") ?? "";
            int port = int.Parse(Environment.GetEnvironmentVariable("RETHINKDB_PORT") ?? "28015");
            return R.Connection()
                .Hostname(host) 
                .Port(port) 
                .User(user, password)
                .Timeout(60);
        }
        public async Task ConnectAsync(){
            connection = await GetConnectionBuilder()
                .ConnectAsync();
            Log.Information("RethinkDB is connected");
        }
        public void ConnectSync(){
            connection = GetConnectionBuilder()
                .Connect();
            Log.Information("RethinkDB is connected");
        }
        public RethinkQueryable<T> Linq<T>(string db,string table){
            if(connection is null || !connection.Open){
                ConnectSync();
            }
            return R.Db(db).Table<T>(table,connection);
        }
        public Rethink Begin(out RethinkDB r){
            r = R;
            return this;
        }
        public async Task<dynamic> End(ReqlExpr e){
            if(connection is null || !connection.Open){
                await ConnectAsync();
            }
            return await e.RunAsync(connection);
        }
        public async Task<dynamic> End<T>(ReqlExpr e){
            if(connection is null || !connection.Open){
                await ConnectAsync();
            }
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