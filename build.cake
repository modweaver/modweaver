
//#load nuget:?package=StyleCop.Error.MSBuild&version=1.0.0

var target = Argument("target", "Build");
var configuration = Argument("configuration", "Release");

var coreBuildDir = $"modweaver.core/bin/{configuration}";
var preloadBuildDir = $"modweaver.preload/bin/{configuration}";

var binDir = "./bin";
var buildDirs = new[] {coreBuildDir, preloadBuildDir};
var csProjs = new[] {"modweaver.core/modweaver.core.csproj", "modweaver.preload/modweaver.preload.csproj"};

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
	.WithCriteria(c => HasArgument("clean"))
	.Does(() => {
		foreach (var dir in buildDirs) {
			CleanDirectory(dir);
		}
		CleanDirectory(binDir);
	});

Task("Build")
	.IsDependentOn("Clean")
	.Does(() => {
		CreateDirectory(binDir);

		MSBuild("./modweaver.sln", settings =>
			settings.SetConfiguration(configuration));
		foreach (var dir in buildDirs) {
			CopyFiles($"{dir}/*", binDir);
			CleanDirectory(dir);
		}
});

/*Task("StyleCop").Does(() => {
    foreach (var proj in csProjs) {
        var result = StyleCopAnalyze();
        if (result > 0)
        {
            Information($"StyleCop found {result} violation(s) in {proj}. Build will fail.");
            throw new Exception($"Build failed due to StyleCop violations in project: {proj}.");
        }
    }
});*/

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
