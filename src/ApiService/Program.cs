// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.Text;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// The Data Estate Health API service.
/// </summary>
public class Program
{
    /// <summary>
    /// Entry point for Data Estate Health API service.
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        string appSettingsJson = Environment.GetEnvironmentVariable("APP_SETTINGS_JSON");
        if (appSettingsJson != null)
        {
            builder.Configuration.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettingsJson)));
        }

        builder.Services.AddOptions()
            .Configure<SampleConfiguration>(builder.Configuration.GetSection("environment"));

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
