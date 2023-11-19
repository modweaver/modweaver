using System.Collections.Generic;

namespace modweaver {
    public class ModManifest {
        public const string embeddedResourceName = "mw_manifest";
        
        public class Metadata
        {
            public string id { get; set; }
            public string version { get; set; }
            public string title { get; set; }
            public List<string> authors { get; set; }
            public string gameVersion { get; set; }
        }

        public class Dependency
        {
            public string guid { get; set; }
            public string versionRange { get; set; }
            public bool hardDependency { get; set; }
        }

        public Metadata metadata { get; set; }
        public List<Dependency> dependencies { get; set; }
        public List<string> incompatibilities { get; set; }
    }
}