namespace modweaver.api.Extensions {
    public static class TypeUtilities {
        // public static Type[] GetInterfaceImplementations(this Type interfaceType, Assembly? assembly = null)
        // {
        //     return (assembly == null
        //                ? PluginUtilities.GetPluginAssemblies().SelectMany(ass => ass.GetTypes())
        //                : assembly.GetTypes())
        //            .Where(type => type.GetInterfaces().Contains(interfaceType) &&
        //                           type.GetConstructor(Type.EmptyTypes) != null)
        //            .ToArray();
        // }
        //
        // public static Type[] GetClassImplementations(this Type classType, Assembly? assembly = null)
        // {
        //     return (assembly == null
        //                ? PluginUtilities.GetPluginAssemblies().SelectMany(ass => ass.GetTypes())
        //                : assembly.GetTypes())
        //            .Where(type => type != classType &&
        //                           classType.IsAssignableFrom(type) &&
        //                           type.IsClass &&
        //                           !type.IsAbstract)
        //            .ToArray();
        // }
        //
        // public static bool IsOverride(this MethodInfo methodInfo)
        // {
        //     return methodInfo.GetBaseDefinition().DeclaringType != methodInfo.DeclaringType;
        // }
    }
}