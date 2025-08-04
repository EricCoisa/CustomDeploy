using CustomDeploy.Models;
using System.Security;

namespace CustomDeploy.Services
{
    /// <summary>
    /// Serviço para gerenciamento do sistema de arquivos
    /// </summary>
    public class FileManagerService : IFileManagerService
    {
        private readonly ILogger<FileManagerService> _logger;

        // Lista de caminhos bloqueados por segurança
        private readonly HashSet<string> _blockedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            @"C:\Windows\System32",
            @"C:\Windows\SysWOW64",
            @"C:\Windows\security",
            @"C:\Windows\Temp",
            @"C:\System Volume Information",
            @"C:\$Recycle.Bin",
            @"C:\Recovery",
            @"C:\pagefile.sys",
            @"C:\hiberfil.sys",
            @"C:\swapfile.sys",
            @"C:\Users\All Users",
            @"C:\Users\Default",
            @"C:\ProgramData\Microsoft\Windows\Start Menu",
            @"C:\Boot"
        };

        // Extensões de arquivo consideradas seguras para exibição
        private readonly HashSet<string> _safeExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".txt", ".log", ".json", ".xml", ".csv", ".md", ".yml", ".yaml",
            ".js", ".css", ".html", ".htm", ".php", ".asp", ".aspx",
            ".cs", ".vb", ".cpp", ".h", ".java", ".py", ".rb", ".go",
            ".sql", ".config", ".ini", ".cfg", ".properties"
        };

        public FileManagerService(ILogger<FileManagerService> logger)
        {
            _logger = logger;
        }

        public async Task<FileSystemResponse> GetDirectoryContentsAsync(
            string? path = null, 
            bool includeHidden = false,
            string? fileExtensionFilter = null,
            string sortBy = "name",
            bool ascending = true)
        {
            var response = new FileSystemResponse();

            try
            {
                // Define o caminho padrão se não fornecido
                if (string.IsNullOrWhiteSpace(path))
                {
                    path = @"C:\";
                }

                // Normaliza o caminho
                path = Path.GetFullPath(path);
                response.CurrentPath = path;

                _logger.LogInformation("Navegando no diretório: {Path}", path);

                // Verifica se o caminho é válido e acessível
                if (!IsPathValidAndAccessible(path))
                {
                    response.IsAccessible = false;
                    response.ErrorMessage = "Caminho inválido ou inacessível";
                    return response;
                }

                // Verifica se o caminho está bloqueado
                if (IsPathBlocked(path))
                {
                    response.IsAccessible = false;
                    response.ErrorMessage = "Acesso negado: caminho restrito por política de segurança";
                    _logger.LogWarning("Tentativa de acesso a caminho bloqueado: {Path}", path);
                    return response;
                }

                // Define o caminho pai
                var parentDirectory = Directory.GetParent(path);
                response.ParentPath = parentDirectory?.FullName;

                var items = new List<FileSystemItem>();

                // Obtém diretórios
                await Task.Run(() =>
                {
                    try
                    {
                        var directories = Directory.GetDirectories(path)
                            .Where(dir => includeHidden || !IsHidden(dir));

                        foreach (var directory in directories)
                        {
                            try
                            {
                                var dirInfo = new DirectoryInfo(directory);
                                var item = new FileSystemItem
                                {
                                    Name = dirInfo.Name,
                                    FullPath = dirInfo.FullName,
                                    Type = "directory",
                                    Size = null,
                                    LastModified = dirInfo.LastWriteTime,
                                    IsAccessible = true
                                };
                                items.Add(item);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning("Erro ao acessar diretório {Directory}: {Error}", directory, ex.Message);
                                // Adiciona item com acesso negado
                                items.Add(new FileSystemItem
                                {
                                    Name = Path.GetFileName(directory),
                                    FullPath = directory,
                                    Type = "directory",
                                    Size = null,
                                    LastModified = DateTime.MinValue,
                                    IsAccessible = false
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao listar diretórios em {Path}", path);
                    }
                });

                // Obtém arquivos
                await Task.Run(() =>
                {
                    try
                    {
                        var files = Directory.GetFiles(path)
                            .Where(file => includeHidden || !IsHidden(file))
                            .Where(file => string.IsNullOrEmpty(fileExtensionFilter) || 
                                         Path.GetExtension(file).Equals(fileExtensionFilter, StringComparison.OrdinalIgnoreCase));

                        foreach (var file in files)
                        {
                            try
                            {
                                var fileInfo = new FileInfo(file);
                                var item = new FileSystemItem
                                {
                                    Name = fileInfo.Name,
                                    FullPath = fileInfo.FullName,
                                    Type = "file",
                                    Size = fileInfo.Length,
                                    LastModified = fileInfo.LastWriteTime,
                                    Extension = fileInfo.Extension,
                                    IsAccessible = true
                                };
                                items.Add(item);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning("Erro ao acessar arquivo {File}: {Error}", file, ex.Message);
                                // Adiciona item com acesso negado
                                items.Add(new FileSystemItem
                                {
                                    Name = Path.GetFileName(file),
                                    FullPath = file,
                                    Type = "file",
                                    Size = null,
                                    LastModified = DateTime.MinValue,
                                    Extension = Path.GetExtension(file),
                                    IsAccessible = false
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao listar arquivos em {Path}", path);
                    }
                });

                // Aplica ordenação
                items = ApplySorting(items, sortBy, ascending);

                response.Items = items;
                response.TotalItems = items.Count;
                response.TotalDirectories = items.Count(i => i.Type == "directory");
                response.TotalFiles = items.Count(i => i.Type == "file");

                _logger.LogInformation("Listagem concluída: {TotalItems} itens encontrados em {Path}", 
                    response.TotalItems, path);
            }
            catch (UnauthorizedAccessException ex)
            {
                response.IsAccessible = false;
                response.ErrorMessage = "Acesso negado ao diretório";
                _logger.LogWarning("Acesso negado ao diretório {Path}: {Error}", path, ex.Message);
            }
            catch (DirectoryNotFoundException ex)
            {
                response.IsAccessible = false;
                response.ErrorMessage = "Diretório não encontrado";
                _logger.LogWarning("Diretório não encontrado {Path}: {Error}", path, ex.Message);
            }
            catch (Exception ex)
            {
                response.IsAccessible = false;
                response.ErrorMessage = $"Erro interno: {ex.Message}";
                _logger.LogError(ex, "Erro ao navegar no diretório {Path}", path);
            }

            return response;
        }

        public bool IsPathValidAndAccessible(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    return false;

                // Verifica se o caminho é válido
                var fullPath = Path.GetFullPath(path);
                
                // Verifica se existe
                return Directory.Exists(fullPath) || File.Exists(fullPath);
            }
            catch
            {
                return false;
            }
        }

        public bool IsPathBlocked(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                var fullPath = Path.GetFullPath(path);
                
                // Normaliza o caminho para comparação
                if (fullPath.EndsWith("\\") && fullPath.Length > 3)
                {
                    fullPath = fullPath.TrimEnd('\\');
                }

                // Verifica se o caminho exato está bloqueado
                if (_blockedPaths.Contains(fullPath, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }

                // Verifica se o caminho está dentro de um diretório bloqueado
                return _blockedPaths.Any(blockedPath => 
                    fullPath.StartsWith(blockedPath + "\\", StringComparison.OrdinalIgnoreCase) ||
                    fullPath.StartsWith(blockedPath + "/", StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return true; // Bloqueia em caso de erro
            }
        }

        public async Task<FileSystemItem?> GetItemInfoAsync(string path)
        {
            try
            {
                if (!IsPathValidAndAccessible(path) || IsPathBlocked(path))
                    return null;

                return await Task.Run(() =>
                {
                    if (Directory.Exists(path))
                    {
                        var dirInfo = new DirectoryInfo(path);
                        return new FileSystemItem
                        {
                            Name = dirInfo.Name,
                            FullPath = dirInfo.FullName,
                            Type = "directory",
                            Size = null,
                            LastModified = dirInfo.LastWriteTime,
                            IsAccessible = true
                        };
                    }
                    else if (File.Exists(path))
                    {
                        var fileInfo = new FileInfo(path);
                        return new FileSystemItem
                        {
                            Name = fileInfo.Name,
                            FullPath = fileInfo.FullName,
                            Type = "file",
                            Size = fileInfo.Length,
                            LastModified = fileInfo.LastWriteTime,
                            Extension = fileInfo.Extension,
                            IsAccessible = true
                        };
                    }

                    return null;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter informações do item {Path}", path);
                return null;
            }
        }

        public List<FileSystemItem> GetAvailableDrives()
        {
            var drives = new List<FileSystemItem>();

            try
            {
                foreach (var drive in DriveInfo.GetDrives())
                {
                    try
                    {
                        var item = new FileSystemItem
                        {
                            Name = $"{drive.Name} ({GetDriveTypeDescription(drive.DriveType)})",
                            FullPath = drive.RootDirectory.FullName,
                            Type = "drive",
                            Size = drive.IsReady ? drive.TotalSize : null,
                            LastModified = DateTime.Now,
                            IsAccessible = drive.IsReady
                        };
                        drives.Add(item);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Erro ao acessar drive {DriveName}: {Error}", drive.Name, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar drives disponíveis");
            }

            return drives;
        }

        private bool IsHidden(string path)
        {
            try
            {
                var attributes = File.GetAttributes(path);
                return (attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
            }
            catch
            {
                return false;
            }
        }

        private List<FileSystemItem> ApplySorting(List<FileSystemItem> items, string sortBy, bool ascending)
        {
            return sortBy.ToLower() switch
            {
                "size" => ascending 
                    ? items.OrderBy(i => i.Size ?? 0).ToList()
                    : items.OrderByDescending(i => i.Size ?? 0).ToList(),
                "date" => ascending
                    ? items.OrderBy(i => i.LastModified).ToList()
                    : items.OrderByDescending(i => i.LastModified).ToList(),
                "type" => ascending
                    ? items.OrderBy(i => i.Type).ThenBy(i => i.Name).ToList()
                    : items.OrderByDescending(i => i.Type).ThenByDescending(i => i.Name).ToList(),
                _ => ascending // "name" ou default
                    ? items.OrderBy(i => i.Type).ThenBy(i => i.Name).ToList()
                    : items.OrderByDescending(i => i.Type).ThenByDescending(i => i.Name).ToList()
            };
        }

        private string GetDriveTypeDescription(DriveType driveType)
        {
            return driveType switch
            {
                DriveType.Fixed => "Disco Local",
                DriveType.Removable => "Disco Removível",
                DriveType.Network => "Unidade de Rede",
                DriveType.CDRom => "CD/DVD",
                DriveType.Ram => "Disco RAM",
                _ => "Desconhecido"
            };
        }

        public async Task<bool> CreateDirectoryAsync(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    _logger.LogWarning("Tentativa de criar pasta com caminho vazio");
                    return false;
                }

                if (IsPathBlocked(path))
                {
                    _logger.LogWarning("Tentativa de criar pasta em caminho bloqueado: {Path}", path);
                    return false;
                }

                if (Directory.Exists(path))
                {
                    _logger.LogWarning("Pasta já existe: {Path}", path);
                    return false;
                }

                await Task.Run(() => Directory.CreateDirectory(path));
                _logger.LogInformation("Pasta criada com sucesso: {Path}", path);
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Acesso negado ao criar pasta: {Path}", path);
                return false;
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.LogError(ex, "Diretório pai não encontrado ao criar pasta: {Path}", path);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pasta: {Path}", path);
                return false;
            }
        }

        public async Task<bool> RenameDirectoryAsync(string oldPath, string newName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(oldPath) || string.IsNullOrWhiteSpace(newName))
                {
                    _logger.LogWarning("Tentativa de renomear pasta com parâmetros inválidos");
                    return false;
                }

                if (!Directory.Exists(oldPath))
                {
                    _logger.LogWarning("Pasta não encontrada para renomear: {Path}", oldPath);
                    return false;
                }

                if (IsPathBlocked(oldPath))
                {
                    _logger.LogWarning("Tentativa de renomear pasta em caminho bloqueado: {Path}", oldPath);
                    return false;
                }

                var parentPath = Path.GetDirectoryName(oldPath);
                if (string.IsNullOrEmpty(parentPath))
                {
                    _logger.LogWarning("Não é possível determinar o diretório pai de: {Path}", oldPath);
                    return false;
                }

                var newPath = Path.Combine(parentPath, newName);

                if (IsPathBlocked(newPath))
                {
                    _logger.LogWarning("Tentativa de renomear pasta para caminho bloqueado: {NewPath}", newPath);
                    return false;
                }

                if (Directory.Exists(newPath))
                {
                    _logger.LogWarning("Pasta de destino já existe: {NewPath}", newPath);
                    return false;
                }

                await Task.Run(() => Directory.Move(oldPath, newPath));
                _logger.LogInformation("Pasta renomeada com sucesso: {OldPath} -> {NewPath}", oldPath, newPath);
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Acesso negado ao renomear pasta: {Path}", oldPath);
                return false;
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.LogError(ex, "Pasta não encontrada ao renomear: {Path}", oldPath);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao renomear pasta: {Path}", oldPath);
                return false;
            }
        }

        public async Task<bool> DeleteDirectoryAsync(string path, bool recursive = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    _logger.LogWarning("Tentativa de deletar pasta com caminho vazio");
                    return false;
                }

                if (!Directory.Exists(path))
                {
                    _logger.LogWarning("Pasta não encontrada para deletar: {Path}", path);
                    return false;
                }

                if (IsPathBlocked(path))
                {
                    _logger.LogWarning("Tentativa de deletar pasta em caminho bloqueado: {Path}", path);
                    return false;
                }

                // Verificações de segurança adicionais para pastas críticas
                var criticalPaths = new[] { "C:\\", "C:\\Windows", "C:\\Program Files", "C:\\Program Files (x86)", 
                                          "C:\\Users", "C:\\ProgramData" };
                
                if (criticalPaths.Any(cp => string.Equals(path.TrimEnd('\\'), cp, StringComparison.OrdinalIgnoreCase)))
                {
                    _logger.LogWarning("Tentativa de deletar pasta crítica do sistema: {Path}", path);
                    return false;
                }

                await Task.Run(() => Directory.Delete(path, recursive));
                _logger.LogInformation("Pasta deletada com sucesso: {Path}, Recursive: {Recursive}", path, recursive);
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Acesso negado ao deletar pasta: {Path}", path);
                return false;
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.LogError(ex, "Pasta não encontrada ao deletar: {Path}", path);
                return false;
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Erro de E/S ao deletar pasta (pasta pode não estar vazia): {Path}", path);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar pasta: {Path}", path);
                return false;
            }
        }
    }
}
