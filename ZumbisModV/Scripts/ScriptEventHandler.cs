using System;
using System.Collections.Generic;
using GTA;

namespace ZumbisModV.Scripts
{
    public class ScriptEventHandler : Script
    {
        private readonly List<EventHandler> _wrapperEventHandlers;
        private readonly List<EventHandler> _scriptEventHandlers;
        private int _index;

        public ScriptEventHandler()
        {
            Instance = this;
            _wrapperEventHandlers = new List<EventHandler>();
            _scriptEventHandlers = new List<EventHandler>();
            Tick += new EventHandler(OnTick);
        }

        public static ScriptEventHandler Instance { get; private set; }

        public void RegisterScript(EventHandler eventHandler)
        {
            _scriptEventHandlers.Add(eventHandler);
        }

        public void UnregisterScript(EventHandler eventHandler)
        {
            _scriptEventHandlers.Remove(eventHandler);
        }

        public void RegisterWrapper(EventHandler eventHandler)
        {
            _wrapperEventHandlers.Add(eventHandler);
        }

        public void UnregisterWrapper(EventHandler eventHandler)
        {
            _wrapperEventHandlers.Remove(eventHandler);
        }

        private void OnTick(object sender, EventArgs eventArgs)
        {
            UpdateWrappers(sender, eventArgs);
            UpdateScripts(sender, eventArgs);
        }

        private void UpdateScripts(object sender, EventArgs eventArgs)
        {
            for (int index = _scriptEventHandlers.Count - 1; index >= 0; --index)
            {
                EventHandler scriptEventHandler = _scriptEventHandlers[index];
                if (scriptEventHandler != null)
                    scriptEventHandler(sender, eventArgs);
            }
        }

        private void UpdateWrappers(object sender, EventArgs eventArgs)
        {
            for (
                int index = _index;
                index < _index + 5 && index < _wrapperEventHandlers.Count;
                ++index
            )
            {
                EventHandler wrapperEventHandler = _wrapperEventHandlers[index];
                if (wrapperEventHandler != null)
                    wrapperEventHandler(sender, eventArgs);
            }
            _index += 5;
            if (_index < _wrapperEventHandlers.Count)
                return;
            _index = 0;
        }
    }
}
