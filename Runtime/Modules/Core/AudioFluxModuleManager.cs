using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Ludo.AudioFlux.Modules
{
    /// <summary>
    /// Manages discovery, initialization, and lifecycle of AudioFlux modules
    /// </summary>
    public class AudioFluxModuleManager : MonoBehaviour
    {
        [Header("Module Settings")]
        [SerializeField] private bool autoDiscoverModules = true;
        [SerializeField] private bool enableModuleUpdates = true;
        [SerializeField] private bool logModuleActivity = true;
        
        [Header("Service References")]
        [SerializeField] private MusicService musicService;
        [SerializeField] private SfxService sfxService;
        
        private readonly Dictionary<string, IAudioFluxModule> _modules = new Dictionary<string, IAudioFluxModule>();
        private readonly Dictionary<string, AudioFluxModuleAttribute> _moduleMetadata = new Dictionary<string, AudioFluxModuleAttribute>();
        private readonly List<IAudioFluxModule> _updateableModules = new List<IAudioFluxModule>();
        private readonly Dictionary<string, List<string>> _moduleDependencies = new Dictionary<string, List<string>>();
        
        private bool _isInitialized = false;
        private Version _audioFluxVersion = new Version(1, 0, 0);
        
        public IReadOnlyDictionary<string, IAudioFluxModule> Modules => _modules;
        public IReadOnlyDictionary<string, AudioFluxModuleAttribute> ModuleMetadata => _moduleMetadata;
        
        private void Awake()
        {
            if (autoDiscoverModules)
            {
                DiscoverModules();
            }
        }
        
        private void Start()
        {
            InitializeModules();
        }
        
        private void Update()
        {
            if (enableModuleUpdates && _isInitialized)
            {
                foreach (var module in _updateableModules)
                {
                    try
                    {
                        module.OnModuleUpdate();
                    }
                    catch (Exception ex)
                    {
                        if (logModuleActivity)
                        {
                            Debug.LogError($"Error updating module {module.ModuleId}: {ex.Message}");
                        }
                    }
                }
            }
        }
        
        private void OnDestroy()
        {
            DestroyAllModules();
        }
        
        /// <summary>
        /// Discover all modules in the current assemblies
        /// </summary>
        public void DiscoverModules()
        {
            if (logModuleActivity)
            {
                Debug.Log("AudioFlux: Discovering modules...");
            }
            
            var moduleTypes = new List<Type>();
            
            // Search all loaded assemblies for module types
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.GetCustomAttribute<AudioFluxModuleAttribute>() != null)
                        .Where(t => typeof(IAudioFluxModule).IsAssignableFrom(t))
                        .Where(t => !t.IsAbstract && !t.IsInterface);
                    
                    moduleTypes.AddRange(types);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    if (logModuleActivity)
                    {
                        Debug.LogWarning($"AudioFlux: Could not load types from assembly {assembly.FullName}: {ex.Message}");
                    }
                }
            }
            
            // Create instances of discovered modules
            foreach (var moduleType in moduleTypes)
            {
                try
                {
                    var attribute = moduleType.GetCustomAttribute<AudioFluxModuleAttribute>();
                    var module = (IAudioFluxModule)Activator.CreateInstance(moduleType);
                    
                    RegisterModule(module, attribute);
                }
                catch (Exception ex)
                {
                    if (logModuleActivity)
                    {
                        Debug.LogError($"AudioFlux: Failed to create module instance of type {moduleType.Name}: {ex.Message}");
                    }
                }
            }
            
            if (logModuleActivity)
            {
                Debug.Log($"AudioFlux: Discovered {_modules.Count} modules");
            }
        }
        
        /// <summary>
        /// Register a module manually
        /// </summary>
        public void RegisterModule(IAudioFluxModule module, AudioFluxModuleAttribute metadata = null)
        {
            if (module == null)
                throw new ArgumentNullException(nameof(module));
            
            if (_modules.ContainsKey(module.ModuleId))
            {
                if (logModuleActivity)
                {
                    Debug.LogWarning($"AudioFlux: Module {module.ModuleId} is already registered");
                }
                return;
            }
            
            _modules[module.ModuleId] = module;
            
            if (metadata != null)
            {
                _moduleMetadata[module.ModuleId] = metadata;
            }
            
            // Store dependencies for later resolution
            if (module.Dependencies != null && module.Dependencies.Length > 0)
            {
                _moduleDependencies[module.ModuleId] = new List<string>(module.Dependencies);
            }
            
            ModuleEvents.InvokeModuleDiscovered(module);
            
            if (logModuleActivity)
            {
                Debug.Log($"AudioFlux: Registered module {module.ModuleId} ({module.ModuleName})");
            }
        }
        
        /// <summary>
        /// Initialize all registered modules
        /// </summary>
        public void InitializeModules()
        {
            if (_isInitialized)
                return;
            
            if (musicService == null || sfxService == null)
            {
                Debug.LogError("AudioFlux: Cannot initialize modules without MusicService and SfxService references");
                return;
            }
            
            if (logModuleActivity)
            {
                Debug.Log("AudioFlux: Initializing modules...");
            }
            
            // Sort modules by initialization priority
            var sortedModules = _modules.Values
                .Where(m => _moduleMetadata.ContainsKey(m.ModuleId))
                .OrderByDescending(m => _moduleMetadata[m.ModuleId].InitializationPriority)
                .ToList();
            
            // Initialize modules in priority order
            foreach (var module in sortedModules)
            {
                InitializeModule(module);
            }
            
            // Initialize modules without metadata
            foreach (var module in _modules.Values.Where(m => !_moduleMetadata.ContainsKey(m.ModuleId)))
            {
                InitializeModule(module);
            }
            
            _isInitialized = true;
            
            if (logModuleActivity)
            {
                Debug.Log($"AudioFlux: Initialized {_modules.Count} modules");
            }
        }
        
        private void InitializeModule(IAudioFluxModule module)
        {
            try
            {
                // Check compatibility
                if (!module.IsCompatible(_audioFluxVersion))
                {
                    if (logModuleActivity)
                    {
                        Debug.LogWarning($"AudioFlux: Module {module.ModuleId} is not compatible with AudioFlux version {_audioFluxVersion}");
                    }
                    return;
                }
                
                // Check dependencies
                if (!ResolveDependencies(module))
                {
                    if (logModuleActivity)
                    {
                        Debug.LogWarning($"AudioFlux: Module {module.ModuleId} dependencies could not be resolved");
                    }
                    return;
                }
                
                // Initialize the module
                module.Initialize(musicService, sfxService);
                
                // Subscribe to events if the module implements specific interfaces
                SubscribeToEvents(module);
                
                // Enable the module if auto-enable is set
                var metadata = _moduleMetadata.GetValueOrDefault(module.ModuleId);
                if (metadata?.AutoEnable ?? true)
                {
                    EnableModule(module.ModuleId);
                }
                
                ModuleEvents.InvokeModuleInitialized(module);
                
                if (logModuleActivity)
                {
                    Debug.Log($"AudioFlux: Initialized module {module.ModuleId}");
                }
            }
            catch (Exception ex)
            {
                ModuleEvents.InvokeModuleInitializationFailed(module, ex);
                
                if (logModuleActivity)
                {
                    Debug.LogError($"AudioFlux: Failed to initialize module {module.ModuleId}: {ex.Message}");
                }
            }
        }
        
        private bool ResolveDependencies(IAudioFluxModule module)
        {
            if (module.Dependencies == null || module.Dependencies.Length == 0)
                return true;
            
            var missingDependencies = new List<string>();
            
            foreach (var dependency in module.Dependencies)
            {
                if (!_modules.ContainsKey(dependency))
                {
                    missingDependencies.Add(dependency);
                }
            }
            
            if (missingDependencies.Count > 0)
            {
                ModuleEvents.InvokeModuleDependenciesFailed(module, missingDependencies.ToArray());
                return false;
            }
            
            ModuleEvents.InvokeModuleDependenciesResolved(module, module.Dependencies);
            return true;
        }
        
        private void SubscribeToEvents(IAudioFluxModule module)
        {
            if (module is IMusicServiceModule musicModule)
            {
                MusicEvents.OnMusicStarted += musicModule.OnMusicStarted;
                MusicEvents.OnMusicStopped += musicModule.OnMusicStopped;
                MusicEvents.OnCrossfadeStarted += musicModule.OnCrossfadeStarted;
                MusicEvents.OnBeat += musicModule.OnBeat;
                MusicEvents.OnBar += musicModule.OnBar;
            }
            
            if (module is ISFXServiceModule sfxModule)
            {
                SoundEvents.OnSoundStarted += sfxModule.OnSoundStarted;
                SoundEvents.OnSoundStopped += sfxModule.OnSoundStopped;
                SoundEvents.OnSoundCompleted += sfxModule.OnSoundCompleted;
            }
        }
        
        /// <summary>
        /// Enable a specific module
        /// </summary>
        public bool EnableModule(string moduleId)
        {
            if (!_modules.TryGetValue(moduleId, out var module))
                return false;
            
            try
            {
                module.OnModuleEnabled();
                
                // Add to updateable modules if it needs updates
                if (!_updateableModules.Contains(module))
                {
                    _updateableModules.Add(module);
                }
                
                ModuleEvents.InvokeModuleEnabled(module);
                
                if (logModuleActivity)
                {
                    Debug.Log($"AudioFlux: Enabled module {moduleId}");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                if (logModuleActivity)
                {
                    Debug.LogError($"AudioFlux: Failed to enable module {moduleId}: {ex.Message}");
                }
                return false;
            }
        }
        
        /// <summary>
        /// Disable a specific module
        /// </summary>
        public bool DisableModule(string moduleId)
        {
            if (!_modules.TryGetValue(moduleId, out var module))
                return false;
            
            try
            {
                module.OnModuleDisabled();
                _updateableModules.Remove(module);
                
                ModuleEvents.InvokeModuleDisabled(module);
                
                if (logModuleActivity)
                {
                    Debug.Log($"AudioFlux: Disabled module {moduleId}");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                if (logModuleActivity)
                {
                    Debug.LogError($"AudioFlux: Failed to disable module {moduleId}: {ex.Message}");
                }
                return false;
            }
        }
        
        /// <summary>
        /// Get a specific module by ID
        /// </summary>
        public T GetModule<T>(string moduleId) where T : class, IAudioFluxModule
        {
            return _modules.GetValueOrDefault(moduleId) as T;
        }
        
        /// <summary>
        /// Get all modules of a specific type
        /// </summary>
        public IEnumerable<T> GetModules<T>() where T : class, IAudioFluxModule
        {
            return _modules.Values.OfType<T>();
        }
        
        private void DestroyAllModules()
        {
            foreach (var module in _modules.Values)
            {
                try
                {
                    module.OnModuleDestroy();
                    ModuleEvents.InvokeModuleDestroyed(module);
                }
                catch (Exception ex)
                {
                    if (logModuleActivity)
                    {
                        Debug.LogError($"AudioFlux: Error destroying module {module.ModuleId}: {ex.Message}");
                    }
                }
            }
            
            _modules.Clear();
            _moduleMetadata.Clear();
            _updateableModules.Clear();
            _moduleDependencies.Clear();
        }
    }
}
