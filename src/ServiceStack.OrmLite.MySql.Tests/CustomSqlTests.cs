﻿using System.Data;
using NUnit.Framework;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.MySql.Tests
{
    public class LetterFrequency
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Letter { get; set; }
    }
 
    [TestFixture]
    public class CustomSqlTests : OrmLiteTestBase
    {
        string spSql = @"DROP PROCEDURE IF EXISTS spSearchLetters;
            CREATE PROCEDURE spSearchLetters (IN pLetter varchar(10), OUT pTotal int)
            BEGIN
                SELECT COUNT(*) FROM LetterFrequency WHERE Letter = pLetter INTO pTotal;
                SELECT * FROM LetterFrequency WHERE Letter = pLetter;
            END";

        [Test]
        public void Can_execute_stored_procedure_using_SqlList_with_out_params()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<LetterFrequency>();

                var rows = "A,B,B,C,C,C,D,D,E".Split(',').Map(x => new LetterFrequency { Letter = x });
                db.InsertAll(rows);

                db.ExecuteSql(spSql);

                IDbDataParameter pTotal = null;
                var results = db.SqlList<LetterFrequency>("spSearchLetters",
                    cmd =>
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.AddParam("pLetter", "C");
                        pTotal = cmd.AddParam("pTotal", direction: ParameterDirection.Output);
                    });

                Assert.That(results.Count, Is.EqualTo(3));
                Assert.That(pTotal.Value, Is.EqualTo(3));
            }
        }

        [Test]
        public void Can_execute_stored_procedure_using_SqlProc_with_out_params()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<LetterFrequency>();

                var rows = "A,B,B,C,C,C,D,D,E".Split(',').Map(x => new LetterFrequency { Letter = x });
                db.InsertAll(rows);

                db.ExecuteSql(spSql);

                var cmd = db.SqlProc("spSearchLetters", new { pLetter = "C" });
                var pTotal = cmd.AddParam("pTotal", direction: ParameterDirection.Output);
                var results = cmd.ConvertToList<LetterFrequency>();

                Assert.That(results.Count, Is.EqualTo(3));
                Assert.That(pTotal.Value, Is.EqualTo(3));
            }
        }
    }
}