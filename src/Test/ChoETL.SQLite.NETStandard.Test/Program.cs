﻿using System;
using System.Diagnostics;
using System.Linq;

namespace ChoETL.SQLite.NETStandard.Test
{
    class Program
    {
        public class Emp
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string City { get; set; }
        }

        static void Main(string[] args)
        {
            ChoETLFrxBootstrap.TraceLevel = System.Diagnostics.TraceLevel.Off;
            StageLargeFile();
        }

        public class USDBitCoin
        {
            public int Id { get; set; }
            public double Price { get; set; }
            public double Qty { get; set; }
        }

        static void StageLargeFile()
        {
            //ChoTypeDescriptor.DoNotUseTypeConverterForTypes = true;
            for (int i = 2; i < 3; i++)
            {
                var watch = Stopwatch.StartNew();
                using (var r = new ChoCSVReader(@"C:\Users\nraj39\Downloads\XBTUSD.csv")
                    .Configure(c => c.NotifyAfter = 100000)
                    .Setup(s => s.RowsLoaded += (o, e) =>
                    {
                        $"Rows Loaded: {e.RowsLoaded} <-- {DateTime.Now}".Print();
                    })
                    .Configure(c => c.FastCSVParsing = true)
                    )
                {
                    //r.Take(1).Print();
                    //return;
                    //r.Take(1000000).Count().Print();
                    //return;
                    r.Take(1000000).StageOnSQLite(new ChoETLSqliteSettings()
                        .Configure(c => c.ConnectionString = "DataSource=local.db;Version=3;Synchronous=OFF;Journal Mode=OFF")
                        .Configure(c => c.NotifyAfter = 500000)
                        .Configure(c => c.BatchSize = 500000)
                        .Configure(c => c.RowsUploaded += (o, e) =>
                        {
                            Console.WriteLine($"Rows uploaded: {e.RowsUploaded}");
                        }));
                }
                watch.Stop();
                watch.Elapsed.Print();
            }
        }
        static void StageJSONFile()
        {
            string json = @"
    [
        {
            ""Id"": 1,
            ""Name"": ""Polo"",
            ""City"": ""New York""
        },
        {
            ""Id"": 2,
            ""Name"": ""328"",
            ""City"": ""Edison""
        }
    ]";
            ChoETLFrxBootstrap.TraceLevel = TraceLevel.Error;
            using (var r = ChoJSONReader<Emp>.LoadText(json)
                )
            {
                r.StageOnSQLite().Where(e => e.Id == 2).Print();
            }

        }

        static void StageCSVFile()
        {
            string csv = @"Id, Name, City
1, Tom, NY
2, Mark, NJ
3, Lou, FL
4, Smith, PA
5, Raj, DC
";

            using (var r = ChoCSVReader<Emp>.LoadText(csv)
                .WithFirstLineHeader())
            {
                r.StageOnSQLite();
            }

        }
    }
}
