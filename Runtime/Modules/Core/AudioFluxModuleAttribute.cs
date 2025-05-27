using System;

namespace Ludo.AudioFlux.Modules
{
    /// <summary>
    /// Attribute to mark classes as AudioFlux modules and provide metadata
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AudioFluxModuleAttribute : Attribute
    {
        /// <summary>
        /// Unique identifier for the module
        /// </summary>
        public string ModuleId { get; }
        
        /// <summary>
        /// Human-readable name for the module
        /// </summary>
        public string ModuleName { get; }
        
        /// <summary>
        /// Version of the module
        /// </summary>
        public string Version { get; }
        
        /// <summary>
        /// Description of what the module does
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Author of the module
        /// </summary>
        public string Author { get; set; }
        
        /// <summary>
        /// Category of the module (e.g., "Advanced3D", "Performance", "Physics")
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Whether this module should be automatically enabled when discovered
        /// </summary>
        public bool AutoEnable { get; set; } = true;
        
        /// <summary>
        /// Priority for module initialization (higher numbers initialize first)
        /// </summary>
        public int InitializationPriority { get; set; } = 0;
        
        /// <summary>
        /// Minimum AudioFlux version required
        /// </summary>
        public string MinimumAudioFluxVersion { get; set; }
        
        /// <summary>
        /// Dependencies on other modules (module IDs)
        /// </summary>
        public string[] Dependencies { get; set; } = new string[0];
        
        /// <summary>
        /// Whether this module is experimental
        /// </summary>
        public bool IsExperimental { get; set; } = false;
        
        public AudioFluxModuleAttribute(string moduleId, string moduleName, string version)
        {
            ModuleId = moduleId ?? throw new ArgumentNullException(nameof(moduleId));
            ModuleName = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
            Version = version ?? throw new ArgumentNullException(nameof(version));
        }
    }
    
    /// <summary>
    /// Attribute to mark methods as module configuration options
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ModuleConfigurationAttribute : Attribute
    {
        /// <summary>
        /// Name of the configuration option
        /// </summary>
        public string ConfigName { get; }
        
        /// <summary>
        /// Description of what this configuration does
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Whether this configuration requires a restart to take effect
        /// </summary>
        public bool RequiresRestart { get; set; } = false;
        
        public ModuleConfigurationAttribute(string configName)
        {
            ConfigName = configName ?? throw new ArgumentNullException(nameof(configName));
        }
    }
    
    /// <summary>
    /// Attribute to mark properties as module settings that can be configured
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ModuleSettingAttribute : Attribute
    {
        /// <summary>
        /// Display name for the setting
        /// </summary>
        public string DisplayName { get; }
        
        /// <summary>
        /// Description of what this setting does
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Category for grouping settings in UI
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Minimum value for numeric settings
        /// </summary>
        public object MinValue { get; set; }
        
        /// <summary>
        /// Maximum value for numeric settings
        /// </summary>
        public object MaxValue { get; set; }
        
        /// <summary>
        /// Whether this setting requires a restart to take effect
        /// </summary>
        public bool RequiresRestart { get; set; } = false;
        
        public ModuleSettingAttribute(string displayName)
        {
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        }
    }
}
