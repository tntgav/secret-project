using AdminToys;
using GameCore;
using MapGeneration;
using Mirror;
using PlayerRoles;
using PlayerRoles.RoleAssign;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using secret_project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Log = PluginAPI.Core.Log;

namespace secret_project
{
    public class mapeditorunborn
    {
        [PluginEvent(ServerEventType.RoundStart)]
        public void RoundStart(RoundStartEvent ev)
        {
            Log.Info("starting");
            GeneratePrimitiveEditor();
            Log.Info("generated");
            foreach (Player plr in Player.GetPlayers()) { current.Add(plr, primitiveEditor); }
            Log.Info("added");
            mapspawner.SpawnMap();
            Log.Info("spawned");
        }

        [PluginEvent(ServerEventType.PlayerChangeRadioRange)]
        public bool PlayerChangeRadioRange(PlayerChangeRadioRangeEvent ev)
        {
            //THIS EVENT IS FOR CHANGING OPTIONS
            if (ev.Player.Role != RoleTypeId.Tutorial) return false;
            Check(ev.Player);
            if (current[ev.Player] == null) { return false; }
            index++;
            if (index > current[ev.Player].children.Count - 1 || index < 0) index = 0;
            List<filesystem> listversion = generateListVersion(current[ev.Player]);
            //Log.Info($"index {index}/{current.children.Count - 1}");
            //Log.Info($"name is {listversion[index].name}. ");
            //if (listversion[index].parent != null) { Log.Info($"parent name is {listversion[index].parent.name}"); }
            //HintHandlers.InitPlayer(ev.Player);
            //HintHandlers.AddFadingText(ev.Player, 325, $"{listversion[index].name}", 1f);
            string built = $"<align=left><b>> {current[ev.Player].name}</b>";
            int iter = 0;
            foreach (filesystem file in listversion)
            {
                if (iter == index) { built += $"<br><align=left>>   {file.name}"; iter++; continue; }
                built += $"<br><align=left>    {file.name}";
                iter++;
            }
            HintHandlers.text(ev.Player, 900, built, 1);
            return false; // with new passive system everyone needs a radio
        }

        public static void Check(Player p) { if (!current.ContainsKey(p)) { current.Add(p, primitiveEditor); } }



        public static Dictionary<Player, filesystem> current = new Dictionary<Player, filesystem>();
        public static List<filesystem> listversion = new List<filesystem>();
        public static int index = -1;
        public static Dictionary<Player, PrimitiveObjectToy> currentprim = new Dictionary<Player, PrimitiveObjectToy>();
        public static List<PrimitiveObjectToy> allSpawned = new List<PrimitiveObjectToy>();

        [PluginEvent(ServerEventType.PlayerRadioToggle)]
        public bool PlayerRadioToggle(PlayerRadioToggleEvent ev)
        {
            //THIS EVENT IS FOR GOING WITH THE CURRENT OPTION
            if (ev.Player.Role != RoleTypeId.Tutorial) return false;
            Check(ev.Player);
            if (current[ev.Player] == null) { return false; }
            filesystem realcurrent = generateListVersion(current[ev.Player])[index];
            if (realcurrent.name.StartsWith("."))
            {
                Log.Info($"Going back from {current[ev.Player].name} to {current[ev.Player].parent.name}. resetting index...");
                current[ev.Player] = current[ev.Player].parent;
                index = -1;
            }
            else if (realcurrent.name.StartsWith("_") || realcurrent.name.StartsWith("+") || realcurrent.name.StartsWith("-"))
            {
                if (realcurrent.name == "_confirm" && current[ev.Player].name == "finish")
                {
                    allSpawned.Add(currentprim[ev.Player]);
                    currentprim[ev.Player] = null; //reset for next add
                }

                if (realcurrent.name == "_confirm" && current[ev.Player].name == "duplicate")
                {
                    allSpawned.Add(currentprim[ev.Player]);
                    PrimitiveObjectToy alt = currentprim[ev.Player];
                    currentprim[ev.Player] = Handlers.SpawnPrim(alt.transform.localPosition, alt.transform.localScale, alt.transform.localRotation.eulerAngles, alt.MaterialColor, alt.PrimitiveType);
                }

                if (realcurrent.name == "_confirm" && current[ev.Player].name == "destroy")
                {
                    NetworkServer.Destroy(currentprim[ev.Player].gameObject);
                    currentprim[ev.Player] = null;
                }

                if (realcurrent.name == "_export")
                {
                    string built = "\n";//starting on one line and ending on the other is ugly
                    foreach (PrimitiveObjectToy prim in allSpawned)
                    {
                        if (prim == null) { continue; } //GOD FUCKING DAMN IT I LOST SO MUCH PROGRESS
                        RoomIdentifier roomOffset = Handlers.NearestRoom(prim.transform.position);
                        Vector3 offset = prim.transform.position - roomOffset.transform.position;
                        Vector3 roomangle = roomOffset.transform.rotation.eulerAngles;
                        //roomangle is CORRECT
                        int digits = Handlers.RangeInt(100, 999);
                        //NORMAL ANGLE - CURRENT ANGLE
                        built += ($"\nBuilding exported{prim.netId}{digits} = new Building(rooms[\"{roomOffset.name}\"].transform.position, rooms[\"{roomOffset.name}\"].transform.rotation.eulerAngles - new Vector3({roomangle.x}, {roomangle.y}, {roomangle.z}), Vector3.one, \"EXPORTED{prim.netId}{digits}\");");
                        built += ($"\nexported{prim.netId}{digits}.Add(new Structure(new Vector3({offset.x}f, {offset.y}f, {offset.z}f), new Vector3({prim.transform.localScale.x}f, {prim.transform.localScale.y}f, {prim.transform.localScale.z}f), new Vector3({prim.transform.localRotation.x}f, {prim.transform.localRotation.y}f, {prim.transform.localRotation.z}f), PrimitiveType.{prim.PrimitiveType}, new Color({prim.MaterialColor.r}f, {prim.MaterialColor.g}f, {prim.MaterialColor.b}f, {prim.MaterialColor.a}f), \"exported{prim.netId}{digits}\"));");
                        built += ($"\nexported{prim.netId}{digits}.SpawnBuilding();");
                    }
                    Log.Info(built);
                }
                if (current[ev.Player].name == "create" && !currentprim.ContainsKey(ev.Player)) //cant add while current one isnt finalized
                {
                    switch (realcurrent.name)
                    {
                        case "_cube":
                            currentprim.Add(ev.Player, Handlers.SpawnPrim(ev.Player.Position, Vector3.one, Vector3.zero, Color.white, PrimitiveType.Cube, true)); break;
                        case "_capsule":
                            currentprim.Add(ev.Player, Handlers.SpawnPrim(ev.Player.Position, Vector3.one, Vector3.zero, Color.white, PrimitiveType.Capsule, true)); break;
                        case "_cylinder":
                            currentprim.Add(ev.Player, Handlers.SpawnPrim(ev.Player.Position, Vector3.one, Vector3.zero, Color.white, PrimitiveType.Cylinder, true)); break;
                        case "_sphere":
                            currentprim.Add(ev.Player, Handlers.SpawnPrim(ev.Player.Position, Vector3.one, Vector3.zero, Color.white, PrimitiveType.Sphere, true)); break;
                    }
                }
                if (current[ev.Player].parent.parent != null)
                {
                    if (current[ev.Player].parent.parent.name == "edit")
                    {
                        if (current[ev.Player].parent.name == "scale")
                        {
                            if (current[ev.Player].name == "x")
                            {
                                switch (realcurrent.name)
                                {
                                    case "+0.05":
                                        currentprim[ev.Player].transform.localScale = new Vector3((float)(currentprim[ev.Player].transform.localScale.x + 0.05), currentprim[ev.Player].transform.localScale.y, currentprim[ev.Player].transform.localScale.z); break;
                                    case "-0.05":
                                        currentprim[ev.Player].transform.localScale = new Vector3((float)(currentprim[ev.Player].transform.localScale.x - 0.05), currentprim[ev.Player].transform.localScale.y, currentprim[ev.Player].transform.localScale.z); break;
                                    case "+0.5":
                                        currentprim[ev.Player].transform.localScale = new Vector3((float)(currentprim[ev.Player].transform.localScale.x + 0.5), currentprim[ev.Player].transform.localScale.y, currentprim[ev.Player].transform.localScale.z); break;
                                    case "-0.5":
                                        currentprim[ev.Player].transform.localScale = new Vector3((float)(currentprim[ev.Player].transform.localScale.x - 0.5), currentprim[ev.Player].transform.localScale.y, currentprim[ev.Player].transform.localScale.z); break;

                                }
                            }
                            if (current[ev.Player].name == "y")
                            {
                                switch (realcurrent.name)
                                {
                                    case "+0.05":
                                        currentprim[ev.Player].transform.localScale = new Vector3(currentprim[ev.Player].transform.localScale.x, (float)(currentprim[ev.Player].transform.localScale.y + 0.05), currentprim[ev.Player].transform.localScale.z); break;
                                    case "-0.05":
                                        currentprim[ev.Player].transform.localScale = new Vector3(currentprim[ev.Player].transform.localScale.x, (float)(currentprim[ev.Player].transform.localScale.y - 0.05), currentprim[ev.Player].transform.localScale.z); break;
                                    case "+0.5":
                                        currentprim[ev.Player].transform.localScale = new Vector3(currentprim[ev.Player].transform.localScale.x, (float)(currentprim[ev.Player].transform.localScale.y + 0.5), currentprim[ev.Player].transform.localScale.z); break;
                                    case "-0.5":
                                        currentprim[ev.Player].transform.localScale = new Vector3(currentprim[ev.Player].transform.localScale.x, (float)(currentprim[ev.Player].transform.localScale.y - 0.5), currentprim[ev.Player].transform.localScale.z); break;

                                }
                            }
                            if (current[ev.Player].name == "z")
                            {
                                switch (realcurrent.name)
                                {
                                    case "+0.05":
                                        currentprim[ev.Player].transform.localScale = new Vector3(currentprim[ev.Player].transform.localScale.x, currentprim[ev.Player].transform.localScale.y, (float)(currentprim[ev.Player].transform.localScale.z + 0.05)); break;
                                    case "-0.05":
                                        currentprim[ev.Player].transform.localScale = new Vector3(currentprim[ev.Player].transform.localScale.x, currentprim[ev.Player].transform.localScale.y, (float)(currentprim[ev.Player].transform.localScale.z - 0.05)); break;
                                    case "+0.5":
                                        currentprim[ev.Player].transform.localScale = new Vector3(currentprim[ev.Player].transform.localScale.x, currentprim[ev.Player].transform.localScale.y, (float)(currentprim[ev.Player].transform.localScale.z + 0.5)); break;
                                    case "-0.5":
                                        currentprim[ev.Player].transform.localScale = new Vector3(currentprim[ev.Player].transform.localScale.x, currentprim[ev.Player].transform.localScale.y, (float)(currentprim[ev.Player].transform.localScale.z - 0.5)); break;

                                }
                            }
                        }

                        if (current[ev.Player].parent.name == "rotation")
                        {
                            Vector3 rot = currentprim[ev.Player].transform.localRotation.eulerAngles;
                            if (current[ev.Player].name == "x")
                            {
                                switch (realcurrent.name)
                                {
                                    case "+1":
                                        currentprim[ev.Player].transform.localRotation = Quaternion.Euler(new Vector3(rot.x + 1f, rot.y, rot.z)); break;
                                    case "-1":
                                        currentprim[ev.Player].transform.localRotation = Quaternion.Euler(new Vector3(rot.x - 1f, rot.y, rot.z)); break;
                                    case "+5":
                                        currentprim[ev.Player].transform.localRotation = Quaternion.Euler(new Vector3(rot.x + 5f, rot.y, rot.z)); break;
                                    case "-5":
                                        currentprim[ev.Player].transform.localRotation = Quaternion.Euler(new Vector3(rot.x - 5f, rot.y, rot.z)); break;
                                }
                            }
                            if (current[ev.Player].name == "y")
                            {
                                switch (realcurrent.name)
                                {
                                    case "+1":
                                        currentprim[ev.Player].transform.localRotation = Quaternion.Euler(new Vector3(rot.x, rot.y + 1f, rot.z)); break;
                                    case "-1":
                                        currentprim[ev.Player].transform.localRotation = Quaternion.Euler(new Vector3(rot.x, rot.y - 1f, rot.z)); break;
                                    case "+5":
                                        currentprim[ev.Player].transform.localRotation = Quaternion.Euler(new Vector3(rot.x, rot.y + 5f, rot.z)); break;
                                    case "-5":
                                        currentprim[ev.Player].transform.localRotation = Quaternion.Euler(new Vector3(rot.x, rot.y - 5f, rot.z)); break;

                                }
                            }
                            if (current[ev.Player].name == "z")
                            {
                                switch (realcurrent.name)
                                {
                                    case "+1":
                                        currentprim[ev.Player].transform.localRotation = Quaternion.Euler(new Vector3(rot.x, rot.y, rot.z + 1f)); break;
                                    case "-1":
                                        currentprim[ev.Player].transform.localRotation = Quaternion.Euler(new Vector3(rot.x, rot.y, rot.z - 1f)); break;
                                    case "+5":
                                        currentprim[ev.Player].transform.localRotation = Quaternion.Euler(new Vector3(rot.x, rot.y, rot.z + 5f)); break;
                                    case "-5":
                                        currentprim[ev.Player].transform.localRotation = Quaternion.Euler(new Vector3(rot.x, rot.y, rot.z - 5f)); break;
                                }
                            }
                        }

                        if (current[ev.Player].parent.name == "position")
                        {
                            if (current[ev.Player].name == "x")
                            {
                                switch (realcurrent.name)
                                {
                                    case "+0.05":
                                        currentprim[ev.Player].transform.position = new Vector3((float)(currentprim[ev.Player].transform.position.x + 0.05), currentprim[ev.Player].transform.position.y, currentprim[ev.Player].transform.position.z); break;
                                    case "-0.05":
                                        currentprim[ev.Player].transform.position = new Vector3((float)(currentprim[ev.Player].transform.position.x - 0.05), currentprim[ev.Player].transform.position.y, currentprim[ev.Player].transform.position.z); break;
                                    case "+0.5":
                                        currentprim[ev.Player].transform.position = new Vector3((float)(currentprim[ev.Player].transform.position.x + 0.5), currentprim[ev.Player].transform.position.y, currentprim[ev.Player].transform.position.z); break;
                                    case "-0.5":
                                        currentprim[ev.Player].transform.position = new Vector3((float)(currentprim[ev.Player].transform.position.x - 0.5), currentprim[ev.Player].transform.position.y, currentprim[ev.Player].transform.position.z); break;

                                }
                            }
                            if (current[ev.Player].name == "y")
                            {
                                switch (realcurrent.name)
                                {
                                    case "+0.05":
                                        currentprim[ev.Player].transform.position = new Vector3(currentprim[ev.Player].transform.position.x, (float)(currentprim[ev.Player].transform.position.y + 0.05), currentprim[ev.Player].transform.position.z); break;
                                    case "-0.05":
                                        currentprim[ev.Player].transform.position = new Vector3(currentprim[ev.Player].transform.position.x, (float)(currentprim[ev.Player].transform.position.y - 0.05), currentprim[ev.Player].transform.position.z); break;
                                    case "+0.5":
                                        currentprim[ev.Player].transform.position = new Vector3(currentprim[ev.Player].transform.position.x, (float)(currentprim[ev.Player].transform.position.y + 0.5), currentprim[ev.Player].transform.position.z); break;
                                    case "-0.5":
                                        currentprim[ev.Player].transform.position = new Vector3(currentprim[ev.Player].transform.position.x, (float)(currentprim[ev.Player].transform.position.y - 0.5), currentprim[ev.Player].transform.position.z); break;

                                }
                            }
                            if (current[ev.Player].name == "z")
                            {
                                switch (realcurrent.name)
                                {
                                    case "+0.05":
                                        currentprim[ev.Player].transform.position = new Vector3(currentprim[ev.Player].transform.position.x, currentprim[ev.Player].transform.position.y, (float)(currentprim[ev.Player].transform.position.z + 0.05)); break;
                                    case "-0.05":
                                        currentprim[ev.Player].transform.position = new Vector3(currentprim[ev.Player].transform.position.x, currentprim[ev.Player].transform.position.y, (float)(currentprim[ev.Player].transform.position.z - 0.05)); break;
                                    case "+0.5":
                                        currentprim[ev.Player].transform.position = new Vector3(currentprim[ev.Player].transform.position.x, currentprim[ev.Player].transform.position.y, (float)(currentprim[ev.Player].transform.position.z + 0.5)); break;
                                    case "-0.5":
                                        currentprim[ev.Player].transform.position = new Vector3(currentprim[ev.Player].transform.position.x, currentprim[ev.Player].transform.position.y, (float)(currentprim[ev.Player].transform.position.z - 0.5)); break;

                                }
                            }
                        }
                    }

                    if (current[ev.Player].name == "color")
                    {
                        switch (realcurrent.name)
                        {
                            case "_black": currentprim[ev.Player].NetworkMaterialColor = Color.black; break;
                            case "_blue": currentprim[ev.Player].NetworkMaterialColor = Color.blue; break;
                            case "_clear": currentprim[ev.Player].NetworkMaterialColor = Color.clear; break;
                            case "_cyan": currentprim[ev.Player].NetworkMaterialColor = Color.cyan; break;
                            case "_gray": currentprim[ev.Player].NetworkMaterialColor = Color.gray; break;
                            case "_green": currentprim[ev.Player].NetworkMaterialColor = Color.green; break;
                            case "_magenta": currentprim[ev.Player].NetworkMaterialColor = Color.magenta; break;
                            case "_red": currentprim[ev.Player].NetworkMaterialColor = Color.red; break;
                            case "_white": currentprim[ev.Player].NetworkMaterialColor = Color.white; break;
                            case "_yellow": currentprim[ev.Player].NetworkMaterialColor = Color.yellow; break;
                        }
                    }
                }
            }
            else
            {
                int count = 0;
                foreach (KeyValuePair<string, filesystem> file in current[ev.Player].children)
                {
                    if (count == index) { current[ev.Player] = file.Value; Log.Info($"Going into {file.Value.name} from {current[ev.Player].name}. resetting index..."); index = -1; }
                    count++;
                }
            }
            return false; // with new passive system everyone needs a radio
        }

        public static void GeneratePrimitiveEditor()
        {
            Log.Info("Generating primitive editor properly...");
            primitiveEditor = new filesystem("main");
            primitiveEditor.AddChild(new filesystem("create"));
            primitiveEditor.AddChild(new filesystem("destroy"));
            primitiveEditor.AddChild(new filesystem("edit"));
            primitiveEditor.AddChild(new filesystem("export"));
            primitiveEditor.AddChild(new filesystem("finish"));
            primitiveEditor.AddChild(new filesystem("duplicate"));

            primitiveEditor.children["create"].AddChild(new filesystem("_cube"));
            primitiveEditor.children["create"].AddChild(new filesystem("_capsule"));
            primitiveEditor.children["create"].AddChild(new filesystem("_cylinder"));
            primitiveEditor.children["create"].AddChild(new filesystem("_sphere"));
            primitiveEditor.children["create"].AddChild(new filesystem(".BACK"));

            primitiveEditor.children["destroy"].AddChild(new filesystem("_confirm"));
            primitiveEditor.children["destroy"].AddChild(new filesystem(".BACK"));

            primitiveEditor.children["edit"].AddChild(new filesystem("scale"));
            primitiveEditor.children["edit"].AddChild(new filesystem("position"));
            primitiveEditor.children["edit"].AddChild(new filesystem("rotation"));
            primitiveEditor.children["edit"].AddChild(new filesystem("color"));
            primitiveEditor.children["edit"].AddChild(new filesystem(".BACK"));

            primitiveEditor.children["export"].AddChild(new filesystem("_export"));
            primitiveEditor.children["export"].AddChild(new filesystem(".BACK"));

            primitiveEditor.children["finish"].AddChild(new filesystem("_confirm"));
            primitiveEditor.children["finish"].AddChild(new filesystem(".BACK"));

            primitiveEditor.children["duplicate"].AddChild(new filesystem("_confirm"));
            primitiveEditor.children["duplicate"].AddChild(new filesystem(".BACK"));

            primitiveEditor.children["edit"].children["scale"].AddChild(new filesystem("x"));
            primitiveEditor.children["edit"].children["scale"].AddChild(new filesystem("y"));
            primitiveEditor.children["edit"].children["scale"].AddChild(new filesystem("z"));
            primitiveEditor.children["edit"].children["scale"].AddChild(new filesystem(".BACK"));

            primitiveEditor.children["edit"].children["position"].AddChild(new filesystem("x"));
            primitiveEditor.children["edit"].children["position"].AddChild(new filesystem("y"));
            primitiveEditor.children["edit"].children["position"].AddChild(new filesystem("z"));
            primitiveEditor.children["edit"].children["position"].AddChild(new filesystem(".BACK"));

            primitiveEditor.children["edit"].children["rotation"].AddChild(new filesystem("x"));
            primitiveEditor.children["edit"].children["rotation"].AddChild(new filesystem("y"));
            primitiveEditor.children["edit"].children["rotation"].AddChild(new filesystem("z"));
            primitiveEditor.children["edit"].children["rotation"].AddChild(new filesystem(".BACK"));

            gooningtonthethird("scale");
            gooningtonthethird("position");

            primitiveEditor.children["edit"].children["rotation"].children["x"].AddChild(new filesystem("+1"));
            primitiveEditor.children["edit"].children["rotation"].children["x"].AddChild(new filesystem("-1"));
            primitiveEditor.children["edit"].children["rotation"].children["x"].AddChild(new filesystem("+5"));
            primitiveEditor.children["edit"].children["rotation"].children["x"].AddChild(new filesystem("-5"));
            primitiveEditor.children["edit"].children["rotation"].children["x"].AddChild(new filesystem(".BACK"));

            primitiveEditor.children["edit"].children["rotation"].children["y"].AddChild(new filesystem("+1"));
            primitiveEditor.children["edit"].children["rotation"].children["y"].AddChild(new filesystem("-1"));
            primitiveEditor.children["edit"].children["rotation"].children["y"].AddChild(new filesystem("+5"));
            primitiveEditor.children["edit"].children["rotation"].children["y"].AddChild(new filesystem("-5"));
            primitiveEditor.children["edit"].children["rotation"].children["y"].AddChild(new filesystem(".BACK"));

            primitiveEditor.children["edit"].children["rotation"].children["z"].AddChild(new filesystem("+1"));
            primitiveEditor.children["edit"].children["rotation"].children["z"].AddChild(new filesystem("-1"));
            primitiveEditor.children["edit"].children["rotation"].children["z"].AddChild(new filesystem("+5"));
            primitiveEditor.children["edit"].children["rotation"].children["z"].AddChild(new filesystem("-5"));
            primitiveEditor.children["edit"].children["rotation"].children["z"].AddChild(new filesystem(".BACK"));

            primitiveEditor.children["edit"].children["color"].AddChild(new filesystem("_black"));
            primitiveEditor.children["edit"].children["color"].AddChild(new filesystem("_blue"));
            primitiveEditor.children["edit"].children["color"].AddChild(new filesystem("_clear"));
            primitiveEditor.children["edit"].children["color"].AddChild(new filesystem("_cyan"));
            primitiveEditor.children["edit"].children["color"].AddChild(new filesystem("_gray"));
            primitiveEditor.children["edit"].children["color"].AddChild(new filesystem("_green"));
            primitiveEditor.children["edit"].children["color"].AddChild(new filesystem("_magenta"));
            primitiveEditor.children["edit"].children["color"].AddChild(new filesystem("_red"));
            primitiveEditor.children["edit"].children["color"].AddChild(new filesystem("_white"));
            primitiveEditor.children["edit"].children["color"].AddChild(new filesystem("_yellow"));
            primitiveEditor.children["edit"].children["color"].AddChild(new filesystem(".BACK"));
            Log.Info("finished generating primitive editor");
        }

        public static void gooningtonthethird(string a)
        {
            primitiveEditor.children["edit"].children[a].children["x"].AddChild(new filesystem("+0.05"));
            primitiveEditor.children["edit"].children[a].children["x"].AddChild(new filesystem("-0.05"));
            primitiveEditor.children["edit"].children[a].children["x"].AddChild(new filesystem("+0.5"));
            primitiveEditor.children["edit"].children[a].children["x"].AddChild(new filesystem("-0.5"));
            primitiveEditor.children["edit"].children[a].children["x"].AddChild(new filesystem(".BACK"));

            primitiveEditor.children["edit"].children[a].children["y"].AddChild(new filesystem("+0.05"));
            primitiveEditor.children["edit"].children[a].children["y"].AddChild(new filesystem("-0.05"));
            primitiveEditor.children["edit"].children[a].children["y"].AddChild(new filesystem("+0.5"));
            primitiveEditor.children["edit"].children[a].children["y"].AddChild(new filesystem("-0.5"));
            primitiveEditor.children["edit"].children[a].children["y"].AddChild(new filesystem(".BACK"));

            primitiveEditor.children["edit"].children[a].children["z"].AddChild(new filesystem("+0.05"));
            primitiveEditor.children["edit"].children[a].children["z"].AddChild(new filesystem("-0.05"));
            primitiveEditor.children["edit"].children[a].children["z"].AddChild(new filesystem("+0.5"));
            primitiveEditor.children["edit"].children[a].children["z"].AddChild(new filesystem("-0.5"));
            primitiveEditor.children["edit"].children[a].children["z"].AddChild(new filesystem(".BACK"));
        }

        public static List<filesystem> generateListVersion(filesystem current)
        {
            List<filesystem> listver = new List<filesystem>();
            foreach (KeyValuePair<string, filesystem> file in current.children)
            {
                listver.Add(file.Value);
            }
            return listver;

        }
        public static filesystem primitiveEditor;
    }


    public class Structure
    {
        private Vector3 position;
        private Vector3 scale;
        private Vector3 rotation;
        private PrimitiveType primitiveType;
        private Color color;
        public string name;
        private bool collision;
        private PrimitiveObjectToy prim;

        public Structure(Vector3 position, Vector3 scale, Vector3 rotation, PrimitiveType type, Color color, string name, bool collision = true)
        {
            this.position = position;
            this.scale = scale;
            this.rotation = rotation;
            this.primitiveType = type;
            this.color = color;
            this.name = name;
            this.collision = collision;
        }

        public void Spawn(Vector3 basepos, Vector3 scalemult, Vector3 rotationmod)
        {
            Vector3 newpos = basepos + position;
            Vector3 newscale = new Vector3(scale.x * scalemult.x, scale.y * scalemult.y, scale.z * scalemult.z);

            prim = Handlers.SpawnPrim(newpos, newscale, rotation, color, primitiveType, collision);
            Vector3 dir = prim.transform.position - basepos;
            Quaternion rot = Quaternion.Euler(rotationmod);
            dir = rot * dir;
            prim.transform.position = basepos + dir;
            prim.transform.rotation = rot * prim.transform.rotation;
        }

        public void Despawn()
        {
            if (prim != null) { NetworkServer.Destroy(prim.gameObject); }
        }

    }

    public class Light
    {
        private Vector3 position;
        private Color color;
        public string name;
        private float range;
        private float intensity;
        private LightSourceToy lightsource;

        public Light(Vector3 position, Color color, string name, float range, float intensity)
        {
            this.position = position;
            this.color = color;
            this.name = name;
            this.range = range;
            this.intensity = intensity;
        }

        public void Spawn(Vector3 basepos)
        {
            lightsource = Handlers.AddLight(basepos + position, color, range, intensity);
        }

        public void Despawn()
        {
            if (lightsource != null) { NetworkServer.Destroy(lightsource.gameObject); }
        }

    }

    public class Building
    {
        public List<Light> lights;
        public List<Structure> structures;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public string name;

        public Building(Vector3 position, Vector3 rotation, Vector3 scale, string name)
        {
            this.structures = new List<Structure>();
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.name = name;
            this.lights = new List<Light>();
        }

        public void AddLight(Light lightsource)
        {
            lights.Add(lightsource);
        }

        public void RemoveLight(Light lightsource)
        {
            lights.Remove(lightsource);
        }

        public void Add(Structure struc)
        {
            structures.Add(struc);
        }

        public void Remove(Structure struc)
        {
            structures.Remove(struc);
        }

        public void SpawnBuilding()
        {
            foreach (Structure structure in structures)
            {
                structure.Spawn(position, scale, rotation);
            }
            foreach (Light light in lights)
            {
                light.Spawn(position);
            }
        }
    }
}