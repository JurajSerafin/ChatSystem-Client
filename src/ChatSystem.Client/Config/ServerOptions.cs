using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatSystem.Client.Config;

/// <summary>
/// Server connection settings loaded from appsettings.json.
/// </summary>
public sealed class ServerOptions {
    public const string SectionName = "Server";

    public string BaseUrl { get; set; } = "http://localhost:8080";


    public int TimeoutSeconds { get; set; } = 30;
}