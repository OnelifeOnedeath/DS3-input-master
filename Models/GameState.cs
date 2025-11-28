namespace DS3InputMaster.Models
{
    /// <summary>
    /// Состояния игры, влияющие на обработку ввода
    /// </summary>
    public enum GameState
    {
        Exploring,      // Исследование мира
        InCombat,       // Бой с обычными врагами
        BossFight,      // Сражение с боссом
        MenuNavigation, // Навигация в меню
        Dialog,         // Диалоги с NPC
        Aiming,         // Прицеливание оружием
        BowAiming,      // Прицеливание луком
        Parrying,       // Парный ритм
        Rolling,        // Перекаты
        Dead,           // Смерть персонажа
        Loading         // Загрузка
    }

    /// <summary>
    /// Контекст для определения состояния игры
    /// </summary>
    public class GameStateContext
    {
        public bool IsInCombat { get; set; }
        public bool HasActiveTarget { get; set; }
        public bool IsMenuOpen { get; set; }
        public bool IsAiming { get; set; }
        public float TimeSinceLastCombat { get; set; }
        public int NearbyEnemiesCount { get; set; }
    }
}
