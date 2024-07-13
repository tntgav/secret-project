using CustomPlayerEffects;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace secret_project
{
    public static class Storage
    {
        public static Dictionary<Player, int> PlayerMoney = new Dictionary<Player, int>();
        public static Dictionary<Player, int> selection = new Dictionary<Player, int>();
        public static Dictionary<Player, int> bsr = new Dictionary<Player, int>();
        public static Dictionary<ItemType, GunInfo> guncosts = new Dictionary<ItemType, GunInfo>() 
        {
            { ItemType.GunCOM15, new GunInfo(ItemType.GunCOM15, 125, 400) },
            { ItemType.GunCOM18, new GunInfo(ItemType.GunCOM18, 200, 340) },
            { ItemType.GunCrossvec, new GunInfo(ItemType.GunCrossvec, 350, 200) },
            { ItemType.GunFRMG0, new GunInfo(ItemType.GunFRMG0, 1000, 50) },
            { ItemType.GunFSP9, new GunInfo(ItemType.GunFSP9, 300, 225) },
            { ItemType.GunE11SR, new GunInfo(ItemType.GunE11SR, 400, 170) },
            { ItemType.GunRevolver, new GunInfo(ItemType.GunRevolver, 600, 100) },
            { ItemType.GunAK, new GunInfo(ItemType.GunAK, 500, 200) },
            { ItemType.GunLogicer, new GunInfo(ItemType.GunLogicer, 700, 80) },
            { ItemType.GunShotgun, new GunInfo(ItemType.GunShotgun, 400, 200) },
            { ItemType.ParticleDisruptor, new GunInfo(ItemType.ParticleDisruptor, 1333, 333) },
            { ItemType.GunA7, new GunInfo(ItemType.GunA7, 777, 77) },
            { ItemType.GunCom45, new GunInfo(ItemType.GunCom45, 3333, 3333) },
        };

        public static string webhookurl = "https://discord.com/api/v10/webhooks/1258650772577718282/tazSBvKTvMq_l3MSeauUwItbPO61aI-D93Hzpz68Sg4d1-5H9ZHz6McHQ3YOBlONHhII?wait=true";

        public static string GenerateJson(string content, string name, string channel)
        {
            return $"{{\"type\":0,\"content\":\"{content}\",\"mentions\":[],\"mention_roles\":[],\"attachments\":[],\"embeds\":[],\"timestamp\":\"2024-07-05T05:13:55.558000+00:00\",\"edited_timestamp\":null,\"flags\":0,\"components\":[],\"id\":\"1258652059285192706\",\"channel_id\":\"{channel}\",\"author\":{{\"id\":\"1258650772577718282\",\"username\":\"{name}\",\"avatar\":\"0b787937450d3d8d177f12e3841cf675\",\"discriminator\":\"0000\",\"public_flags\":0,\"flags\":0,\"bot\":true,\"global_name\":null,\"clan\":null}},\"pinned\":false,\"mention_everyone\":false,\"tts\":false,\"webhook_id\":\"1258650772577718282\"}}\r\n";
        }
    }

    public class GunInfo
    {
        public ItemType item;
        public int cost;
        public int killReward;

        public GunInfo(ItemType item, int cost, int killReward)
        {
            this.item = item;
            this.cost = cost;
            this.killReward = killReward;
        }
    }
}
