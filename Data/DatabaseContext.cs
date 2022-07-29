using Dapper;
using Microsoft.Data.Sqlite;
using ROB.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROB.Data
{
    internal class DatabaseContext
    {
        private string ConnectionString;
        public DatabaseContext(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public void Init()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                try
                {
                    connection.Execute(@"
                        create table Quotes (
                            Id          integer primary key AUTOINCREMENT,
                            Command     varchar(25) not null,
                            Message     varchar(2000) not null,
                            CreatedOn   datetime not null,
                            CreatedBy   varchar(100) not null
                        )
                    ");
                }
                catch (Exception ex) {
                }
            }
        }

        public string GetRandomQuote(string command)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                var random = new Random();

                connection.Open();
                var quote = connection
                    .Query<Quote>($"select * from Quotes where Command = @command order by RANDOM() limit 1", new { command = command })
                    .FirstOrDefault();

                return quote.Message;
            }
        }

        public void AddQuote(string command, string message, string createdBy)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                connection.Execute(
                    "insert into Quotes (Command, Message, CreatedOn, CreatedBy) values (@command, @message, DATE('now'), @createdBy)",
                    new
                    {
                        command = command,
                        message = message,
                        createdBy = createdBy
                    });
            }
        }
    }
}
