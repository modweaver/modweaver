using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace modweaver.api.API {
    public class Weapons {
        [HarmonyPatch(typeof(WeaponManager))]
        public class WeaponManagerPatch {
            public static Weapon? EquippedWeapon { get; internal set; }

            [HarmonyPatch("EquipWeapon")]
            [HarmonyPostfix]
            internal static void EquipWeaponPostfix(ref Weapon weapon) {
                EquippedWeapon = weapon;
                if (EquippedWeapon != null) {
                    weapon = EquippedWeapon;
                }
            }

            [HarmonyPatch("EquipWeapon")]
            [HarmonyPrefix]
            internal static void EquipWeaponPrefix(ref Weapon weapon) {
                if (EquippedWeapon != null) {
                    weapon = EquippedWeapon;
                }
            }

            [HarmonyPatch("UnEquipWeapon")]
            [HarmonyPostfix]
            internal static void UnEquipWeaponPostfix() {
                EquippedWeapon = null;
            }
        }

        public static Weapon? GetEquippedWeapon() {
            return WeaponManagerPatch.EquippedWeapon;
        }

        // * To be done
        public static void SetEquippedWeapon(Weapon weapon) {
            WeaponManagerPatch.EquippedWeapon = weapon;
        }

        // Add weapon
        public static Weapon CreateWeapon(int ammo, List<Weapon.WeaponType> weaponType, string label) {
            Weapon weapon = new() {
                maxAmmo = ammo,
                ammo    = ammo,
                label   = label,
                type    = weaponType
            };

            return weapon;
        }


        //* To be done

        private List<SpawnableWeapon> getVersusWeapons() {
            bool                  flag = GameSettings.Instance == null;
            List<SpawnableWeapon> result;
            if (flag) {
                result = null;
            }
            else {
                List<SpawnableWeapon> list = GameSettings.Instance.AvailableVersusWeapons();
                result = list;
            }

            return result;
        }

        /*private void spawnWeapon(int selectedWeapon) {
            GameObject gameObject = GameObject.FindGameObjectWithTag("PlayerRigidbody");
            Vector2    a          = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
            //bool flag = !Physics2D.Raycast(a + new Vector2(0f, 10f), gameObject.transform.up, 0.1f,
            //                               GameController.instance.worldLayers);
            bool flag2 = flag;
            if (flag2) {
                //Object.Instantiate(getVersusWeapons()[selectedWeapon].weapon, gameObject.transform.position, gameObject.transform.rotation);
                ObjectSpawner.Instantiate(getVersusWeapons()[selectedWeapon].weaponObject,
                                          gameObject.transform.position,
                                          gameObject.transform.rotation);
            }
        }*/
    }
}