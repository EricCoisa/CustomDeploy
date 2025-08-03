namespace CustomDeploy.Models
{
    /// <summary>
    /// Request para criar um novo site IIS
    /// </summary>
    public class CreateSiteRequest
    {
        public string SiteName { get; set; } = string.Empty;
        public string BindingInformation { get; set; } = string.Empty;
        public string PhysicalPath { get; set; } = string.Empty;
        public string AppPoolName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request para editar um site IIS existente
    /// </summary>
    public class UpdateSiteRequest
    {
        public string? PhysicalPath { get; set; }
        public List<SiteBinding>? Bindings { get; set; }
    }

    /// <summary>
    /// Request para criar uma aplicação em um site
    /// </summary>
    public class CreateApplicationRequest
    {
        public string SiteName { get; set; } = string.Empty;
        public string AppPath { get; set; } = string.Empty;
        public string PhysicalPath { get; set; } = string.Empty;
        public string AppPoolName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request para editar uma aplicação existente
    /// </summary>
    public class UpdateApplicationRequest
    {
        public string? PhysicalPath { get; set; }
        public string? AppPoolName { get; set; }
    }

    /// <summary>
    /// Request para criar um Application Pool
    /// </summary>
    public class CreateAppPoolRequest
    {
        public string PoolName { get; set; } = string.Empty;
        public string RuntimeVersion { get; set; } = "v4.0";
        public string PipelineMode { get; set; } = "Integrated"; // Classic ou Integrated
    }

    /// <summary>
    /// Request para editar um Application Pool
    /// </summary>
    public class UpdateAppPoolRequest
    {
        public string? RuntimeVersion { get; set; }
        public string? PipelineMode { get; set; }
        public bool? Enable32BitAppOnWin64 { get; set; }
        public int? IdleTimeout { get; set; }
    }

    /// <summary>
    /// Informações de binding de um site
    /// </summary>
    public class SiteBinding
    {
        public string Protocol { get; set; } = "http";
        public string IpAddress { get; set; } = "*";
        public int Port { get; set; } = 80;
        public string HostName { get; set; } = string.Empty;
        public string BindingInformation => $"{IpAddress}:{Port}:{HostName}";
    }

    /// <summary>
    /// Informações detalhadas de um site IIS
    /// </summary>
    public class DetailedSiteInfo
    {
        public string Name { get; set; } = string.Empty;
        public int Id { get; set; }
        public string State { get; set; } = string.Empty;
        public string PhysicalPath { get; set; } = string.Empty;
        public List<SiteBinding> Bindings { get; set; } = new();
        public List<ApplicationInfo> Applications { get; set; } = new();
        public string AppPoolName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Informações detalhadas de um Application Pool
    /// </summary>
    public class AppPoolInfo
    {
        public string Name { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string RuntimeVersion { get; set; } = string.Empty;
        public string PipelineMode { get; set; } = string.Empty;
        public bool Enable32BitAppOnWin64 { get; set; }
        public int IdleTimeout { get; set; }
        public List<string> AssociatedSites { get; set; } = new();
        public List<string> AssociatedApplications { get; set; } = new();
    }

    /// <summary>
    /// Resultado de operação IIS
    /// </summary>
    public class IISOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Resultado de verificação de permissões IIS
    /// </summary>
    public class IISPermissionResult
    {
        public bool CanCreateFolders { get; set; }
        public bool CanMoveFiles { get; set; }
        public bool CanExecuteIISCommands { get; set; }
        public bool CanManageIIS { get; set; }
        public bool AllPermissionsGranted { get; set; }
        public List<string> TestDetails { get; set; } = new();
        public List<string> Instructions { get; set; } = new();
    }
}
