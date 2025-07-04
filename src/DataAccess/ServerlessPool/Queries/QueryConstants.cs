﻿// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Models;

internal class QueryConstants
{
    // Filter value sent by the API user to filter on empty values
    public static readonly string NoneFilter = "{None}";

    // Filter value sent by the API user to filter on non-empty values
    public static readonly string AppliedFilter = "{Applied}";

    // Delimiter for data columns which contain multiple entries
    public static readonly string PipeDelimiter = "|";

    public static readonly string WhereClause = "WHERE ";

    public static readonly string AndClause = "AND ";

    public static class ServerlessQuery
    {
        public const string DeltaFormat = "delta";

        public const string ParquetFormat = "PARQUET";

        public const string AsRows = " AS ROWS ";

        public static string OpenRowSet(string path, string format)
        {
            return $" FROM OPENROWSET(BULK '{path}', FORMAT = '{format}') ";
        }

        public static string FromTable(string databaseName, string tableName)
        {
            return $" FROM [{databaseName}].[{tableName}] ";
        }
    }

    public enum SQLOperator
    {
        Equal,
        LikeWithPipe,
        Like,
        In,
        NotEqual,
        Greater,
        Less,
        GreaterOrEqual,
        LessOrEqual,
        Is
    }

    public class DataEstateHealthSummaryColumnNamesForKey
    {
        public static readonly string BusinessDomainId = "BusinessDomainId";
    }

    public class HealthActionColumnNamesForKey
    {
        public static readonly string BusinessDomainId = "BusinessDomainId";
    }

    public class HealthScoresColumnNamesForKey
    {
        public static readonly string BusinessDomainId = "BusinessDomainId";
    }

    public class HealthTrendsColumnNamesForKey
    {
        public static readonly string OpenActions = "TotalOpenActionsCount";
        public static readonly string BusinessDomainCount = "BusinessDomainCount";
        public static readonly string DataProductCount = "DataProductCount";
        public static readonly string DataAssetCount = "AssetCount";
        public static readonly string LastRefreshedAt = "LastRefreshedAt";
        public static readonly string BusinessDomainId = "BusinessDomainId";
    }

    public static readonly Dictionary<TrendKind, string> HealthTrendKindToColumnName = new Dictionary<TrendKind, string>() {
        { TrendKind.OpenActions, HealthTrendsColumnNamesForKey.OpenActions },
        { TrendKind.BusinessDomainCount, HealthTrendsColumnNamesForKey.BusinessDomainCount },
        { TrendKind.DataProductCount, HealthTrendsColumnNamesForKey.DataProductCount },
        { TrendKind.DataAssetCount, HealthTrendsColumnNamesForKey.DataAssetCount },
    };
}
