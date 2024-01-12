// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Data.DeltaLake.Types;
using Microsoft.Purview.DataEstateHealth.Core;
using Microsoft.Purview.DataGovernance.DeltaWriter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

internal class DeltaTableEventProcessor : IDeltaTableEventProcessor
{
    private const string SourcePathFragment = "Source";
    private const string SinkPathFragment = "Sink";
    private const string DeletedPathFragment = "_Deleted";

    private const string Tag = nameof(DeltaTableEventProcessor);

    private readonly IDataEstateHealthRequestLogger logger;

    public DeltaTableEventProcessor(IDataEstateHealthRequestLogger logger)
    {
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task PersistToStorage<T>(Dictionary<EventOperationType, List<T>> models, IDeltaLakeOperator deltaTableWriter, string prefix, bool isSourceEvent = true)
        where T : BaseEventHubEntityModel
    {
        if (!TryGetFirstEntityModel(models, out T model))
        {
            this.logger.LogTrace($"{Tag}|No entity models found to persist.");
            return;
        }

        List<T> mergedCreateEvents = MergeEventsByTypes(models, EventOperationType.Create, EventOperationType.Update, EventOperationType.Upsert).ToList();

        await this.WriteEvents(deltaTableWriter, mergedCreateEvents, GetTableRelativePath(isSourceEvent, prefix, model, includeDeletedPath: false));

        if (models.TryGetValue(EventOperationType.Delete, out List<T> deleteRows))
        {
            await this.WriteEvents(deltaTableWriter, deleteRows, GetTableRelativePath(isSourceEvent, prefix, model, includeDeletedPath: true));
        }
    }

    private async Task WriteEvents<T>(IDeltaLakeOperator deltaTableWriter, IList<T> events, string tableRelativePath)
        where T : BaseEventHubEntityModel
    {
        if (deltaTableWriter == null)
        {
            throw new ArgumentNullException(nameof(deltaTableWriter));
        }

        if (string.IsNullOrEmpty(tableRelativePath))
        {
            throw new ArgumentException("TableRelativePath cannot be null or empty.", nameof(tableRelativePath));
        }

        if (events.Count > 0)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                PayloadKind payloadKind = events.First().GetPayloadKind();
                StructType schemaDefinition = events.First().GetSchemaDefinition();
                await deltaTableWriter.CreateOrAppendDataset(tableRelativePath, schemaDefinition, events);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"{Tag}|Failed to write {events.Count} events to {tableRelativePath}. Details: {ex.Message}", ex);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                this.logger.LogTrace($"{Tag}|Operation for {events.Count} events to {tableRelativePath} completed in {stopwatch.Elapsed.TotalSeconds}s.");
            }
        }
    }

    private static bool TryGetFirstEntityModel<T>(Dictionary<EventOperationType, List<T>> eventHubEntityModels, out T entityModel)
        where T : BaseEventHubEntityModel
    {
        entityModel = eventHubEntityModels.Values.FirstOrDefault()?.FirstOrDefault();
        return entityModel != null;
    }

    private static IEnumerable<T> MergeEventsByTypes<T>(Dictionary<EventOperationType, List<T>> eventHubEntityModels, params EventOperationType[] types)
        where T : BaseEventHubEntityModel
    {
        foreach (EventOperationType type in types)
        {
            if (eventHubEntityModels.TryGetValue(type, out List<T> rows))
            {
                foreach (T row in rows)
                {
                    yield return row;
                }
            }
        }
    }

    private static string GetTableRelativePath<T>(bool isSourceEvent, string prefix, T eventHubEntityModel, bool includeDeletedPath)
        where T : BaseEventHubEntityModel
    {
        string pathFragment = isSourceEvent ? SourcePathFragment : SinkPathFragment;
        string deletedFragment = includeDeletedPath ? $"/{DeletedPathFragment}" : string.Empty;

        return $"/{pathFragment}/{prefix}{deletedFragment}/{eventHubEntityModel.GetPayloadKind()}";
    }
}
