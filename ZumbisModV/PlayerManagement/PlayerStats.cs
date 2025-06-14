using System;
using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.UI;
using LemonUI.TimerBars;
using ZumbisModV;
using ZumbisModV.Interfaces;
using ZumbisModV.PlayerManagement;
using ZumbisModV.Static;

namespace ZumbisModV.PlayerManagement
{
    public class PlayerStats : Script
    {
        public static bool UseStats = true;
        private readonly float _statDamageInterval = 5f;
        private readonly float _hungerReductionMultiplier = 0.00045f;
        private readonly float _thirstReductionMultiplier = 0.0007f;
        private readonly float _sprintReductionMultiplier = 0.05f;
        private readonly float _statSustainLength = 120f;
        private readonly List<StatDisplayItem> _statDisplay;
        private float _hungerDamageTimer;
        private float _hungerSustainTimer;
        private float _thirstDamageTimer;
        private float _thirstSustainTimer;
        private bool _removedDisplay;

        public PlayerStats()
        {
            PlayerInventory.FoodUsed += PlayerInventoryOnFoodUsed;
            _sprintReductionMultiplier = Settings.GetValue(
                "stats",
                "sprint_reduction_multiplier",
                _sprintReductionMultiplier
            );
            _hungerReductionMultiplier = Settings.GetValue(
                "stats",
                "hunger_reduction_multiplier",
                _hungerReductionMultiplier
            );
            _thirstReductionMultiplier = Settings.GetValue(
                "stats",
                "thirst_reduction_multiplier",
                _thirstReductionMultiplier
            );
            _statDamageInterval = Settings.GetValue(
                "stats",
                "stat_damage_interaval",
                _statDamageInterval
            );
            _statSustainLength = Settings.GetValue(
                "stats",
                "stat_sustain_length",
                _statSustainLength
            );
            Settings.SetValue("stats", "use_stats", UseStats);
            Settings.SetValue("stats", "sprint_reduction_multiplier", _sprintReductionMultiplier);
            Settings.SetValue("stats", "hunger_reduction_multiplier", _hungerReductionMultiplier);
            Settings.SetValue("stats", "thirst_reduction_multiplier", _thirstReductionMultiplier);
            Settings.SetValue("stats", "stat_damage_interaval", _statDamageInterval);
            Settings.SetValue("stats", "stat_sustain_length", _statSustainLength);
            Settings.Save();
            _statDisplay = new List<StatDisplayItem>();
            Stats stats = new Stats();
            foreach (Stat stat in stats.StatList)
            {
                var statDisplayItem = new StatDisplayItem
                {
                    Stat = stat,
                    Bar = new TimerBarProgress(stat.Name.ToUpper())
                    {
                        ForegroundColor = Color.White,
                        BackgroundColor = Color.Gray,
                    },
                };
                _statDisplay.Add(statDisplayItem);
                MenuController.TimerBars.Add(statDisplayItem.Bar);
            }
            Tick += OnTick;
            Interval = 10;
        }

        private void PlayerInventoryOnFoodUsed(FoodInventoryItem item, FoodType foodType)
        {
            switch (foodType)
            {
                case FoodType.Water:
                    UpdateStat(item, "Sede", "Sede ~g~satisfeita~s~.", 0f);
                    break;
                case FoodType.Food:
                    UpdateStat(item, "Fome", "Fome ~g~satisfeita~s~.", 0f);
                    break;
                case FoodType.SpecialFood:
                    UpdateStat(item, "Fome", "Fome ~g~satisfeita~s~.", 0f);
                    UpdateStat(item, "Sede", "Sede ~g~satisfeita~s~.", 0.15f);
                    break;
            }
        }

        private void UpdateStat(
            IFood item,
            string statName,
            string notificationMessage,
            float valueOverride = 0f
        )
        {
            // Encontra o item de exibição da estatística pelo nome.
            StatDisplayItem statDisplayItem = _statDisplay.Find(displayItem =>
                displayItem.Stat.Name == statName
            );
            statDisplayItem.Stat.Value += (
                (valueOverride <= 0f) ? item.RestorationAmount : valueOverride
            );
            statDisplayItem.Stat.Sustained = true;
            Notification.Show(notificationMessage, true);

            if (statDisplayItem.Stat.Value > statDisplayItem.Stat.MaxVal)
            {
                statDisplayItem.Stat.Value = statDisplayItem.Stat.MaxVal;
            }
        }

        private static Ped PlayerPed
        {
            get { return Database.PlayerPed; }
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (Database.PlayerIsDead)
            {
                // Restaura os valores máximos para todas as estatísticas se o jogador estiver morto
                foreach (var statDisplayItem in _statDisplay)
                {
                    statDisplayItem.Stat.Value = statDisplayItem.Stat.MaxVal;
                }
                return;
            }

            if (!UseStats)
            {
                // Remove o display se as estatísticas não estiverem sendo usadas
                if (!_removedDisplay)
                {
                    foreach (var statDisplayItem2 in _statDisplay)
                    {
                        MenuController.TimerBars.Remove(statDisplayItem2.Bar);
                    }
                    _removedDisplay = true;
                }
                return;
            }

            // Adiciona o display de volta se necessário
            if (_removedDisplay)
            {
                foreach (var statDisplayItem3 in _statDisplay)
                {
                    MenuController.TimerBars.Add(statDisplayItem3.Bar);
                }
                _removedDisplay = false;
            }

            int i = 0;
            int count = _statDisplay.Count;
            // Atualiza as estatísticas
            foreach (var statDisplayItem in _statDisplay)
            {
                Stat stat = statDisplayItem.Stat;
                statDisplayItem.Bar.Progress = stat.Value;

                // Gerencia as estatísticas específicas
                HandleReductionStat(
                    stat,
                    "Fome",
                    "Você está ~r~morrendo de fome~s~!",
                    _hungerReductionMultiplier,
                    ref _hungerDamageTimer,
                    ref _hungerSustainTimer
                );

                HandleReductionStat(
                    stat,
                    "Sede",
                    "Você está ~r~desidratado~s~!",
                    _thirstReductionMultiplier,
                    ref _thirstDamageTimer,
                    ref _thirstSustainTimer
                );

                HandleStamina(stat);
            }
        }

        private void HandleStamina(Stat stat)
        {
            if (stat.Name != "Stamina")
                return;

            if (stat.Sustained)
            {
                if (Database.PlayerIsSprinting)
                {
                    // Reduz a resistência ao correr
                    if (stat.Value > 0f)
                    {
                        stat.Value -= Game.LastFrameTime * _sprintReductionMultiplier;
                    }
                    else
                    {
                        // Se a resistência acabar, desativa a sustentação e zera o valor
                        stat.Sustained = false;
                        stat.Value = 0f;
                    }
                }
                else
                {
                    // Recupera a resistência enquanto o jogador não está correndo
                    if (stat.Value < stat.MaxVal)
                    {
                        stat.Value += Game.LastFrameTime * (_sprintReductionMultiplier * 10f);
                    }
                    else
                    {
                        stat.Value = stat.MaxVal;
                    }
                }
            }
            else
            {
                // Desabilita o controle de sprint enquanto a resistência não está sustentada
                Game.DisableControlThisFrame(Control.Sprint);
                stat.Value += Game.LastFrameTime * _sprintReductionMultiplier;

                // Restaura a sustentação quando a resistência é suficiente
                if (stat.Value >= stat.MaxVal * 0.3f)
                {
                    stat.Sustained = true;
                }
            }
        }

        private void HandleReductionStat(
            Stat stat,
            string targetName,
            string notification,
            float reductionMultiplier,
            ref float damageTimer,
            ref float sustainTimer
        )
        {
            // Verifica se o nome do Stat corresponde ao nome alvo
            if (stat.Name != targetName)
                return;

            if (!stat.Sustained)
            {
                // Reduz o valor do Stat se ele for maior que zero
                if (stat.Value > 0f)
                {
                    stat.Value -= Game.LastFrameTime * reductionMultiplier;
                    damageTimer = _statDamageInterval; // Reinicia o cronômetro de dano
                }
                else
                {
                    // Exibe notificação quando o Stat atinge zero
                    Notification.Show(notification);
                    damageTimer += Game.LastFrameTime;

                    // Aplica dano ao jogador se o intervalo de dano for atingido
                    if (damageTimer >= _statDamageInterval)
                    {
                        PlayerPed.ApplyDamage(Database.Random.Next(3, 15));
                        damageTimer = 0f;
                    }
                    stat.Value = 0f; // Garante que o Stat não fique abaixo de zero
                }
            }
            else
            {
                // Quando o Stat está sustentado, atualiza o cronômetro de sustentação
                damageTimer = _statDamageInterval;
                sustainTimer += Game.LastFrameTime;

                // Desativa a sustentação após atingir o tempo limite
                if (sustainTimer > _statSustainLength)
                {
                    sustainTimer = 0f;
                    stat.Sustained = false;
                }
            }
        }
    }
}
