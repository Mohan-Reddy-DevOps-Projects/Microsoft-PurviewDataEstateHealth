namespace DEH.Application.Backfill;

using Messaging;

public record KickOffCatalogBackfillCommand(
    List<string> AccountIds,
    int BatchAmount = 100,
    int BufferTimeInMinutes = 15) : ICommand<string>;
