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
        public static Dictionary<Player, int> currentGuns = new Dictionary<Player, int>();
        public static List<ItemType> gungamelist = new List<ItemType>
        {
            ItemType.GunLogicer, ItemType.GunFRMG0, ItemType.GunE11SR, ItemType.GunAK,
            ItemType.GunCrossvec, ItemType.GunShotgun, ItemType.GunCom45, ItemType.GunFSP9,
            ItemType.GunA7, ItemType.GunRevolver, ItemType.ParticleDisruptor, ItemType.GunCOM18,
            ItemType.GunCOM15, ItemType.Jailbird
        };
    }
}
