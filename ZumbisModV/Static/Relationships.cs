using System;
using GTA;
using GTA.Native;

namespace ZumbisModV.Static
{
    public static class Relationships
    {
        public static RelationshipGroup InfectedRelationship;
        public static RelationshipGroup FriendlyRelationship;
        public static RelationshipGroup MilitiaRelationship;
        public static RelationshipGroup HostileRelationship;
        public static RelationshipGroup PlayerRelationship;

        public static void SetRelationships()
        {
            // Adicionando os grupos de relacionamento
            InfectedRelationship = World.AddRelationshipGroup("Zombie");
            FriendlyRelationship = World.AddRelationshipGroup("Friendly");
            MilitiaRelationship = World.AddRelationshipGroup("Private_Militia");
            HostileRelationship = World.AddRelationshipGroup("Hostile");

            // Obtendo o grupo de relacionamento do jogador
            PlayerRelationship = Database.PlayerPed.RelationshipGroup;

            // Definindo os relacionamentos entre os grupos
            SetRelationshipBothWays(InfectedRelationship, Relationship.Hate, FriendlyRelationship);
            SetRelationshipBothWays(InfectedRelationship, Relationship.Hate, MilitiaRelationship);
            SetRelationshipBothWays(InfectedRelationship, Relationship.Hate, HostileRelationship);
            SetRelationshipBothWays(InfectedRelationship, Relationship.Hate, PlayerRelationship);

            SetRelationshipBothWays(FriendlyRelationship, Relationship.Hate, MilitiaRelationship);
            SetRelationshipBothWays(FriendlyRelationship, Relationship.Hate, HostileRelationship);
            SetRelationshipBothWays(HostileRelationship, Relationship.Hate, MilitiaRelationship);
            SetRelationshipBothWays(HostileRelationship, Relationship.Hate, PlayerRelationship);
            SetRelationshipBothWays(PlayerRelationship, Relationship.Hate, MilitiaRelationship);

            SetRelationshipBothWays(PlayerRelationship, Relationship.Like, FriendlyRelationship);

            // Fazendo o jogador ser prioridade para os inimigos
            Database.PlayerPed.IsPriorityTargetForEnemies = true;
        }

        public static void SetRelationshipBothWays(
            RelationshipGroup group1,
            Relationship relationship,
            RelationshipGroup group2
        )
        {
            // Usando o Function.Call para definir o relacionamento entre os grupos
            Function.Call(
                Hash.SET_RELATIONSHIP_BETWEEN_GROUPS,
                (int)relationship,
                group1.Hash,
                group2.Hash
            );
            Function.Call(
                Hash.SET_RELATIONSHIP_BETWEEN_GROUPS,
                (int)relationship,
                group2.Hash,
                group1.Hash
            );
        }
    }
}
