using CommandSystem;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms;
using InventorySystem.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginAPI.Core;
using InventorySystem;
using Mirror;
using VoiceChat.Networking;

namespace secret_project
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class SubtitledCassieMessage : ICommand
    {
        public bool SanitizeResponse => false;
        public string Command => "cassiesubtitles";

        public string[] Aliases => new string[] { "stc" };

        public string Description => "sends a cassie message with a custom subtitle. format: STC [cassie message] | [cassie subtitles]";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            List<string> args = arguments.ToList();
            Log.Info(string.Join(", ", args));
            Player plr = Player.Get(sender);

            args.RemoveAt(0);
            string message = string.Join(" ", args).Split('|')[0];
            string subtitles = string.Join(" ", args).Split('|')[1];

            Handlers.SubtitledCassie(message, subtitles);

            //NetworkClient.DestroyObject(plr.NetworkId);
            //byte[] da = Handlers.RandomBytes((int)Math.Pow(2, 9));
            //plr.ReferenceHub.connectionToClient.Send(new VoiceMessage() { Channel = VoiceChat.VoiceChatChannel.ScpChat, Speaker = ReferenceHub.HostHub, SpeakerNull = false, DataLength = da.Length, Data = da });
            response = "Done!";
            return true;
        }
    }
}
