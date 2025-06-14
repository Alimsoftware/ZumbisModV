using System;
using GTA;
using LemonUI;
using LemonUI.TimerBars;

namespace ZumbisModV.Static
{
    public class MenuController : Script
    {
        private static ObjectPool _menuPool;
        private static TimerBarCollection _timerBarCollection;

        public MenuController()
        {
            Tick += OnTick;
        }

        public static ObjectPool MenuPool
        {
            get
            {
                if (_menuPool == null)
                {
                    _menuPool = new ObjectPool();
                }
                return _menuPool;
            }
        }

        public static TimerBarCollection TimerBars
        {
            get
            {
                if (_timerBarCollection == null)
                {
                    _timerBarCollection = new TimerBarCollection();
                }
                return _timerBarCollection;
            }
        }

        public void OnTick(object sender, EventArgs eventArgs)
        {
            if (
                _timerBarCollection != null
                && (_menuPool == null || (_menuPool != null && !_menuPool.AreAnyVisible))
            )
            {
                _timerBarCollection?.Process();
            }

            if (_menuPool != null)
            {
                MenuPool?.Process();
            }
        }
    }
}
