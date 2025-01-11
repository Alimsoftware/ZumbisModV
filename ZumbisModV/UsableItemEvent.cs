using System;

namespace ZumbisModV
{
    [Serializable]
    public class UsableItemEvent
    {
        public ItemEvent Event;
        public object EventArgument;

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="UsableItemEvent"/>.
        /// </summary>
        /// <param name="event">O evento associado ao item.</param>
        /// <param name="eventArgument">O argumento específico do evento associado ao item.</param>

        public UsableItemEvent(ItemEvent @event, object eventArgument)
        {
            Event = @event;
            EventArgument = eventArgument;
        }
    }
}
