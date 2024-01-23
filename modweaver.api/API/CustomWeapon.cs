using UnityEngine;
using static modweaver.api.API.NewWeaponsManager;

namespace modweaver.api.API {
    public class CustomWeapon {

        public WeaponType Type;
        public GameObject WeaponObject;
        public string Name;
        public CustomWeapon(string name, WeaponType inheritFrom) {
            Name = name;
            Type = inheritFrom;
        }

    }
}
