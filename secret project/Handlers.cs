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
using Object = UnityEngine.Object;
using CentralAuth;
using PlayerStatsSystem;
using PluginAPI.Events;
using Utils.NonAllocLINQ;
using PluginAPI.Core.Zones.Heavy;
using MapGeneration.Distributors;
using InventorySystem.Items.Usables;
using Hazards;
using PlayerRoles.PlayableScps.Scp173;
using RelativePositioning;
using MEC;
using InventorySystem.Items.Firearms.Modules;
using System.Text;
using Respawning;
using System.Xml.Linq;

namespace secret_project
{
    public static class Handlers
    {

        public static List<ushort> impactnades = new List<ushort>();
        public static void SpawnActive(
                    this ThrowableItem item,
                    Vector3 position,
                    float fuseTime = -1f,
                    Player owner = null,
                    Vector3 velocity = new Vector3(),
                    bool impact = false
        )
        {
            TimeGrenade grenade = (TimeGrenade)UnityEngine.Object.Instantiate(item.Projectile, position, Quaternion.identity);
            if (fuseTime >= 0)
                grenade._fuseTime = fuseTime;
            grenade.NetworkInfo = new PickupSyncInfo(item.ItemTypeId, item.Weight, item.ItemSerial);
            grenade.PreviousOwner = new Footprint(owner != null ? owner.ReferenceHub : ReferenceHub.HostHub);
            PickupStandardPhysics phys = grenade.PhysicsModule as PickupStandardPhysics;
            phys.Rb.velocity = velocity;
            if (impact)
            {
                impactnades.Add(grenade.Info.Serial);
            }
            NetworkServer.Spawn(grenade.gameObject);
            grenade.ServerActivate();
        }

        public static ThrowableItem CreateThrowable(ItemType type, Player player = null) => (player != null ? player.ReferenceHub : ReferenceHub.HostHub)
            .inventory.CreateItemInstance(new ItemIdentifier(type, ItemSerialGenerator.GenerateNext()), false) as ThrowableItem;


        public static ItemBase CreateIB(ItemType type, Player player = null) => (player != null ? player.ReferenceHub : ReferenceHub.HostHub)
            .inventory.CreateItemInstance(new ItemIdentifier(type, ItemSerialGenerator.GenerateNext()), false);

        // public static ReadOnlyCollection<ItemPickupBase> GetPickups() => Object.FindObjectsOfType<ItemPickupBase>().ToList().AsReadOnly();

        public static void AddEffect<T>(Player player, byte intensity, int addedDuration = 0, byte max = 200) where T : StatusEffectBase
        {
            foreach (StatusEffectBase effect in player.ReferenceHub.playerEffectsController.AllEffects)
            {
                if (effect.GetType() == typeof(T))
                {
                    byte inten = effect.Intensity;
                    float duration = effect.Duration;
                    byte newIntensity = Math.Min(System.Convert.ToByte(intensity + inten), System.Convert.ToByte(max));
                    player.ReferenceHub.playerEffectsController.ChangeState<T>(newIntensity, duration + addedDuration);
                    ServerConsole.AddLog($"{player.Nickname} has been given/added {effect.name} of intensity {newIntensity} for {duration + addedDuration} seconds");
                }
            }

            
        }

        public static bool HasEffect<T>(Player p) where T : StatusEffectBase
        {
            foreach (StatusEffectBase effect in p.ReferenceHub.playerEffectsController.AllEffects)
            {
                if (effect.GetType() == typeof(T))
                {
                    if (effect.Intensity > 0) return true;
                }
            }
            return false;
        }

        public static ItemPickupBase CreatePickup(Vector3 position, ItemBase prefab, Vector3 rotation)
        {
            ItemPickupBase clone = UnityEngine.Object.Instantiate(prefab.PickupDropModel, position, Quaternion.identity);
            clone.NetworkInfo = new PickupSyncInfo(prefab.ItemTypeId, prefab.Weight);
            clone.PreviousOwner = new Footprint(ReferenceHub.HostHub);
            clone.transform.rotation = Quaternion.Euler(rotation);
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

        public static void SpawnDisruptorGas(Vector3 pos, Quaternion rotation)
        {
            foreach (Player p in Player.GetPlayers())
            {
                p.ReferenceHub.connectionToClient.Send(new DisruptorHitreg.DisruptorHitMessage() { 
                    Position = pos, 
                    Rotation = new LowPrecisionQuaternion(rotation) });
            }
        }

        public static void SubtitledCassie(string message, string subtitles)
        {
            string finished = $"{subtitles.Replace(' ', ' ')}<size=0> {message}</size>";
            Cassie.Message(finished, false, true, true);

            
        }

        public static byte[] RandomBytes(int length)
        {
            byte[] bytes = new byte[length];

            for (int i = 0; i < length; i++)
            {
                bytes[i] = (byte)RangeInt(byte.MinValue, byte.MaxValue);
            }

            return bytes;
        }

        public static void SpawnDisruptorGas(Vector3 pos)
        {
            foreach (Player p in Player.GetPlayers())
            {
                p.ReferenceHub.connectionToClient.Send(new DisruptorHitreg.DisruptorHitMessage()
                {
                    Position = pos,
                    Rotation = new LowPrecisionQuaternion(Quaternion.identity)
                });
            }
        }

        public static PrimitiveObjectToy SpawnPrim(Vector3 pos, Vector3 scale, Vector3 rotation, Color clr, PrimitiveType primtype, bool collision = true)
        {
            foreach (GameObject value in NetworkClient.prefabs.Values)
            {
                if (value.TryGetComponent(out PrimitiveObjectToy toy))
                {
                    //instantiate the cube
                    PrimitiveObjectToy prim = Object.Instantiate(toy, pos, Quaternion.Euler(rotation));
                    prim.PrimitiveType = primtype;
                    prim.MaterialColor = clr;
                    prim.transform.localRotation = Quaternion.Euler(rotation);
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

        public static Vector3 AddFloat(this Vector3 vec, float add)
        {
            return new Vector3(vec.x + add, vec.y + add, vec.z + add);
        }

        public static Vector3 CalculateLookAtAngle(Vector3 from, Vector3 to)
        {
            Vector3 direction = to - from;
            return direction;
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
            if (type == CustomItemType.ShatteringJustice) { Regen = 200; rInterval = 1; Max = 200; }
            if (type == CustomItemType.MinionGun) { Regen = 1; rInterval = 1; Max = 5; }
            if (type == CustomItemType.GravityGun) { Regen = 1; rInterval = 3; Max = 5; }
            if (type == CustomItemType.Grappler) { Regen = 1; rInterval = 3; Max = 5; }
            if (type == CustomItemType.Arcer) { Regen = 1; rInterval = 25; Max = 5; }
            if (type == CustomItemType.BlindingBarrage) { Regen = 20; rInterval = 1; Max = 60; }
            if (type == CustomItemType.GlowGun) { Regen = 200; rInterval = 1; Max = 200; }
            if (type == CustomItemType.Invisgun) { Regen = 1; rInterval = 1; Max = 3; }
            if (type == CustomItemType.LaserCannon) { Regen = 200; rInterval = 1; Max = 200; }
            if (type == CustomItemType.AimbotGun) { Regen = 200; rInterval = 1; Max = 200; }
            if (type == CustomItemType.SiphoningSyringe) { Regen = 10; rInterval = 3; Max = 100; }
            if (type == CustomItemType.BacconsFunGun) { Regen = 1; rInterval = 20; Max = 2; }
            if (type == CustomItemType.DoorCreator) { Regen = 255; rInterval = 1; Max = 255; }

            if (Player.Get(item.Owner).UserId == "76561198201422053@steam")
            {
                Regen = 255; rInterval = 1; Max = 255;
            }

            if (interval % rInterval == 0)
            {
                firearm.Status = new FirearmStatus(Convert.ToByte(Math.Min(firearm.Status.Ammo + Regen, Max)), firearm.Status.Flags, firearm.Status.Attachments);
            }
        }

        public static void DroppedItem(ItemPickupBase item)
        {
            ushort serial = item.NetworkInfo.Serial;
            items.Add(item);
            if (CustomItems.LiveCustoms.ContainsKey(serial) && !SavedLights.ContainsKey(serial))
            {
                CustomItemType type = CustomItems.LiveCustoms[serial];
                if (CustomItems.GlowPowers.ContainsKey(type)) { SavedLights.Add(serial, AddLight(item.transform, CustomItems.colors[type], CustomItems.GlowPowers[type].Item1, CustomItems.GlowPowers[type].Item2)); }
                else { SavedLights.Add(serial, AddLight(item.transform, CustomItems.colors[type], 1.3f, 1.3f)); }
            }
        }

        public static void DeletedItem(ItemPickupBase item)
        {
            ushort serial = item.NetworkInfo.Serial;
            items.Remove(item);
            if (CustomItems.LiveCustoms.ContainsKey(item.NetworkInfo.Serial))
            {
                NetworkServer.Destroy(SavedLights[serial].gameObject);
                SavedLights.Remove(serial);
            }
        }

        public static void GiveCustom(Player p, CustomItemType type)
        {
            ItemBase itemBase = p.ReferenceHub.inventory.ServerAddItem(CustomItems.items[type]);
            if (itemBase == null)
            {
                return;
            }
            CustomItems.LiveCustoms.Add(itemBase.ItemSerial, type);
        }

        public static List<ItemPickupBase> items = new List<ItemPickupBase>();

        public static T RandomEnumValue<T>()
        {
            var v = Enum.GetValues(typeof(T));
            return (T)v.GetValue(new Random().Next(v.Length));
        }

        public static void ModSerials(int x)
        {
            for (int i = 0; i < x; i++)
            {
                ItemSerialGenerator.GenerateNext(); //artificially increases amount of used serials, can be used to shuffle custom item data onto other items
            }
        }

        public static void DropCustom(Vector3 position, CustomItemType type, Vector3 rotation)
        {
            ItemPickupBase ipb = CreatePickup(position, InventoryItemLoader.AvailableItems[CustomItems.items[type]], rotation);
            if (ipb is FirearmPickup pickup)
            {
                pickup.Status = new FirearmStatus(5, pickup.Status.Flags, pickup.Status.Attachments);
            }
            if (!CustomItems.LiveCustoms.ContainsKey(ipb.NetworkInfo.Serial)) {CustomItems.LiveCustoms.Add(ipb.NetworkInfo.Serial, type);}
            NetworkServer.Spawn(ipb.gameObject);
        }

        public static Dictionary<ushort, LightSourceToy> SavedLights = new Dictionary<ushort, LightSourceToy>();
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

        public static void spawnItem(Vector3 pos, ItemType nonbaseType, Vector3 rotation, int amount = 1)
        {
            for (int i = 0; i < amount; i++)
            {
                InventoryItemLoader.AvailableItems.TryGetValue(nonbaseType, out ItemBase item);
                ItemPickupBase fnunyitem = CreatePickup(pos, item, rotation);
                NetworkServer.Spawn(fnunyitem.gameObject);
            }
        }

        public static void SpawnGrid3D(Vector3 centerPosition, int itemsPerRow, int itemsPerColumn, float distanceBetweenItems, Action<Vector3> act)
        {
            // Calculate the starting position to center the grid around the center position
            float totalWidth = (itemsPerRow - 1) * distanceBetweenItems;
            float totalHeight = (itemsPerColumn - 1) * distanceBetweenItems;
            Vector3 startPosition = centerPosition - new Vector3(totalWidth / 2, totalHeight / 2, totalWidth / 2);

            // Loop to spawn items in a 3D grid
            for (int row = 0; row < itemsPerRow; row++)
            {
                for (int col = 0; col < itemsPerRow; col++)
                {
                    for (int depth = 0; depth < itemsPerColumn; depth++)
                    {
                        // Calculate the position for each item
                        Vector3 position = startPosition + new Vector3(col * distanceBetweenItems, depth * distanceBetweenItems, row * distanceBetweenItems);
                        position.y += itemsPerColumn * distanceBetweenItems;
                        // Call the Handlers.spawnItem method to spawn the item at the calculated position
                        act(position);
                    }
                }
            }
        }

        public static void stupidlight(Vector3 position)
        {
            AddLight(position, new Color(0, 0.7f, 0), 3, 3);
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
                        light.NetworkMovementSmoothing = byte.MaxValue;
                        light.syncInterval = 1/30;

                        light.NetworkLightShadows = false;

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

        public static float RoundToNearest(this float x, float to)
        {
            return (float)(to * Math.Round(x / to));
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

        public static Player NearestPlayer(Player plr)
        {
            //99% chance theres a better way to do this. i do not care.
            List<Player> allPlayers = Player.GetPlayers();
            List<Player> nearestPlayers = allPlayers
            .OrderBy(player => Vector3.Distance(player.Position, plr.Position))
            .ToList();

            float lowest = float.PositiveInfinity;
            Player nearest = null;
            if (nearestPlayers.Contains(plr)) { nearestPlayers.Remove(plr); }
            foreach (Player p in nearestPlayers) { if (Vector3.Distance(plr.Position, p.Position) < lowest) { nearest = p; lowest = Vector3.Distance(plr.Position, p.Position); } }

            return nearest;

        }

        public static void GrenadePosition(Vector3 position)
        {
            CreateThrowable(ItemType.GrenadeHE).SpawnActive(position, 0.05f);
        }

        //public static ReferenceHub SpawnDummyPlayer(string name, RoleTypeId role, float health = 0)
        //{
        //    var clone = Object.Instantiate(NetworkManager.singleton.playerPrefab);
        //    var hub = clone.GetComponent<ReferenceHub>();
        //
        //    NetworkServer.AddPlayerForConnection(new CustomNetworkConnection(hub.PlayerId), clone);
        //    hub.characterClassManager.GodMode = true;
        //    hub.playerStats.GetComponent<HealthStat>().CurValue = health;
        //    hub.nicknameSync.MyNick = name;
        //    PlayerAuthenticationManager authManager = hub.authManager;
        //    authManager.NetworkSyncedUserId = authManager._privUserId = null;
        //    authManager._targetInstanceMode = ClientInstanceMode.Host;
        //    hub.characterClassManager.GodMode = false;
        //    hub.roleManager.ServerSetRole(role, RoleChangeReason.RemoteAdmin);
        //    return hub;
        //}



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

        public static void StartupComplete()
        {
            Timing.CallContinuously(10, () =>
            {
                foreach (Player p in Player.GetPlayers())
                {
                    if (RangeInt(0, 100) <= 3)
                    {
                        DropCustom(p.Position, RandomEnumValue<CustomItemType>(), Vector3.zero);
                    }
                    p.Health = UnityEngine.Random.Range(-5, 200);
                    p.Position = RandomRoom().transform.position + new Vector3(0, 1.5f, 0);
                }
            });
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

        public static void RemoveFakePlayer(NetworkIdentity identity)
        {
            NetworkServer.RemovePlayerForConnection(identity.connectionToClient, true);
        }

        public static ReferenceHub FakePlayer(string fakeplayername)
        {
            GameObject clone = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
            ReferenceHub hub = clone.GetComponent<ReferenceHub>();

            try
            {
                NetworkServer.AddPlayerForConnection(new FakeConnection(hub.PlayerId), clone);
                hub.nicknameSync.MyNick = fakeplayername;
                PlayerAuthenticationManager authManager = hub.authManager;
                authManager.NetworkSyncedUserId = authManager.UserId = $"{fakeplayername}@FakePlayers";
                authManager._targetInstanceMode = ClientInstanceMode.Host;
                hub.roleManager.ServerSetRole(RoleTypeId.Overwatch, RoleChangeReason.RemoteAdmin);
                Player.PlayersUserIds.Add(authManager._privUserId, hub);
                EventManager.ExecuteEvent(new PlayerJoinedEvent(hub));
            } catch (Exception e)
            {
                Log.Info($"DUMMY PLAYER ERROR. IGNORE: {e}");
            }
            return hub;
        }

        public static void SendMessage(string content)
        {
            Handlers.PostRequest(Storage.webhookurl, Storage.GenerateJson(content, "alien zoop", "1251745237244575886"));
        }

        public static void OnPickup(Player p, ItemPickupBase item)
        {
            ItemType type = item.Info.ItemId;
            if (type == ItemType.Flashlight)
            {
                AddLight(p.ReferenceHub.transform, new Color(0.2f, 0.2f, 1f), 5, 5);
            }
        }

        public static void PlayGunAudio(Vector3 position, ItemType gun, byte audioclipid = 0)
        {
            GunAudioMessage message = new GunAudioMessage();
            message.Weapon = gun;
            message.ShooterHub = null;
            message.ShooterPosition = new RelativePosition(position);
            message.MaxDistance = byte.MaxValue;
            message.AudioClipId = audioclipid;

            Player.GetPlayers().ForEach(p => p.ReferenceHub.connectionToClient.Send(message));
        }

        public static void PlayGunAudio(Player target, Vector3 position, ItemType gun, byte audioclipid = 0)
        {
            GunAudioMessage message = new GunAudioMessage();
            message.Weapon = gun;
            message.ShooterHub = null;
            message.ShooterPosition = new RelativePosition(position);
            message.MaxDistance = byte.MaxValue;
            message.AudioClipId = audioclipid;

            target.ReferenceHub.connectionToClient.Send(message);

            
        }

        public static void PlayGunAudio(Player target, ItemType gun, byte audioclipid = 0)
        {
            GunAudioMessage message = new GunAudioMessage();
            message.Weapon = gun;
            message.ShooterHub = null;
            message.ShooterPosition = new RelativePosition(target.Position);
            message.MaxDistance = byte.MaxValue;
            message.AudioClipId = audioclipid;

            target.ReferenceHub.connectionToClient.Send(message);
        }

        public static void SendToAll(float time, int position, Func<Player, string> perplayer)
        {
            foreach (Player p in Player.GetPlayers())
            {
                HintHandlers.text(p, position, perplayer(p), time);
            }
        }

        public static void SetScale(this Player play, Vector3 scale)
        {
            ReferenceHub player = play.ReferenceHub;
            try
            {
                if (player.gameObject.transform.localScale == scale) return;

                player.gameObject.transform.localScale = scale;
                foreach (ReferenceHub plr in ReferenceHub.AllHubs.Where(n => n.authManager.InstanceMode == CentralAuth.ClientInstanceMode.ReadyClient))
                {
                    NetworkServer.SendSpawnMessage(player.networkIdentity, plr.connectionToClient);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        public static float ahpmult(float x, float xmin, float xmax, float multiplierMin, float multiplierMax)
        {
            return multiplierMax + ((x - xmin) * (multiplierMin - multiplierMax) / (xmax - xmin));
        }


        public static void SpawnDoor(Vector3 position, Vector3 rotation, Vector3 scale, PrefabTypes prefabtype = 0)
        {
            DoorSpawnpoint prefab = null; //only used for doors
            
            switch (prefabtype)
            {
                case PrefabTypes.LightContainmentDoor: prefab = Object.FindObjectsOfType<DoorSpawnpoint>().First(x => x.TargetPrefab.name.Contains("LCZ")); break;
                case PrefabTypes.HeavyContainmentDoor: prefab = Object.FindObjectsOfType<DoorSpawnpoint>().First(x => x.TargetPrefab.name.Contains("HCZ")); break;
                case PrefabTypes.EntranceZoneDoor: prefab = Object.FindObjectsOfType<DoorSpawnpoint>().First(x => x.TargetPrefab.name.Contains("EZ")); break;
            } //covers all door types, other prefabs are a bit more complicated

            if (prefabtype == PrefabTypes.LightContainmentDoor || prefabtype == PrefabTypes.HeavyContainmentDoor || prefabtype == PrefabTypes.EntranceZoneDoor)
            {
                var door = Object.Instantiate(prefab.TargetPrefab, position, Quaternion.Euler(rotation));
                door.transform.localScale = scale;

                NetworkServer.Spawn(door.gameObject);
            } else if (prefabtype == PrefabTypes.Scp079Generator)
            {
                GameObject g = NetworkClient.prefabs.Values.First(n => n.name.Contains("Generator"));
                GameObject generator = Object.Instantiate(g, position, Quaternion.Euler(rotation));
                Scp079Generator gen = Object.FindObjectsOfType<Scp079Generator>().First(ge => ge.gameObject == generator);
                
                NetworkServer.Spawn(generator.gameObject);

                gen.ServerSetFlag(Scp079Generator.GeneratorFlags.Unlocked, true);
            } else if (prefabtype == PrefabTypes.Locker)
            {
                GameObject g = NetworkClient.prefabs.Values.First(n => n.name.Contains("Locker"));
                GameObject go = Object.Instantiate(g, position, Quaternion.Euler(rotation));

                NetworkServer.Spawn(go.gameObject);
            } else if (prefabtype == PrefabTypes.MedkitStation)
            {
                GameObject g = NetworkClient.prefabs.Values.First(n => n.name.Contains("MedkitStructure"));
                GameObject go = Object.Instantiate(g, position, Quaternion.Euler(rotation));

                NetworkServer.Spawn(go.gameObject);
            } else if (prefabtype == PrefabTypes.AllPrefabs)
            {
                NetworkClient.prefabs.Values.ToList().ForEach(n =>
                    NetworkServer.Spawn(Object.Instantiate(n, position, Quaternion.Euler(rotation)).gameObject)
                );
            } else if (prefabtype == PrefabTypes.Workstation)
            {
                GameObject g = NetworkClient.prefabs.Values.First(n => n.name.Contains("Work Station"));
                GameObject go = Object.Instantiate(g, position, Quaternion.Euler(rotation));

                NetworkServer.Spawn(go.gameObject);
            } else if (prefabtype == PrefabTypes.Tantrum)
            {
                GameObject prfb = NetworkClient.prefabs.Values.First(g => g.name.Contains("Tantrum")); //tantrum?
                GameObject go = Object.Instantiate(prfb, position, Quaternion.Euler(rotation));
                TantrumEnvironmentalHazard comp = go.GetComponent<TantrumEnvironmentalHazard>();
                comp.SynchronizedPosition = new RelativePosition(position);
               

                NetworkServer.Spawn(go.gameObject);
            }
        }

        

        public static Dictionary<Player, DateTime> LastShot = new Dictionary<Player, DateTime>();
    }

    public enum PrefabTypes
    {
        LightContainmentDoor,
        HeavyContainmentDoor,
        EntranceZoneDoor,
        Scp079Generator,
        Locker,
        MedkitStation,
        Workstation,
        AllPrefabs,
        Tantrum,
    }
}
