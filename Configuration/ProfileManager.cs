using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DS3InputMaster.Models.InputProfiles;

namespace DS3InputMaster.Configuration
{
    /// <summary>
    /// Менеджер профилей управления - загрузка, сохранение, применение настроек
    /// </summary>
    public class ProfileManager
    {
        private readonly string _profilesDirectory;
        private readonly Dictionary<string, ControlProfile> _loadedProfiles;
        private ControlProfile _activeProfile;
        private ControlProfile _defaultProfile;

        public event Action<ControlProfile> ProfileChanged;
        public event Action<string> ProfileSaved;
        public event Action<string> ProfileLoaded;

        public ControlProfile ActiveProfile 
        { 
            get => _activeProfile;
            private set
            {
                if (_activeProfile != value)
                {
                    _activeProfile = value;
                    ProfileChanged?.Invoke(value);
                }
            }
        }

        public IReadOnlyDictionary<string, ControlProfile> LoadedProfiles => _loadedProfiles;
        public IEnumerable<string> AvailableProfileNames => _loadedProfiles.Keys;

        public ProfileManager()
        {
            _profilesDirectory = GetProfilesDirectory();
            _loadedProfiles = new Dictionary<string, ControlProfile>(StringComparer.OrdinalIgnoreCase);
            
            EnsureProfilesDirectoryExists();
            CreateDefaultProfile();
        }

        public async Task InitializeAsync()
        {
            await LoadAllProfilesAsync();
            
            // Активируем профиль по умолчанию или последний использованный
            var lastUsed = await GetLastUsedProfileNameAsync();
            var initialProfile = !string.IsNullOrEmpty(lastUsed) && _loadedProfiles.ContainsKey(lastUsed) 
                ? _loadedProfiles[lastUsed] 
                : _defaultProfile;

            ActiveProfile = initialProfile;
        }

        public void ApplyProfile(string profileName)
        {
            if (_loadedProfiles.TryGetValue(profileName, out var profile))
            {
                ActiveProfile = profile;
                _ = SaveLastUsedProfileNameAsync(profileName);
            }
        }

        public void ApplyProfile(ControlProfile profile)
        {
            if (profile != null)
            {
                ActiveProfile = profile;
                
                // Если это именованный профиль, сохраняем как последний использованный
                if (!string.IsNullOrEmpty(profile.Name) && _loadedProfiles.ContainsKey(profile.Name))
                {
                    _ = SaveLastUsedProfileNameAsync(profile.Name);
                }
            }
        }

        public async Task<bool> SaveProfileAsync(ControlProfile profile, string profileName = null)
        {
            try
            {
                var name = profileName ?? profile.Name;
                if (string.IsNullOrEmpty(name))
                    return false;

                // Обновляем метаданные
                profile.Name = name;
                profile.UpdatedAt = DateTime.Now;

                // Сериализуем в JSON
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var json = JsonSerializer.Serialize(profile, options);
                var filePath = GetProfileFilePath(name);

                await File.WriteAllTextAsync(filePath, json);

                // Обновляем кэш
                _loadedProfiles[name] = profile;
                ProfileSaved?.Invoke(name);

                return true;
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                return false;
            }
        }

        public async Task<ControlProfile> LoadProfileAsync(string profileName)
        {
            try
            {
                var filePath = GetProfileFilePath(profileName);
                if (!File.Exists(filePath))
                    return null;

                var json = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                };
                
                var profile = JsonSerializer.Deserialize<ControlProfile>(json, options);
                
                if (profile != null)
                {
                    _loadedProfiles[profileName] = profile;
                    ProfileLoaded?.Invoke(profileName);
                }

                return profile;
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                return null;
            }
        }

        public async Task<bool> DeleteProfileAsync(string profileName)
        {
            try
            {
                var filePath = GetProfileFilePath(profileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                _loadedProfiles.Remove(profileName);

                // Если удаляем активный профиль, переключаемся на дефолтный
                if (ActiveProfile?.Name == profileName)
                {
                    ActiveProfile = _defaultProfile;
                }

                return true;
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                return false;
            }
        }

        public ControlProfile CreateNewProfile(string name, string description = null)
        {
            var newProfile = CloneProfile(_defaultProfile);
            newProfile.Name = name;
            newProfile.Description = description ?? $"Профиль {name}";
            newProfile.CreatedAt = DateTime.Now;
            newProfile.UpdatedAt = DateTime.Now;

            _loadedProfiles[name] = newProfile;
            return newProfile;
        }

        public ControlProfile CloneProfile(ControlProfile source)
        {
            var json = JsonSerializer.Serialize(source);
            return JsonSerializer.Deserialize<ControlProfile>(json);
        }

        public void UpdateActiveProfileSensitivity(MouseCalibrationData calibrationData)
        {
            if (ActiveProfile != null && calibrationData != null)
            {
                // Адаптируем чувствительность на основе калибровочных данных
                var baseSensitivity = calibrationData.BaseSensitivity;
                ActiveProfile.Mouse.CameraSensitivity *= baseSensitivity;
                ActiveProfile.Mouse.AimingSensitivity *= baseSensitivity;
                ActiveProfile.Mouse.BowSensitivity *= baseSensitivity;

                ActiveProfile.UpdatedAt = DateTime.Now;
            }
        }

        private async Task LoadAllProfilesAsync()
        {
            try
            {
                if (!Directory.Exists(_profilesDirectory))
                    return;

                var profileFiles = Directory.GetFiles(_profilesDirectory, "*.json");
                var loadTasks = profileFiles.Select(async filePath =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    return await LoadProfileAsync(fileName);
                });

                var profiles = await Task.WhenAll(loadTasks);
                
                foreach (var profile in profiles.Where(p => p != null))
                {
                    _loadedProfiles[profile.Name] = profile;
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки
            }
        }

        private void CreateDefaultProfile()
        {
            _defaultProfile = new ControlProfile
            {
                Name = "Default",
                Description = "Стандартный профиль для Dark Souls 3",
                Mouse = new MouseSettings
                {
                    CameraSensitivity = 1.0f,
                    AimingSensitivity = 0.7f,
                    BowSensitivity = 0.5f,
                    Smoothing = 0.1f
                },
                Movement = new MovementSettings
                {
                    WalkThreshold = 0.3f,
                    RunThreshold = 0.8f,
                    AnalogResponseCurve = 1.5f,
                    ToggleSprint = true
                },
                Combat = new CombatSettings
                {
                    QuickAttackTiming = 0.15f,
                    ParryWindow = 0.2f,
                    DodgeAttackDelay = 0.1f
                },
                Bindings = CreateDefaultBindings()
            };

            _loadedProfiles["Default"] = _defaultProfile;
        }

        private KeyBindings CreateDefaultBindings()
        {
            return new KeyBindings
            {
                Actions = new Dictionary<GameAction, InputBinding>
                {
                    [GameAction.MoveForward] = new InputBinding { PrimaryKey = VirtualKey.W },
                    [GameAction.MoveBackward] = new InputBinding { PrimaryKey = VirtualKey.S },
                    [GameAction.MoveLeft] = new InputBinding { PrimaryKey = VirtualKey.A },
                    [GameAction.MoveRight] = new InputBinding { PrimaryKey = VirtualKey.D },
                    [GameAction.Roll] = new InputBinding { PrimaryKey = VirtualKey.Space },
                    [GameAction.Jump] = new InputBinding { PrimaryKey = VirtualKey.Space, Modifiers = InputModifier.Shift },
                    [GameAction.Sprint] = new InputBinding { PrimaryKey = VirtualKey.Shift },
                    [GameAction.LightAttack] = new InputBinding { MouseButton = MouseButton.Left },
                    [GameAction.HeavyAttack] = new InputBinding { MouseButton = MouseButton.Right },
                    [GameAction.Parry] = new InputBinding { PrimaryKey = VirtualKey.Q },
                    [GameAction.UseItem] = new InputBinding { PrimaryKey = VirtualKey.R },
                    [GameAction.LockOn] = new InputBinding { PrimaryKey = VirtualKey.F },
                    [GameAction.Menu] = new InputBinding { PrimaryKey = VirtualKey.Escape },
                    [GameAction.SwitchRightHand] = new InputBinding { PrimaryKey = VirtualKey.X },
                    [GameAction.SwitchLeftHand] = new InputBinding { PrimaryKey = VirtualKey.Z },
                    [GameAction.SwitchSpell] = new InputBinding { PrimaryKey = VirtualKey.Tab },
                    [GameAction.SwitchItem] = new InputBinding { PrimaryKey = VirtualKey.Tab, Modifiers = InputModifier.Shift }
                }
            };
        }

        private string GetProfilesDirectory()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "DS3InputMaster", "Profiles");
        }

        private void EnsureProfilesDirectoryExists()
        {
            if (!Directory.Exists(_profilesDirectory))
            {
                Directory.CreateDirectory(_profilesDirectory);
            }
        }

        private string GetProfileFilePath(string profileName)
        {
            var safeFileName = Path.GetInvalidFileNameChars()
                .Aggregate(profileName, (current, c) => current.Replace(c, '_'));
                
            return Path.Combine(_profilesDirectory, $"{safeFileName}.json");
        }

        private async Task<string> GetLastUsedProfileNameAsync()
        {
            try
            {
                var settingsPath = Path.Combine(Path.GetDirectoryName(_profilesDirectory), "settings.json");
                if (File.Exists(settingsPath))
                {
                    var json = await File.ReadAllTextAsync(settingsPath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    return settings?.LastUsedProfile;
                }
            }
            catch
            {
                // Игнорируем ошибки чтения настроек
            }
            
            return null;
        }

        private async Task SaveLastUsedProfileNameAsync(string profileName)
        {
            try
            {
                var settingsDirectory = Path.GetDirectoryName(_profilesDirectory);
                var settingsPath = Path.Combine(settingsDirectory, "settings.json");
                
                var settings = new AppSettings { LastUsedProfile = profileName };
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                
                await File.WriteAllTextAsync(settingsPath, json);
            }
            catch
            {
                // Игнорируем ошибки сохранения настроек
            }
        }
    }

    public class AppSettings
    {
        public string LastUsedProfile { get; set; }
    }

    public class MouseCalibrationData
    {
        public float BaseSensitivity { get; set; } = 1.0f;
        public float MeasuredDpi { get; set; }
        public float UserPreferenceMultiplier { get; set; } = 1.0f;
    }
}
