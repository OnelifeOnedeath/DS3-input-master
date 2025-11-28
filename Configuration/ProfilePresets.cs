// Configuration/ProfilePresets.cs
using DS3InputMaster.Models.InputProfiles;

namespace DS3InputMaster.Configuration
{
    /// <summary>
    /// Предустановленные профили для различных стилей игры
    /// </summary>
    public static class ProfilePresets
    {
        public static ControlProfile CreatePvpProfile()
        {
            return new ControlProfile
            {
                Name = "PvP",
                Description = "Оптимизирован для сражений с другими игроками",
                Mouse = new MouseSettings
                {
                    CameraSensitivity = 1.2f,
                    AimingSensitivity = 0.9f,
                    BowSensitivity = 0.7f,
                    Smoothing = 0.05f // Меньше сглаживания для быстрой реакции
                },
                Movement = new MovementSettings
                {
                    WalkThreshold = 0.2f,
                    RunThreshold = 0.9f,
                    AnalogResponseCurve = 1.8f, // Более агрессивная кривая
                    ToggleSprint = false // Удержание для спринта
                },
                Combat = new CombatSettings
                {
                    QuickAttackTiming = 0.12f, // Более быстрое выполнение комбо
                    ParryWindow = 0.18f, // Уже окно для точного парирования
                    DodgeAttackDelay = 0.08f, // Быстрее контратаки после уворота
                    AutoWeaponSwap = true // Автопереключение оружия
                }
            };
        }

        public static ControlProfile CreateMagicProfile()
        {
            return new ControlProfile
            {
                Name = "Magic",
                Description = "Для заклинателей и пиромантов",
                Mouse = new MouseSettings
                {
                    CameraSensitivity = 0.8f,
                    AimingSensitivity = 0.6f,
                    BowSensitivity = 0.4f, // Низкая чувствительность для точного прицеливания
                    Smoothing = 0.15f // Больше сглаживания для плавного прицеливания
                },
                Movement = new MovementSettings
                {
                    WalkThreshold = 0.4f,
                    RunThreshold = 0.7f, // Чаще ходьба для экономии выносливости
                    AnalogResponseCurve = 1.2f,
                    ToggleSprint = true
                },
                Combat = new CombatSettings
                {
                    QuickAttackTiming = 0.2f,
                    ParryWindow = 0.25f, // Шире окно для компенсации дальнего боя
                    DodgeAttackDelay = 0.15f // Больше задержка для кастеров
                }
            };
        }

        public static ControlProfile CreateControllerLikeProfile()
        {
            return new ControlProfile
            {
                Name = "ControllerLike", 
                Description = "Эмуляция ощущений от геймпада",
                Mouse = new MouseSettings
                {
                    CameraSensitivity = 0.6f,
                    AimingSensitivity = 0.4f,
                    BowSensitivity = 0.3f,
                    Smoothing = 0.2f, // Сильное сглаживание как на геймпаде
                    InvertY = false
                },
                Movement = new MovementSettings
                {
                    WalkThreshold = 0.5f, // Плавные переходы между ходьбой и бегом
                    RunThreshold = 0.9f,
                    AnalogResponseCurve = 2.0f, // S-образная кривая как на стиках
                    ToggleSprint = true
                },
                Combat = new CombatSettings
                {
                    QuickAttackTiming = 0.25f,
                    ParryWindow = 0.3f,
                    DodgeAttackDelay = 0.12f
                }
            };
        }

        public static ControlProfile CreateAccessibilityProfile()
        {
            return new ControlProfile
            {
                Name = "Accessibility",
                Description = "Настройки для игроков с ограниченными возможностями",
                Mouse = new MouseSettings
                {
                    CameraSensitivity = 0.5f,
                    AimingSensitivity = 0.4f,
                    BowSensitivity = 0.3f,
                    Smoothing = 0.3f, // Максимальное сглаживание
                    InvertY = false
                },
                Movement = new MovementSettings
                {
                    WalkThreshold = 0.6f, // Преимущественно ходьба
                    RunThreshold = 1.0f, // Бег только на полном нажатии
                    AnalogResponseCurve = 1.0f, // Линейный отклик
                    ToggleSprint = true // Не требовать удержания
                },
                Combat = new CombatSettings
                {
                    QuickAttackTiming = 0.3f, // Увеличенное время для реакций
                    ParryWindow = 0.4f, // Широкое окно парирования
                    DodgeAttackDelay = 0.2f, // Больше времени на реакции
                    AutoWeaponSwap = true // Упрощенное управление
                }
            };
        }
    }
}
