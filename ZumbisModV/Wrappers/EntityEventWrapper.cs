using System;
using System.Collections.Generic;
using GTA;
using ZumbisModV.Scripts;

namespace ZumbisModV.Wrappers
{
    public class EntityEventWrapper : IDisposable
    {
        private static readonly List<EntityEventWrapper> Wrappers = new List<EntityEventWrapper>();
        private bool _isDead;

        // Eventos no padrão .NET
        public event EventHandler<EntityEventArgs> Died;
        public event EventHandler<EntityEventArgs> Aborted;
        public event EventHandler<EntityEventArgs> Updated;
        public event EventHandler<EntityEventArgs> Disposed;

        public Entity Entity { get; }

        public bool IsDead
        {
            get => Entity.IsDead;
            private set
            {
                if (value && !_isDead)
                {
                    Died?.Invoke(this, new EntityEventArgs(Entity));
                }
                _isDead = value;
            }
        }

        public EntityEventWrapper(Entity ent)
        {
            Entity = ent ?? throw new ArgumentNullException(nameof(ent));
            ScriptEventHandler.Instance.RegisterWrapper(OnTick);
            ScriptEventHandler.Instance.Aborted += OnAborted;
            Wrappers.Add(this);
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (Entity == null || !Entity.Exists())
            {
                Dispose();
                return;
            }

            IsDead = Entity.IsDead;
            Updated?.Invoke(this, new EntityEventArgs(Entity));
        }

        private void OnAborted(object sender, EventArgs e)
        {
            Aborted?.Invoke(this, new EntityEventArgs(Entity));
        }

        public void Dispose()
        {
            ScriptEventHandler.Instance.UnregisterWrapper(OnTick);
            ScriptEventHandler.Instance.Aborted -= OnAborted;
            Wrappers.Remove(this);
            Disposed?.Invoke(this, new EntityEventArgs(Entity));
        }

        public static void Dispose(Entity entity)
        {
            var wrapper = Wrappers.Find(w => w.Entity == entity);
            wrapper?.Dispose();
        }
    }
}
