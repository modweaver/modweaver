using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine;

namespace modweaver.api.API {
    public static class NewWeaponsManager {
        private static uint _prefabHandlerId = 7777; //TODO: fix possible collision
        private static List<Weapon> _newWeapons = new();
        private static List<CustomWeapon> _newUserWeapons = new();
        private static bool _weaponsAdded = false;

        public delegate void CallbackDelegate();
        public static event CallbackDelegate OnInitCompleted;

        public static void AddNewWeapon(CustomWeapon weapon) {
            _newUserWeapons.Add(weapon);
        }


        [HarmonyPatch(typeof(SelectedCustomTierScreen), nameof(SelectedCustomTierScreen.Awake))] //TODO: make it dynamicly fit screen
        public static class FixWeaponsVisualPatch {
            [HarmonyPostfix]
            public static void Postfix(ref SelectedCustomTierScreen __instance) {
                GameObject grid = GameObject.Find("View - VersusWeapons/Grid");
                var gridLayout = grid.GetComponent<GridLayoutGroup>();
                gridLayout.spacing = new Vector2(5, 15);
                gridLayout.constraintCount = 8;
                gridLayout.cellSize = new Vector2(76, 40);
            }
        }



        [HarmonyPatch(typeof(CustomMapUI), "Awake")]
        public static class AddWeaponsPatch1 {
            [HarmonyPrefix]
            public static void Prefix() {
                AddWeapons();
            }
        }
        [HarmonyPatch(typeof(CustomTiersScreen), "Awake")]
        public static class AddWeaponsPatch2 {
            [HarmonyPrefix]
            public static void Prefix() {
                AddWeapons();
            }
        }
        [HarmonyPatch(typeof(SelectedCustomTierScreen), "Awake")]
        public static class AddWeaponsPatch3 {
            [HarmonyPrefix]
            public static void Prefix() {
                AddWeapons();
            }
        }

        [HarmonyPatch(typeof(CustomMapEditor), "Awake")]
        public static class AddWeaponsPatch5 {
            [HarmonyPrefix]
            public static void Prefix() {
                AddWeapons();
            }
        }

        [HarmonyPatch(typeof(CustomTierWavesScreen), "Setup")]
        public static class AddWeaponsPatch6 {
            [HarmonyPrefix]
            public static void Prefix() {
                AddWeapons();
            }
        }
        [HarmonyPatch(typeof(VersusMode), "Awake")]
        public static class AddWeaponsPatch7 {
            [HarmonyPostfix]
            public static void Postfix(ref VersusMode __instance) {
                AddWeapons();
                __instance.weapons.AddRange(_newWeapons.Select(x => new SpawnableWeapon(x, 3)));
            }
        }

        [HarmonyPatch(typeof(CustomTierWeaponButton), "Setup")]
        public static class FixNewWeaponName {
            [HarmonyPostfix]
            public static void Postfix(ref CustomTierWeaponButton __instance) {//TODO: save original translation
                AccessTools.FieldRefAccess<CustomTierWeaponButton, TextMeshProUGUI>("titleText")(__instance).text =
                    AccessTools.FieldRefAccess<CustomTierWeaponButton, Weapon>("_weaponObj")(__instance).name;
            }
        }

        static void AddWeapons() {
            var tmp = Resources.FindObjectsOfTypeAll<ElementLists>();

            if (tmp.Length == 0 || _weaponsAdded) {
                return;
            }

            ElementLists list = tmp[0];
            SerializationWeaponName maxName = (SerializationWeaponName)Enum.GetValues(typeof(SerializationWeaponName)).Cast<int>().Max();
            _newWeapons.AddRange(list.allWeapons);
            List<Weapon> tmpList = new List<Weapon>();

            foreach (var weapon in _newUserWeapons) {
                Weapon newWeapon = _newWeapons[(int)weapon.Type];
                GameObject copiedWeapon = UnityEngine.Object.Instantiate(newWeapon.gameObject);
                GameObject.DontDestroyOnLoad(copiedWeapon.gameObject);
                copiedWeapon.SetActive(false);
                weapon.WeaponObject = copiedWeapon;

                newWeapon = copiedWeapon.GetComponent<Weapon>();
                newWeapon.serializationWeaponName = maxName + 1;
                newWeapon.name = weapon.Name;
                newWeapon.usedInCustomTiers = true;

                OnInitCompleted?.Invoke();

                var no = newWeapon.GetComponent<NetworkObject>();
                var handler = new WeaponNetworkPrefabInstanceHandler(_prefabHandlerId++, no);
                NetworkManager.Singleton.PrefabHandler.AddHandler(handler.Id, handler);

                newWeapon.transform.TransformPoint(9999 + tmpList.Count * 1000, 9999, 9999);
                tmpList.Add(newWeapon);
            }
            _newWeapons = tmpList;
            list.allWeapons.AddRange(_newWeapons);
            _weaponsAdded = true;
        }

        [HarmonyPatch(typeof(WeaponSpawner), nameof(WeaponSpawner.SpawnWeapon))]
        public static class SpawnWeaponFix {//this just in case there will be timer weapon to live

            [HarmonyPrefix]
            public static void Prefix(ref WeaponSpawner __instance) {
                foreach (var w in _newWeapons) {
                    w.gameObject.SetActive(true);
                }
            }
            [HarmonyPostfix]
            public static void Postfix(ref WeaponSpawner __instance) {
                foreach (var w in _newWeapons) {
                    w.gameObject.SetActive(false);
                }
            }
        }
        public enum WeaponType {
            BigGrenade,
            BigParticleBlade,
            BurstLauncher,
            DeathRay,
            DoubleParticleBlade,
            DoubleRailvolver,
            GrenadeLauncher,
            GrenadeParkour,
            Grenade,
            HeckSaw,
            KhepriStaff,
            LaserCannonParkour,
            LaserCannon,
            LaserCube,
            Mine,
            MineParkour,
            MiniShotgun,
            BeachBall,
            FireworkLauncherParkour,
            FireworkLauncher,
            Flare,
            Flashlight,
            ParticleBladeLauncher,
            ParticleBladeParkour,
            ParticleBlade,
            ParticleSpearParkour,
            ParticleSpearVariant,
            PreArmedGrenade,
            PreArmedParticleBlade,
            Railvolver,
            RailvolverParkour,
            RocketLauncher,
            RocketLauncherParkour,
            Shotgun,
            ShotgunParkour,
            AutoShotgun,
            GravityGrenade,
            GravitySaw,
            BoomStick,
            DeathCube,
            Snowball,
        }
    }
}
