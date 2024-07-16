using Footprinting;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using InventorySystem.Items;
using Mirror;
using PluginAPI.Core;
using System.Collections.ObjectModel;
using UnityEngine;
using CustomPlayerEffects;
using System;
using MapGeneration;
using System.Collections.Generic;
using InventorySystem;
using Utf8Json.Formatters;
using System.Linq;
using PlayerRoles.PlayableScps.Scp079;
using AdminToys;
using PlayerRoles;
using System.Threading.Tasks;
using Interactables.Interobjects.DoorUtils;
using PluginAPI.Core.Items;
using System.Reflection;
using Random = System.Random;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using System.Net;
using System.IO;
using static PlayerList;

namespace secret_project
{
    public static class Handlers
    {
        public static void SpawnActive(
                    this ThrowableItem item,
                    Vector3 position,
                    float fuseTime = -1f,
                    Player owner = null,
                    Vector3 velocity = new Vector3()
        )
        {
            TimeGrenade grenade = (TimeGrenade)UnityEngine.Object.Instantiate(item.Projectile, position, Quaternion.identity);
            if (fuseTime >= 0)
                grenade._fuseTime = fuseTime;
            grenade.NetworkInfo = new PickupSyncInfo(item.ItemTypeId, item.Weight, item.ItemSerial);
            grenade.PreviousOwner = new Footprint(owner != null ? owner.ReferenceHub : ReferenceHub.HostHub);
            PickupStandardPhysics phys = grenade.PhysicsModule as PickupStandardPhysics;
            phys.Rb.velocity = velocity;
            NetworkServer.Spawn(grenade.gameObject);
            grenade.ServerActivate();
        }

        public static ThrowableItem CreateThrowable(ItemType type, Player player = null) => (player != null ? player.ReferenceHub : ReferenceHub.HostHub)
            .inventory.CreateItemInstance(new ItemIdentifier(type, ItemSerialGenerator.GenerateNext()), false) as ThrowableItem;

        // public static ReadOnlyCollection<ItemPickupBase> GetPickups() => Object.FindObjectsOfType<ItemPickupBase>().ToList().AsReadOnly();

        public static void AddEffect<T>(Player player, byte intensity, int addedDuration = 0) where T : StatusEffectBase
        {
            foreach (StatusEffectBase effect in player.ReferenceHub.playerEffectsController.AllEffects)
            {
                if (effect.GetType() == typeof(T))
                {
                    byte inten = effect.Intensity;
                    float duration = effect.Duration;
                    byte newIntensity = Math.Min(System.Convert.ToByte(intensity + inten), System.Convert.ToByte(200));
                    player.ReferenceHub.playerEffectsController.ChangeState<T>(newIntensity, duration + addedDuration);
                    ServerConsole.AddLog($"{player.Nickname} has been given/added {effect.name} of intensity {newIntensity} for {duration + addedDuration} seconds");
                }
            }
        }
        public static ItemPickupBase CreatePickup(Vector3 position, ItemBase prefab)
        {
            ItemPickupBase clone = UnityEngine.Object.Instantiate(prefab.PickupDropModel, position, Quaternion.identity);
            clone.NetworkInfo = new PickupSyncInfo(prefab.ItemTypeId, prefab.Weight);
            clone.PreviousOwner = new Footprint(ReferenceHub.HostHub);
            return clone;
        }

        public static Vector3 CalculateInitialVelocity(Vector3 start, Vector3 end, float gravity)
        {
            Vector3 displacement = end - start;
            Vector3 displacementXZ = new Vector3(displacement.x, 0, displacement.z);

            float time = Mathf.Sqrt(-2 * displacement.y / gravity) + Mathf.Sqrt(2 * (displacement.y + gravity * Mathf.Sqrt(displacementXZ.magnitude)) / gravity);

            Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * displacement.y);
            Vector3 velocityXZ = displacementXZ / time;

            return velocityXZ + velocityY * -Mathf.Sign(gravity);
        }

        public static PrimitiveObjectToy SpawnPrim(Vector3 pos, Vector3 scale, Vector3 rotation, Color clr, PrimitiveType primtype, bool collision = true)
        {
            foreach (GameObject value in NetworkClient.prefabs.Values)
            {
                if (value.TryGetComponent(out PrimitiveObjectToy toy))
                {
                    //instantiate the cube
                    PrimitiveObjectToy prim = UnityEngine.Object.Instantiate(toy, pos, Quaternion.Euler(rotation));
                    prim.PrimitiveType = primtype;
                    prim.MaterialColor = clr;
                    prim.transform.localScale = scale;
                    prim.PrimitiveFlags = PrimitiveFlags.Visible;
                    prim.gameObject.AddComponent<BoxCollider>();
                    prim.GetComponent<BoxCollider>().isTrigger = false;
                    prim.GetComponent<BoxCollider>().center = pos;
                    prim.GetComponent<BoxCollider>().size = scale;
                    prim.GetComponent<BoxCollider>().enabled = true;

                    if (collision) { prim.PrimitiveFlags = PrimitiveFlags.Collidable | PrimitiveFlags.Visible; } else { prim.PrimitiveFlags = PrimitiveFlags.Visible; }

                    NetworkServer.Spawn(prim.gameObject);
                    Log.Info("Object spawned!");
                    return prim;
                }
            }
            return null;
        }



        public static void RegenerateGun(ItemBase item, int interval)
        {
            if (!(item is Firearm firearm)) { return; }
            if (!CustomItems.LiveCustoms.ContainsKey(item.ItemSerial)) { return; }
            CustomItemType type = CustomItems.LiveCustoms[item.ItemSerial];
            float Regen = 0;
            float rInterval = 1;
            float Max = 200;
            if (type == CustomItemType.GrenadeLauncher) { Regen = 1; rInterval = 30; Max = 3; }
            if (type == CustomItemType.MiniNukeLauncher) { Regen = 5; rInterval = 1; Max = 5; }
            if (type == CustomItemType.MitzeyScream) { Regen = 200; rInterval = 1; Max = 200; }
            if (type == CustomItemType.LightningTest) { Regen = 200; rInterval = 1; Max = 200; }
            if (type == CustomItemType.MinionGun) { Regen = 1; rInterval = 1; Max = 5; }
            if (type == CustomItemType.GravityGun) { Regen = 1; rInterval = 3; Max = 5; }
            if (type == CustomItemType.Grappler) { Regen = 1; rInterval = 3; Max = 5; }
            if (type == CustomItemType.Arcer) { Regen = 10; rInterval = 3; Max = 50; }

            if (interval % rInterval == 0)
            {
                firearm.Status = new FirearmStatus(Convert.ToByte(Math.Min(firearm.Status.Ammo + Regen, Max)), firearm.Status.Flags, firearm.Status.Attachments);
            }
        }

        public static void DroppedItem(ItemPickupBase item)
        {
            ushort serial = item.NetworkInfo.Serial;
            if (CustomItems.LiveCustoms.ContainsKey(serial))
            {
                CustomItemType type = CustomItems.LiveCustoms[serial];
                SavedLights.Add(serial, AddLight(item.transform, CustomItems.colors[type], 1.3f, 1.3f));
            }
        }

        public static void DeletedItem(ItemPickupBase item)
        {
            ushort serial = item.NetworkInfo.Serial;
            if (CustomItems.LiveCustoms.ContainsKey(item.NetworkInfo.Serial))
            {
                NetworkServer.Destroy(SavedLights[serial].gameObject);
                SavedLights.Remove(serial);
            }
        }

        private static Dictionary<ushort, LightSourceToy> SavedLights = new Dictionary<ushort, LightSourceToy>();
        public static void OnBulletShot(RaycastHit hit, Player plr)
        {
            if (CustomItems.LiveCustoms.ContainsKey(plr.CurrentItem.ItemSerial))
            {
                CustomItems.UseCustomItem(plr, plr.CurrentItem, CustomItems.LiveCustoms[plr.CurrentItem.ItemSerial], hit.point);
            }
        }

        public static float SpreadCalculation(Player plr)
        {
            float val = (plr.Velocity.magnitude) + ((Storage.bsr[plr] - 1));
            return val < 0 ? 0 : val;
            //return 0;
        }

        public static void spawnItem(Vector3 pos, ItemType nonbaseType, int amount = 1)
        {
            for (int i = 0; i < amount; i++)
            {
                InventoryItemLoader.AvailableItems.TryGetValue(nonbaseType, out ItemBase item);
                ItemPickupBase fnunyitem = Handlers.CreatePickup(pos, item);
                NetworkServer.Spawn(fnunyitem.gameObject);
            }
        }

        public static LightSourceToy AddLight(Vector3 position, Color clr, float range, float intensity)
        {
            LightSourceToy adminToy;
            LightSourceToy light;
            Dictionary<uint, GameObject>.ValueCollection.Enumerator Enumerator = NetworkClient.prefabs.Values.GetEnumerator();
            while (Enumerator.MoveNext())
            {
                if (Enumerator.Current.TryGetComponent(out adminToy))
                {
                    try
                    {
                        light = UnityEngine.Object.Instantiate(adminToy, position, Quaternion.identity);
                        light.LightColor = clr;
                        light.LightRange = range;
                        light.LightIntensity = intensity;

                        NetworkServer.Spawn(light.gameObject);
                        return light;
                    }
                    catch (Exception e)
                    {
                        Log.Error($"{e}");
                    }

                }
            }
            return null;
        }

        public static LightSourceToy AddLight(Transform trans, Color clr, float range, float intensity)
        {
            LightSourceToy adminToy;
            LightSourceToy light;
            Dictionary<uint, GameObject>.ValueCollection.Enumerator Enumerator = NetworkClient.prefabs.Values.GetEnumerator();
            while (Enumerator.MoveNext())
            {
                if (Enumerator.Current.TryGetComponent(out adminToy))
                {
                    try
                    {
                        light = UnityEngine.Object.Instantiate(adminToy, trans);
                        light.LightColor = clr;
                        light.LightRange = range;
                        light.LightIntensity = intensity;

                        NetworkServer.Spawn(light.gameObject);
                        return light;
                    }
                    catch (Exception e)
                    {
                        Log.Error($"{e}");
                    }

                }
            }
            return null;
        }

        public static Dictionary<Player, float> NearestPlayers(Vector3 position, int players = 1)
        {
            //99% chance theres a better way to do this. i do not care.
            List<Player> allPlayers = Player.GetPlayers();
            List<Player> nearestPlayers = allPlayers
            .OrderBy(player => Vector3.Distance(player.Position, position))
            .Take(players)
            .ToList();

            Dictionary<Player, float> Final = new Dictionary<Player, float>();
            foreach (Player player in nearestPlayers)
            {
                Final.Add(player, Vector3.Distance(position, player.Position));
            }
            return Final;

        }

        public static void GrenadePosition(Vector3 position)
        {
            CreateThrowable(ItemType.GrenadeHE).SpawnActive(position, 0.05f);
        }

        public static void GrenadePosition(Vector3 position, int amount)
        {
            for (int i = 0; i < amount;)
            {
                CreateThrowable(ItemType.GrenadeHE).SpawnActive(position, 0.05f);
            }
        }

        public static void GrenadePosition(Vector3 position, Player plr)
        {
            CreateThrowable(ItemType.GrenadeHE, plr).SpawnActive(position, 0.05f, plr);
        }

        public static void GrenadePosition(Vector3 position, int amount, Player plr)
        {
            for (int i = 0; i < amount;)
            {
                CreateThrowable(ItemType.GrenadeHE, plr).SpawnActive(position, 0.05f, plr);
            }
        }

        public static RoomIdentifier NearestRoom(Vector3 pos)
        {
            float lowest = float.PositiveInfinity;
            RoomIdentifier nearest = null;
            foreach (RoomIdentifier room in RoomIdentifier.AllRoomIdentifiers)
            {
                if (room == null) continue;
                if (Vector3.Distance(pos, room.transform.position) < lowest)
                {
                    lowest = Vector3.Distance(pos, room.transform.position);
                    nearest = room;
                }
            }
            return nearest;
        }
        public static int RangeInt(int min, int max)
        {
            return UnityEngine.Random.Range(min, max + 1); //max is exclusive and i want this to be inclusive
        }

        public static void SetEffect<T>(Player player, byte intensity, int addedDuration = 0) where T : StatusEffectBase
        {
            foreach (StatusEffectBase effect in player.ReferenceHub.playerEffectsController.AllEffects)
            {
                if (effect.GetType() == typeof(T))
                {
                    byte inten = effect.Intensity;
                    float duration = effect.Duration;

                    player.ReferenceHub.playerEffectsController.ChangeState<T>(intensity, duration + addedDuration);
                }
            }
        }

        public static void RemoveEffect<T>(Player player) where T : StatusEffectBase
        {
            foreach (StatusEffectBase effect in player.ReferenceHub.playerEffectsController.AllEffects)
            {
                if (effect.GetType() == typeof(T))
                {
                    player.ReferenceHub.playerEffectsController.DisableEffect<T>();
                }
            }
        }

        public static RoomIdentifier RandomRoom()
        {
            List<RoomIdentifier> rooms = new List<RoomIdentifier>();
            foreach (RoomIdentifier room in RoomIdentifier.AllRoomIdentifiers)
            {
                if (room.Zone == FacilityZone.HeavyContainment)
                {
                    rooms.Add(room);
                }
            }
            return rooms[new Random().Next(rooms.Count)];
        }
        public static Vector3 moveRand(Vector3 originalPosition, float distance)
        {
            // Generate a random direction
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere.normalized;

            float randomDistance = UnityEngine.Random.Range(0f, distance);

            // Calculate the new position by moving the original position along the random direction by the random distance
            Vector3 newPosition = originalPosition + randomDirection * randomDistance;

            return newPosition;
        }

        public static void GiveFullGun(ReferenceHub ply, ItemType id)
        {
            ItemBase itemBase = ply.inventory.ServerAddItem(id);
            if (itemBase == null || !(itemBase is Firearm firearm))
                return;

            Dictionary<ItemType, uint> dictionary;
            uint code;
            if (AttachmentsServerHandler.PlayerPreferences.TryGetValue(ply, out dictionary) && dictionary.TryGetValue(itemBase.ItemTypeId, out code))
                firearm.ApplyAttachmentsCode(code, true);
            firearm.Status = new FirearmStatus(firearm.AmmoManagerModule.MaxAmmo, FirearmStatusFlags.MagazineInserted, firearm.GetCurrentAttachmentsCode());
        }

        public static void PostRequest(string url, string json)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }
        }

        public static void SendMessage(string content)
        {
            Handlers.PostRequest(Storage.webhookurl, Storage.GenerateJson(content, "alien zoop", "1251745237244575886"));
        }

        public static string randomdeathmessage()
        {
            List<string> messages = new List<string>
            {
                "learned my secret",
                "died painlessly",
                "died extremely painfully",
                "died happily",
                "called me stupid",
                "has unfinished business",
                "passed out for no reason"
            };

            return messages[new Random().Next(messages.Count)];
        }
    }
}
