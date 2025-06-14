using System;
using GTA;
using GTA.Native;

namespace ZumbisModV.Extensions
{
    public static class PropExt
    {
        public static void SetStateOfDoor(this Prop prop, bool locked, DoorState heading)
        {
            Function.Call(
                Hash.SET_STATE_OF_CLOSEST_DOOR_OF_TYPE,
                prop.Model.Hash,
                prop.Position.X,
                prop.Position.Y,
                prop.Position.Z,
                locked,
                heading,
                1
            );
        }

        /* public static unsafe bool GettDoorLockState(this Prop prop)
         {
             if (prop == null)
             {
                 throw new ArgumentNullException(nameof(prop), "O objeto Prop não pode ser nulo.");
             }
 
             bool locked = false; // Variável para armazenar o estado de bloqueio da porta
             int heading = 0; // Variável para armazenar a direção (heading) da porta
 
             // Chama a função nativa para obter o estado da porta mais próxima do tipo especificado
             Function.Call(
                 Hash.GET_STATE_OF_CLOSEST_DOOR_OF_TYPE,
                 prop.Model.Hash, // Hash do modelo da porta
                 prop.Position.X, // Coordenada X da posição
                 prop.Position.Y, // Coordenada Y da posição
                 prop.Position.Z, // Coordenada Z da posição
                 &locked, // Ponteiro para armazenar o estado de bloqueio
                 &heading // Ponteiro para armazenar a direção
             );
 
             return locked; // Retorna se a porta está trancada
         }*/

        public static bool GetDoorLockState(this Prop prop)
        {
            if (prop == null)
            {
                throw new ArgumentNullException(nameof(prop), "O objeto Prop não pode ser nulo.");
            }
            OutputArgument lockedArg = new OutputArgument();
            OutputArgument headingArg = new OutputArgument();

            Function.Call(
                Hash.GET_STATE_OF_CLOSEST_DOOR_OF_TYPE,
                prop.Model.Hash,
                prop.Position.X,
                prop.Position.Y,
                prop.Position.Z,
                lockedArg,
                headingArg
            );

            return lockedArg.GetResult<bool>();
        }
    }
}
