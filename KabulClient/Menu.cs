﻿using VRC;
using VRC.Core;
using MelonLoader;
using UnityEngine;
using KabulClient.Features.Worlds;
using Il2CppSystem.Collections.Generic;

namespace KabulClient
{
    class Menu
    {
        // 0 = Main
        // 1 = ESP
        // 2 = World
        // 3 = Players
        private static int selectedTab = 0;

        public static bool showMenu = false;
        private static int yOffset = 0;
        public static Player selectedPlayer = null;

        public static void ToggleMenu()
        {
            MelonLogger.Msg("Toggling menu.");
            showMenu = !showMenu;
        }

        public static void HandleCursor()
        {
            // If the menu is disabled these will be set back automatically by VRChat itself.
            Cursor.lockState = showMenu ? CursorLockMode.None : Cursor.lockState;
            Cursor.visible = showMenu;
        }

        private static void ESPTab()
        {
            yOffset = 70;

            GUI.contentColor = Features.ESP.espEnabled ? Color.green : Color.red;
            
            if (GUI.Button(new Rect(20, yOffset, 200, 20), Features.ESP.espEnabled ? "ESP enabled" : "ESP disabled"))
            {
                Features.ESP.Toggle();
            }

            GUI.contentColor = Color.white;

            yOffset += 30;

            GUI.Label(new Rect(20, yOffset, 200, 20), $"RGB speed ({Features.ESP.espRainbowSpeed})");
            Features.ESP.espRainbowSpeed = GUI.HorizontalSlider(new Rect(130, yOffset + 2, 200, 20), Features.ESP.espRainbowSpeed, 0.0f, 1.0f);

            yOffset += 30;
        }

        private static void WorldTab()
        {
            yOffset = 70;

            // This is messy.
            if (JustBClub.worldLoaded)
            {
                if (JustBClub.roomsInitialized)
                {
                    VRCPlayer localPlayer = Utils.GetLocalPlayer();

                    foreach (JustBClub.PrivateRoom privateRoom in JustBClub.privateRooms)
                    {
                        // Keep in mind that if one of these are null, all of them are null.
                        if (privateRoom == null)
                        {
                            continue;
                        }

                        if (GUI.Button(new Rect(20, yOffset, 200, 20), privateRoom.roomNumber == 7 ? "VIP room" : $"Room {privateRoom.roomNumber}"))
                        {
                            // Set the room object active so we can see and interact with it.
                            privateRoom.roomObject?.SetActive(true);

                            // Teleport to the room.
                            localPlayer.transform.position = privateRoom.position;
                        }

                        yOffset += 30;
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(20, yOffset, 200, 20), $"Initialize rooms"))
                    {
                        JustBClub.InitializeRooms();
                    }

                    yOffset += 30;
                }
            } 
            else if (AmongUs.worldLoaded)
            {
                // TODO: Check if you're in the world first.
                if (GUI.Button(new Rect(20, yOffset, 200, 20), $"Call emergency button"))
                {
                    AmongUs.EmergencyButton();
                }

                yOffset += 30;
            }
            else
            {
                GUI.Label(new Rect(20, yOffset, 200, 20), "No options exist for this world!");
            }
        }

        private static void PlayerTab(Player ply)
        {
            APIUser apiUser = ply?.prop_APIUser_0;

            yOffset = 70;

            GUI.Label(new Rect(20, yOffset, 500, 20), $"Selected player: {apiUser.displayName}");

            yOffset += 30;

            if (GUI.Button(new Rect(20, yOffset, 200, 20), "Teleport"))
            {
                VRCPlayer localPlayer = Utils.GetLocalPlayer();

                if (localPlayer != null && ply != null)
                {
                    localPlayer.transform.position = ply.transform.position;
                }
            }

            yOffset += 30;

            if (GUI.Button(new Rect(20, yOffset, 200, 20), "Back"))
            {
                selectedPlayer = null;
            }

            yOffset += 30;
        }

        private static void PlayersTab()
        {
            if (selectedPlayer == null)
            {
                yOffset = 70;

                List<Player> players = Utils.GetAllPlayers();

                foreach (Player player in players)
                {
                    if (player == null)
                    {
                        continue;
                    }

                    if (GUI.Button(new Rect(20, yOffset, 300, 20), $"{player?.prop_APIUser_0.displayName}"))
                    {
                        MelonLogger.Msg($"Selecting player {player?.prop_APIUser_0.displayName}.");
                        selectedPlayer = player;
                    }

                    yOffset += 30;
                }
            }
            else
            {
                PlayerTab(selectedPlayer);
            }
        }

        private static void MainTab()
        {
            yOffset = 70;

            GUI.contentColor = Features.AntiPortal.antiPortalEnabled ? Color.green : Color.red;

            if (GUI.Button(new Rect(20, yOffset, 200, 20), Features.AntiPortal.antiPortalEnabled ? "Anti-Portal enabled" : "Anti-Portal disabled"))
            {
                Features.AntiPortal.Toggle();
            }

            GUI.contentColor = Color.white;

            yOffset += 30;

            Features.Speedhack.speedMultiplier = GUI.HorizontalSlider(new Rect(20, yOffset, 200, 20), Features.Speedhack.speedMultiplier, 1, 10);
            GUI.Label(new Rect(200, yOffset + 2, 400, 20), $"Speed ({Features.Speedhack.speedMultiplier})");
            
            yOffset += 30;
        }

        public static void Main()
        {
            if (!showMenu)
            {
                return;
            }

            // This code sucks dick and I HATE UI being done in code.
            GUI.BeginGroup(new Rect(10, 10, 500, 700));

            GUI.contentColor = Color.yellow;
            GUI.Box(new Rect(0, 0, 500, 700), "Kabul Client");
            GUI.contentColor = Color.white;

            // Tab control.
            // This is really bad but due to how using GUI.SelectionGrid crashes the game due to the fact it tries
            // to use string[] for what is supposed to be Il2CppStringArray which I can't figure out how to create.
            if (GUI.Button(new Rect(20, 40, 50, 20), (selectedTab == 0) ? "[Main]" : "Main")) { selectedTab = 0; }
            if (GUI.Button(new Rect(70, 40, 50, 20), (selectedTab == 1) ? "[ESP]": "ESP")) { selectedTab = 1; }
            if (GUI.Button(new Rect(120, 40, 60, 20), (selectedTab == 2) ? "[World]" : "World")) { selectedTab = 2; }
            if (GUI.Button(new Rect(180, 40, 70, 20), (selectedTab == 3) ? "[Players]" : "Players")) { selectedTab = 3; }

            // Choose the tab to render.
            switch (selectedTab)
            {
                case 0: MainTab(); break;
                case 1: ESPTab(); break;
                case 2: WorldTab(); break;
                case 3: PlayersTab(); break;
            }

            GUI.EndGroup();
        }
    }
}
