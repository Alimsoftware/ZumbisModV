using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;
using ZumbisModV.Extensions;
using ZumbisModV.Interfaces;
using ZumbisModV.Static;

namespace ZumbisModV.Scripts
{
    public class AnimalSpawner : Script, ISpawner
    {
        public const int MinAnimalsPerSpawn = 3;
        public const int MaxAnimalsPerSpawn = 10;
        public const int RespawnDistance = 200;

        private readonly PedHash[] _possibleAnimals;
        private readonly List<Blip> _spawnBlips;
        private Dictionary<Blip, List<Ped>> _spawnMap;

        public AnimalSpawner()
        {
            // Inicializando os arrays e coleções
            _possibleAnimals = new PedHash[]
            {
                PedHash.Deer,
                PedHash.Boar,
                PedHash.Coyote,
                PedHash.MountainLion,
                PedHash.Panther,
            };

            _spawnBlips = new List<Blip>();
            _spawnMap = new Dictionary<Blip, List<Ped>>();

            // Configurando eventos
            Instance = this;
            Tick += OnTick;
            Aborted += OnAborted;
        }

        public static AnimalSpawner Instance { get; private set; }

        public bool Spawn { get; set; }

        private void OnAborted(object sender, EventArgs e) => Clear();

        private void OnTick(object sender, EventArgs e)
        {
            if (Spawn)
            {
                CreateBlips();
                foreach (var spawnBlip in _spawnBlips)
                {
                    if (!_spawnMap.ContainsKey(spawnBlip))
                    {
                        // Cria pedestres associados ao blip
                        //_spawnMap[spawnBlip] = CreateAnimals(spawnBlip);
                        List<Ped> animals = CreateAnimals(spawnBlip);
                        _spawnMap.Add(spawnBlip, animals);
                    }
                    else
                    {
                        List<Ped> animals2 = _spawnMap[spawnBlip];
                        for (int j = animals2.Count - 1; j >= 0; j--)
                        {
                            Ped animal = animals2[j];
                            if (animal == null)
                            {
                                animals2.Remove(null);
                            }
                            else if (!animal.IsDead && animal.Exists())
                            {
                                continue;
                            }
                            animal.MarkAsNoLongerNeeded();
                            animals2.Remove(animal);
                        }
                    }
                }
                // Filtra o mapa para remover blips sem pedestres e fora do alcance
                _spawnMap = _spawnMap
                    .Where(pair =>
                        pair.Value.Count != 0
                        || pair.Key.Position.VDist(Database.PlayerPosition) <= 200.0
                    )
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
            }
            else
            {
                Clear();
            }
        }

        private List<Ped> CreatsesAnimals(Blip blip)
        {
            List<Ped> animals = new List<Ped>();
            int numAnimals = Database.Random.Next(3, 10);
            PedHash selectedAnimal = _possibleAnimals[
                Database.Random.Next(_possibleAnimals.Length)
            ];

            for (int i = 0; i < numAnimals; i++)
            {
                Vector3 spawnPosition = blip.Position.Around(5f);
                Ped ped = World.CreatePed(selectedAnimal, spawnPosition);

                if (ped.Exists())
                {
                    animals.Add(ped);
                    ped.Task.WanderAround();
                    ped.IsPersistent = true;
                    Relationships.SetRelationshipBothWays(
                        Relationships.PlayerRelationship,
                        Relationship.Hate,
                        ped.RelationshipGroup
                    );
                }
            }
            return animals;
        }

        private List<Ped> CreateAnimals(Blip blip)
        {
            List<Ped> animals = new List<Ped>();
            int num = Database.Random.Next(3, 10);
            PedHash hash = _possibleAnimals[Database.Random.Next(_possibleAnimals.Length)];
            for (int i = 0; i < num; i++)
            {
                Ped animal = World.CreatePed(hash, blip.Position.Around(5f));

                if (animal != null)
                {
                    animals.Add(animal);
                    animal.Task.WanderAround();
                    animal.IsPersistent = true;
                    Relationships.SetRelationshipBothWays(
                        Relationships.PlayerRelationship,
                        Relationship.Hate,
                        animal.RelationshipGroup
                    );
                }
            }
            return animals;
        }

        private void CreateBlips()
        {
            if (_spawnBlips.Count >= Database.AnimalSpawns.Length)
                return;

            foreach (var spawnPoint in Database.AnimalSpawns)
            {
                Blip blip = World.CreateBlip(spawnPoint);
                blip.Sprite = BlipSprite.Hunting;
                blip.Name = "Animais";
                _spawnBlips.Add(blip);
            }
        }

        private void Clear()
        {
            foreach (Blip spawnBlip in _spawnBlips)
            {
                if (_spawnMap.TryGetValue(spawnBlip, out var entities))
                {
                    foreach (Entity entity in entities)
                        entity?.Delete();
                }
                spawnBlip.Delete();
            }
            _spawnBlips.Clear();
            _spawnMap.Clear();
        }
    }
}
