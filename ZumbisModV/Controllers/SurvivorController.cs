using System;
using System.Collections.Generic;
using GTA;
using ZumbisModV.DataClasses;
using ZumbisModV.Interfaces;
using ZumbisModV.Static;
using ZumbisModV.SurvivorTypes;

namespace ZumbisModV.Controllers
{
    public class SurvivorController : Script, ISpawner
    {
        private readonly List<Survivors> _survivors = new List<Survivors>();
        private readonly int _survivorInterval = 30;
        private readonly float _survivorSpawnChance = 0.7f;
        private readonly int _merryweatherTimeout = 120;
        private DateTime _currentDelayTime;

        public event OnCreatedSurvivorsEvent CreatedSurvivors;
        public delegate void OnCreatedSurvivorsEvent();
        public bool Spawn { get; set; }
        public static SurvivorController Instance { get; private set; }

        public SurvivorController()
        {
            Instance = this;
            _survivorInterval = Settings.GetValue(
                "survivors",
                "survivor_interval",
                _survivorInterval
            );
            _survivorSpawnChance = Settings.GetValue(
                "survivors",
                "survivor_spawn_chance",
                _survivorSpawnChance
            );
            _merryweatherTimeout = Settings.GetValue(
                "survivors",
                "merryweather_timeout",
                _merryweatherTimeout
            );
            Settings.SetValue("survivors", "survivor_interval", _survivorInterval);
            Settings.SetValue("survivors", "survivor_spawn_chance", _survivorSpawnChance);
            Settings.SetValue("survivors", "merryweather_timeout", _merryweatherTimeout);
            Settings.Save();
            Tick += OnTick;
            Aborted += (sender, args) => _survivors.ForEach(s => s.Abort());
            CreatedSurvivors += OnCreatedSurvivors;
        }

        private void OnCreatedSurvivors()
        {
            bool rand = Database.Random.NextDouble() <= _survivorSpawnChance;
            EventTypes[] values = (EventTypes[])Enum.GetValues(typeof(EventTypes));
            EventTypes eventTypes = values[Database.Random.Next(values.Length)];
            Survivors currentEvent = null;
            switch (eventTypes)
            {
                case EventTypes.Friendly:
                    currentEvent = TryCreateEvent(new FriendlySurvivors());
                    break;
                case EventTypes.Hostile:
                    if (Database.Random.NextDouble() <= 20)
                    {
                        currentEvent = TryCreateEvent(new HostileSurvivors());
                        break;
                    }
                    break;
                case EventTypes.Merryweather:
                    currentEvent = TryCreateEvent(new MerryweatherSurvivors(_merryweatherTimeout));
                    break;
            }
            if (currentEvent != null)
            {
                _survivors.Add(currentEvent);
                currentEvent.SpawnEntities();
                currentEvent.Completed += (
                    survivors =>
                    {
                        SetDelayTime();
                        survivors.CleanUp();
                        _survivors.Remove(survivors);
                    }
                );
            }
        }

        private Survivors TryCreateEvent(Survivors survivors)
        {
            Survivors currentEvent = null;
            if (_survivors.FindIndex(s => s.GetType() == survivors.GetType()) <= -1)
                currentEvent = survivors;
            return currentEvent;
        }

        private void OnTick(object sender, EventArgs eventArgs)
        {
            Create();
            Destroy();

            _survivors.ForEach(s =>
            {
                s.Update();
            });
        }

        private void Destroy()
        {
            if (!Spawn)
            {
                _survivors.ForEach(
                    (
                        s =>
                        {
                            _survivors.Remove(s);
                            s.Abort();
                        }
                    )
                );
                _currentDelayTime = DateTime.UtcNow;
            }
        }

        private void Create()
        {
            if (Spawn)
            {
                if (DateTime.UtcNow > _currentDelayTime)
                {
                    CreatedSurvivors?.Invoke();
                    SetDelayTime();
                }
            }
        }

        private void SetDelayTime()
        {
            _currentDelayTime = DateTime.UtcNow + new TimeSpan(0, 0, _survivorInterval);
        }
    }
}
