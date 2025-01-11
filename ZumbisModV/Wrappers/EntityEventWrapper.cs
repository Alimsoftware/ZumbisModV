using System;
using System.Collections.Generic;
using GTA;
using ZumbisModV.Scripts;

namespace ZumbisModV.Wrappers
{
    public class EntityEventWrapper
    {
        private static readonly List<EntityEventWrapper> Wrappers = new List<EntityEventWrapper>();
        private bool _isDead;

        public event OnDeathEvent Died;
        public event OnWrapperAbortedEvent Aborted;
        public event OnWrapperUpdateEvent Updated;
        public event OnWrapperDisposedEvent Disposed;

        public EntityEventWrapper(Entity ent)
        {
            Entity = ent;
            ScriptEventHandler.Instance.RegisterWrapper(OnTick);
            ScriptEventHandler.Instance.Aborted += (sender, args) => Abort();
            Wrappers.Add(this);
        }

        public Entity Entity { get; }

        public bool IsDead
        {
            get => Entity.IsDead;
            private set
            {
                if (value && !_isDead)
                {
                    //OnDeathEvent died = Died;
                    //if (died != null)
                    //    died(this, Entity);
                    Died?.Invoke(this, Entity);
                }
                _isDead = value;
            }
        }

        public void OnTick(object sender, EventArgs eventArgs)
        {
            // Verifica se a entidade é nula ou não existe mais no jogo
            if (Entity == null || !Entity.Exists())
            {
                // Realiza a limpeza se a entidade não for válida
                Dispose();
                return;
            }
            //else
            //{

            // Atualiza o estado da entidade
            IsDead = Entity.IsDead;

            //EntityEventWrapper.OnWrapperUpdateEvent updated = Updated;
            //if (updated == null)
            //return;
            //updated(this, Entity);

            // Dispara o evento de atualização, se houver inscritos
            Updated?.Invoke(this, Entity);
            //}
        }

        public void Abort()
        {
            //EntityEventWrapper.OnWrapperAbortedEvent aborted = Aborted;
            //if (aborted == null)
            //    return;
            //aborted(this, Entity);
            Aborted?.Invoke(this, Entity);
        }

        public void Dispose()
        {
            ScriptEventHandler.Instance.UnregisterWrapper(OnTick);
            Wrappers.Remove(this);
            Disposed?.Invoke(this, Entity);
        }

        public static void Dispose(Entity entity)
        {
            EntityEventWrapper entityEventWrapper = Wrappers.Find(w => w.Entity == entity);
            entityEventWrapper?.Dispose();
            //Wrappers.Remove(entityEventWrapper);
        }

        public delegate void OnDeathEvent(EntityEventWrapper sender, Entity entity);
        public delegate void OnWrapperAbortedEvent(EntityEventWrapper sender, Entity entity);
        public delegate void OnWrapperUpdateEvent(EntityEventWrapper sender, Entity entity);
        public delegate void OnWrapperDisposedEvent(EntityEventWrapper sender, Entity entity);
    }
}
