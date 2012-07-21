﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace ServiceStack.OrmLite
{
    public static class ReadConnectionExtensions
    {
        [ThreadStatic]
        internal static string LastCommandText;

        public static T Exec<T>(this IDbConnection dbConn, Func<IDbCommand, T> filter)
        {
            var holdProvider = OrmLiteConfig.TSDialectProvider;
            try
            {
                var ormLiteDbConn = dbConn as OrmLiteConnection;
                if (ormLiteDbConn != null)
                    OrmLiteConfig.TSDialectProvider = ormLiteDbConn.Factory.DialectProvider;

                using (var dbCmd = dbConn.CreateCommand())
                {
                    dbCmd.Transaction = OrmLiteConfig.CurrentTransaction;

                    var ret = filter(dbCmd);
                    LastCommandText = dbCmd.CommandText;
                    return ret;
                }
            }
            finally
            {
                OrmLiteConfig.TSDialectProvider = holdProvider;
            }
        }

        public static void Exec(this IDbConnection dbConn, Action<IDbCommand> filter)
        {
            var dialectProvider = OrmLiteConfig.DialectProvider;
            try
            {
                var ormLiteDbConn = dbConn as OrmLiteConnection;
                if (ormLiteDbConn != null)
                    OrmLiteConfig.DialectProvider = ormLiteDbConn.Factory.DialectProvider;

                using (var dbCmd = dbConn.CreateCommand())
                {
                    dbCmd.Transaction = OrmLiteConfig.CurrentTransaction;

                    filter(dbCmd);
                    LastCommandText = dbCmd.CommandText;
                }
            }
            finally
            {
                OrmLiteConfig.DialectProvider = dialectProvider;
            }
        }

        public static IEnumerable<T> ExecLazy<T>(this IDbConnection dbConn, Func<IDbCommand, IEnumerable<T>> filter)
        {
            var dialectProvider = OrmLiteConfig.DialectProvider;
            try
            {
                var ormLiteDbConn = dbConn as OrmLiteConnection;
                if (ormLiteDbConn != null)
                    OrmLiteConfig.DialectProvider = ormLiteDbConn.Factory.DialectProvider;

                using (var dbCmd = dbConn.CreateCommand())
                {
                    dbCmd.Transaction = OrmLiteConfig.CurrentTransaction;

                    var results = filter(dbCmd);
                    LastCommandText = dbCmd.CommandText;
                    foreach (var item in results)
                    {
                        yield return item;
                    }
                }
            }
            finally
            {
                OrmLiteConfig.DialectProvider = dialectProvider;
            }
        }

        public static IDbTransaction OpenTransaction(this IDbConnection dbConn)
        {
            return new OrmLiteTransaction(dbConn.BeginTransaction());
        }

        public static IDbTransaction OpenTransaction(this IDbConnection dbConn, IsolationLevel isolationLevel)
        {
            return new OrmLiteTransaction(dbConn.BeginTransaction(isolationLevel));
        }

        public static IOrmLiteDialectProvider GetDialectProvider(this IDbConnection dbConn)
        {
            var ormLiteDbConn = dbConn as OrmLiteConnection;
            return ormLiteDbConn != null 
                ? ormLiteDbConn.Factory.DialectProvider 
                : OrmLiteConfig.DialectProvider;
        }

        public static SqlExpressionVisitor<T> CreateExpression<T>()
        {
            return OrmLiteConfig.DialectProvider.ExpressionVisitor<T>();
        }

        public static List<T> Select<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate)
            where T : new()
        {
            return dbConn.Exec(dbCmd => dbCmd.Select(predicate));
        }

        public static List<T> Select<T>(this IDbConnection dbConn, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> expression)
            where T : new()
        {
            return dbConn.Exec(dbCmd => dbCmd.Select(expression));
        }

        public static List<T> Select<T>(this IDbConnection dbConn, SqlExpressionVisitor<T> expression)
            where T : new()
        {
            return dbConn.Exec(dbCmd => dbCmd.Select(expression));
        }

        public static T First<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate)
            where T : new()
        {
            return dbConn.Exec(dbCmd => dbCmd.First(predicate));
        }

        public static T First<T>(this IDbConnection dbConn, SqlExpressionVisitor<T> expression)
            where T : new()
        {
            return dbConn.Exec(dbCmd => dbCmd.First(expression));
        }

        public static T FirstOrDefault<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate)
            where T : new()
        {
            return dbConn.Exec(dbCmd => dbCmd.FirstOrDefault(predicate));
        }

        public static T FirstOrDefault<T>(this IDbConnection dbConn, SqlExpressionVisitor<T> expression)
            where T : new()
        {
            return dbConn.Exec(dbCmd => dbCmd.FirstOrDefault(expression));
        }

        public static TKey GetScalar<T, TKey>(this IDbConnection dbConn, Expression<Func<T, TKey>> field)
            where T : new()
        {
            return dbConn.Exec(dbCmd => dbCmd.GetScalar(field));
        }

        public static TKey GetScalar<T, TKey>(this IDbConnection dbConn, Expression<Func<T, TKey>> field,
                                             Expression<Func<T, bool>> predicate)
            where T : new()
        {
            return dbConn.Exec(dbCmd => dbCmd.GetScalar(field, predicate));
        }
    }
}