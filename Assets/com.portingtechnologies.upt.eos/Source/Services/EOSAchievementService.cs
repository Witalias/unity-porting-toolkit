using System;
using UPT.Services;

namespace UPT.EOS
{
    public class EOSAchievementService
    {
        public event Action<string> OnAchievementUnlocked;
        public event Action<string, int, int> OnAchievementProgressed;
    }
}
