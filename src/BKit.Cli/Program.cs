using System;
using BKit.Compression;
using BKit.Core.Abstractions;
using BKit.Core.Models;
using BKit.Core.Services;
using BKit.Credentials;
using BKit.Encryption;
using BKit.Modules;
using BKit.Rotation;
using BKit.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.CommandLine;

namespace BKit.Cli
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            // Serilog konfigurieren
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs/backuptool-.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            try
            {
                var serviceProvider = ConfigureServices();
                var moduleRegistry = serviceProvider.GetRequiredService<ModuleRegistry>();

                var rootCommand = new RootCommand("BackupTool - Modulares Backup und Restore Tool");

                // list-modules Command
                var listCommand = new Command("list-modules", "Zeigt alle verfügbaren Module an");
                //listCommand.SetHandler(() =>
                //{
                //    Console.WriteLine("\nVerfügbare Module:");
                //    Console.WriteLine("==================");
                //    foreach (var module in moduleRegistry.GetAllModules())
                //    {
                //        Console.WriteLine($"  {module.ModuleName,-15} - {module.Description}");
                //    }
                //    Console.WriteLine();
                //});
                //rootCommand.AddCommand(listCommand);

                // Dynamisch Module als Commands hinzufügen
                foreach (var module in moduleRegistry.GetAllModules())
                {
                    var moduleCommand = CreateModuleCommand(module, serviceProvider);
                    //rootCommand.AddCommand(moduleCommand);
                }

                return await rootCommand.InvokeAsync(args);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Anwendung abgebrochen mit Fehler");
                return 1;
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }

            return 0;
        }

        static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Logging
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                //builder.AddSerilog(dispose: true);
            });

            // Core Services
            services.AddSingleton<IFileNameGenerator, FileNameGenerator>();
            services.AddSingleton<ChecksumService>();
            services.AddSingleton<RetryPolicyService>();

            // Factories
            services.AddSingleton<StorageFactory>();
            services.AddSingleton<CompressionFactory>();
            services.AddSingleton<EncryptionFactory>();

            // Credentials
            services.AddSingleton<ICredentialProvider, CompositeCredentialProvider>();

            // Rotation
            services.AddSingleton<RotationService>();

            // Module Registry
            services.AddSingleton<ModuleRegistry>();

            return services.BuildServiceProvider();
        }

        static Command CreateModuleCommand(IBackupModule module, ServiceProvider serviceProvider)
        {
            var moduleCommand = new Command(module.ModuleName, module.Description);

            // Backup Subcommand
            var backupCommand = new Command("backup", "Erstellt ein Backup");

            // Storage Optionen
            //var storageTypeOption = new Option<string>("--storage", "Storage Typ (filesystem, azureblob, s3)") { IsRequired = true };
            //var storagePathOption = new Option<string>("--storage-path", "Storage Pfad oder Container") { IsRequired = true };

            //backupCommand.AddOption(storageTypeOption);
            //backupCommand.AddOption(storagePathOption);

            // Compression & Encryption
            var compressionOption = new Option<string?>("--compression", "Komprimierung (gzip, none)");
            var encryptOption = new Option<bool>("--encrypt", "Backup verschlüsseln");

            //backupCommand.AddOption(compressionOption);
            //backupCommand.AddOption(encryptOption);

            // Rotation
            var rotationDailyOption = new Option<int?>("--rotation-daily", "Anzahl täglicher Backups");
            //backupCommand.AddOption(rotationDailyOption);

            // Dry-Run
            var dryRunOption = new Option<bool>("--dry-run", "Validierung ohne Ausführung");
            //backupCommand.AddOption(dryRunOption);

            // Modul-spezifische Optionen hinzufügen
            foreach (var option in module.GetRequiredOptions())
            {
                //backupCommand.AddOption(option);
            }
            foreach (var option in module.GetOptionalOptions())
            {
                //backupCommand.AddOption(option);
            }

            //backupCommand.SetHandler(async (context) =>
            //{
            //    var parseResult = context.ParseResult;
            //    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            //    try
            //    {
            //        logger.LogInformation("=== BackupTool gestartet ===");
            //        logger.LogInformation("Modul: {Module}", module.ModuleName);

            //        // BackupContext erstellen
            //        var backupContext = CreateBackupContext(
            //            module,
            //            parseResult,
            //            serviceProvider,
            //            OperationType.Backup
            //        );

            //        // Validierung
            //        var validationResult = await module.ValidateConfigurationAsync(backupContext, backupContext.DryRun);

            //        if (!validationResult.IsValid)
            //        {
            //            logger.LogError("Validierung fehlgeschlagen:");
            //            foreach (var error in validationResult.Errors)
            //            {
            //                logger.LogError("  - {Error}", error);
            //            }
            //            context.ExitCode = 1;
            //            return;
            //        }

            //        if (backupContext.DryRun)
            //        {
            //            logger.LogInformation("[DRY-RUN] Validierung erfolgreich. Keine Änderungen vorgenommen.");
            //            return;
            //        }

            //        // Backup ausführen
            //        var result = await module.ExecuteBackupAsync(backupContext);

            //        if (result.Success)
            //        {
            //            // Rotation anwenden
            //            if (backupContext.RotationPolicy != null)
            //            {
            //                var rotationService = serviceProvider.GetRequiredService<RotationService>();
            //                await rotationService.ApplyRotationAsync(
            //                    backupContext.StorageProvider,
            //                    parseResult.GetValueForOption(storagePathOption)!,
            //                    backupContext.RotationPolicy
            //                );
            //            }

            //            logger.LogInformation("=== Backup erfolgreich abgeschlossen ===");
            //            context.ExitCode = 0;
            //        }
            //        else
            //        {
            //            logger.LogError("Backup fehlgeschlagen: {Error}", result.ErrorMessage);
            //            context.ExitCode = 1;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.LogError(ex, "Unerwarteter Fehler während des Backups");
            //        context.ExitCode = 1;
            //    }
            //});

            // Restore Subcommand
            var restoreCommand = new Command("restore", "Stellt ein Backup wieder her");

            //restoreCommand.AddOption(storageTypeOption);
            //restoreCommand.AddOption(storagePathOption);
            //restoreCommand.AddOption(compressionOption);
            //restoreCommand.AddOption(encryptOption);
            //restoreCommand.AddOption(dryRunOption);

            //var restoreFileOption = new Option<string>("--restore-file", "Backup-Datei zum Wiederherstellen") { IsRequired = true };
            //restoreCommand.AddOption(restoreFileOption);

            foreach (var option in module.GetRequiredOptions())
            {
                //restoreCommand.AddOption(option);
            }
            foreach (var option in module.GetOptionalOptions())
            {
                //restoreCommand.AddOption(option);
            }

            //restoreCommand.SetHandler(async (context) =>
            //{
            //    var parseResult = context.ParseResult;
            //    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            //    try
            //    {
            //        logger.LogInformation("=== BackupTool Restore gestartet ===");

            //        var backupContext = CreateBackupContext(
            //            module,
            //            parseResult,
            //            serviceProvider,
            //            OperationType.Restore
            //        );

            //        backupContext.RestoreFilePath = parseResult.GetValueForOption(restoreFileOption);

            //        if (backupContext.DryRun)
            //        {
            //            logger.LogInformation("[DRY-RUN] Würde Backup wiederherstellen: {File}",
            //                backupContext.RestoreFilePath);
            //            return;
            //        }

            //        var result = await module.ExecuteRestoreAsync(backupContext);

            //        if (result.Success)
            //        {
            //            logger.LogInformation("=== Restore erfolgreich abgeschlossen ===");
            //            context.ExitCode = 0;
            //        }
            //        else
            //        {
            //            logger.LogError("Restore fehlgeschlagen: {Error}", result.ErrorMessage);
            //            context.ExitCode = 1;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.LogError(ex, "Unerwarteter Fehler während des Restores");
            //        context.ExitCode = 1;
            //    }
            //});

            //moduleCommand.AddCommand(backupCommand);
            //moduleCommand.AddCommand(restoreCommand);

            return moduleCommand;
        }

        static BackupContext CreateBackupContext(
            IBackupModule module,
            ParseResult parseResult,
            ServiceProvider serviceProvider,
            OperationType operationType)
        {
            var storageFactory = serviceProvider.GetRequiredService<StorageFactory>();
            var compressionFactory = serviceProvider.GetRequiredService<CompressionFactory>();
            var encryptionFactory = serviceProvider.GetRequiredService<EncryptionFactory>();

            //var storageType = parseResult.GetValueForOption<string>("--storage")!;
            //var storagePath = parseResult.GetValueForOption<string>("--storage-path")!;
            //var compressionType = parseResult.GetValueForOption<string?>("--compression");
            //var encrypt = parseResult.GetValueForOption<bool>("--encrypt");
            //var dryRun = parseResult.GetValueForOption<bool>("--dry-run");
            //var rotationDaily = parseResult.GetValueForOption<int?>("--rotation-daily");

            var context = new BackupContext
            {
                ModuleName = module.ModuleName,
                OperationType = operationType,
                //DryRun = dryRun,
                //Encrypted = encrypt,
                //CompressionType = compressionType,
                //StorageType = storageType,
                Logger = serviceProvider.GetRequiredService<ILogger<Program>>(),
                CredentialProvider = serviceProvider.GetRequiredService<ICredentialProvider>(),
                FileNameGenerator = serviceProvider.GetRequiredService<IFileNameGenerator>(),
                //StorageProvider = storageFactory.CreateProvider(storageType, new Dictionary<string, object>
                //{
                //    ["path"] = storagePath
                //}),
                //CompressionProvider = compressionFactory.CreateProvider(compressionType),
                //EncryptionProvider = encryptionFactory.CreateProvider(encrypt)
            };

            //if (rotationDaily.HasValue)
            //{
            //    context.RotationPolicy = new RotationPolicy
            //    {
            //        DailyRetention = rotationDaily.Value
            //    };
            //}

            // Modul-spezifische Konfiguration sammeln
            foreach (var option in module.GetRequiredOptions().Concat(module.GetOptionalOptions()))
            {
                var optionName = option.Name.TrimStart('-');
                //var value = parseResult.GetValueForOption(option);
                //if (value != null)
                //{
                //    context.ModuleConfiguration[optionName] = value;
                //}
            }

            return context;
        }
    }
}
