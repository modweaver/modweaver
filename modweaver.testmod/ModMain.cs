using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using modweaver.core;
using UnityEngine;
using modweaver.api.API;


namespace modweaver.testmod {
    [ModMainClass]
    // ReSharper disable once UnusedType.Global (because it is used!!)
    public class ModMain : Mod {

        public static List<global::Weapon> newWeapons;
        CustomWeapon weapon1;

        public override void Init() {
            newWeapons = new();

            weapon1 = new CustomWeapon("test weapon", NewWeaponsManager.WeaponType.ParticleBlade);
            NewWeaponsManager.AddNewWeapon(weapon1);
            NewWeaponsManager.OnInitCompleted += Weapon1;

            Harmony harmony = new Harmony(Metadata.id);
            Logger.Info("Running patches!");
            harmony.PatchAll();
            Logger.Info("Patched!");
        }


        void Weapon1() {
            weapon1.WeaponObject.GetComponent<ParticleBlade>().baseSize = new Vector2(10, 100);
            string path = "modweaver\\mods\\Testmod\\weapon1.png";

            int textureSize = 1000;
            SpriteRenderer sr = weapon1.WeaponObject.GetComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(LoadTextureFromFile(path, textureSize, textureSize), new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f));
        }

        private Texture2D LoadTextureFromFile(string filePath, int width, int height) {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(width, height);
            texture.LoadImage(fileData);

            return texture;
        }

        public override void Ready() {
            Logger.Info("Testmod is ready!");
        }

        public override void OnGUI(ModsMenuPopup ui) {

        }

        [HarmonyPatch(typeof(VersionNumberTextMesh), "Start")]
        public static class VNTMPatch {

            [HarmonyPostfix]
            public static void Postfix(ref VersionNumberTextMesh __instance) {
                __instance.textMesh.text += $"\n<color=red> Testmod v0.0.1 by YourName</color>";
            }
        }
    }
}