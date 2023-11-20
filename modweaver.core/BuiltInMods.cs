using System.Collections.Generic;

namespace modweaver.core {
    public static class BuiltInMods {
        public static ModManifest modweaver = new() {
            metadata = new ModManifest.Metadata {
                id = "org.modweaver.loader",
                version = Utils.version,
                title = "ModWeaver",
                authors = new List<string> { "ModWeaver Team", "reddust9", "Ecorous", "viadot" },
                gameVersion = "1.4"
            },
            dependencies = new List<ModManifest.Dependency>(),
            incompatibilities = new List<string>()
        };
        
        public static ModManifest spiderheck = new() {
            metadata = new ModManifest.Metadata {
                id = "com.spiderheck",
                version = "1.4",
                title = "SpiderHeck",
                authors = new List<string> { "Neverjam", "Jazeps", "Pushka Studios" },
                gameVersion = "1.4"
            },
            dependencies = new List<ModManifest.Dependency>(),
            incompatibilities = new List<string>()
        };
    }
}