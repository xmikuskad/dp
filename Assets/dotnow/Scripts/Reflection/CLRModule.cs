﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Mono.Cecil;
using Mono.Collections.Generic;
using dotnow.Runtime.JIT;

namespace dotnow.Reflection
{
#if API_NET35
    public sealed class CLRModule : IJITOptimizable
#else
    public sealed class CLRModule : Assembly, IJITOptimizable
#endif
    {
        // Private
        private AppDomain domain = null;
        private AssemblyDefinition assembly = null;
        private AssemblyName assemblyName = null;
        private AssemblyName[] referenceAssemblyNames = null;
        private CLRType[] types = null;
        private CLRType[] exportedTypes = null;
        private string location = "";

        // Properties
        public AppDomain Domain
        {
            get { return domain; }
        }       

        public AssemblyDefinition Assembly
        {
            get { return assembly; }
        }

        public AssemblyNameDefinition AssemblyName
        {
            get { return assembly.Name; }
        }

#if API_NET35
        internal IList<CLRType> CLRTypes
#else
        internal IReadOnlyList<CLRType> CLRTypes
#endif
        {
            get { return types; }
        }

        // System.Reflection.Assembly
#region Inherit
#if API_NET35
        public string CodeBase
#else
        public override string CodeBase
#endif
        {
            get { return location; }
        }

#if API_NET35
        public string EscapedCodeBase
#else
        public override string EscapedCodeBase
#endif
        {
            get { return Uri.EscapeDataString(location); }
        }

#if API_NET35
        public string Location
#else
        public override string Location
#endif
        {
            get { return location; }
        }

#if !API_NET35
        public override IEnumerable<TypeInfo> DefinedTypes
        {
            get { throw new NotSupportedException("Use GetTypes instead"); }
        }

        public override IEnumerable<Type> ExportedTypes
        {
            get { return exportedTypes; }
        }

        public override IEnumerable<CustomAttributeData> CustomAttributes
        {
            get { throw new NotSupportedException("Custom attributes are not supported"); }
        }
#endif

#if API_NET35
        public MethodInfo EntryPoint
#else
        public override MethodInfo EntryPoint
#endif
        {
            get { throw new NotSupportedException("Entry points are not supported"); }
        }

#if API_NET35
        public string FullName
#else
        public override string FullName
#endif
        {
            get { return assembly.FullName; }
        }

#if !API_NET35
        public override Module ManifestModule
        {
            get { throw new NotSupportedException("Trivial CLR has no concept of modules"); }
        }

        public override IEnumerable<Module> Modules
        {
            get { throw new NotSupportedException("Trivial CLR has no concept of modules"); }
        }
#endif
#endregion

        // Constructor
        internal CLRModule(AppDomain domain, AssemblyDefinition assembly, string location)
        {
            this.domain = domain;
            this.assembly = assembly;
            this.assemblyName = new AssemblyName(assembly.FullName);
            this.location = location;

            // Create references
            this.referenceAssemblyNames = new AssemblyName[assembly.MainModule.AssemblyReferences.Count];

            for (int i = 0; i < referenceAssemblyNames.Length; i++)
                this.referenceAssemblyNames[i] = new AssemblyName(assembly.MainModule.AssemblyReferences[i].FullName);

            // Create types
            Collection<TypeDefinition> typeDefinitions = assembly.MainModule.Types;
            int exportedCount = 0;

            types = new CLRType[typeDefinitions.Count];

            for (int i = 0; i < types.Length; i++)
            {
                types[i] = new CLRType(domain, this, typeDefinitions[i]);

                // Check for public
                if (types[i].IsPublic == true)
                    exportedCount++;
            }

            // Create exported types
            exportedTypes = new CLRType[exportedCount];

            for(int i = 0, j = 0; i < types.Length; i++)
            {
                if(types[i].IsPublic == true)
                {
                    exportedTypes[j] = types[i];
                    j++;
                }
            }
        }

        // Methods
        void IJITOptimizable.EnsureJITOptimized()
        {
            foreach (Type type in types)
                JITOptimize.EnsureJITOptimized(type);
        }

        public IMetadataTokenProvider GetRuntimeToken(MetadataToken token)
        {
            return assembly.MainModule.LookupToken(token);
        }

        // System.Reflection.Assembly
#region Inherit
#if API_NET35
        public Type[] GetTypes()
#else
        public override Type[] GetTypes()
#endif
        {
            return types;
        }

#if API_NET35
        public Type GetType(string name)
#else
        public override Type GetType(string name)
#endif
        {
            foreach (CLRType type in types)
            {
                // Check name
                if (string.Compare(type.Name, name) == 0)
                    return type;

                // Check full name
                if (string.Compare(type.FullName, name) == 0)
                    return type;
            }

            return null;
        }

#if API_NET35
        public Type GetType(string name, bool throwOnError)
#else
        public override Type GetType(string name, bool throwOnError)
#endif
        {
            foreach (CLRType type in types)
            {
                // Check name
                if (string.Compare(type.Name, name) == 0)
                    return type;

                // Check full name
                if (string.Compare(type.FullName, name) == 0)
                    return type;
            }

            if (throwOnError == true)
                throw new TargetException("Failed to find type: " + name);

            return null;
        }

#if API_NET35
        public Type GetType(string name, bool throwOnError, bool ignoreCase)
#else
        public override Type GetType(string name, bool throwOnError, bool ignoreCase)
#endif
        {
            foreach (CLRType type in types)
            {
                // Check name
                if (string.Compare(type.Name, name, true) == 0)
                    return type;

                // Check full name
                if (string.Compare(type.FullName, name) == 0)
                    return type;
            }

            if (throwOnError == true)
                throw new TargetException("Failed to find type: " + name);

            return null;
        }

#if API_NET35
        public Type[] GetExportedTypes()
#else
        public override Type[] GetExportedTypes()
#endif
        {
            return exportedTypes;
        }

#if !API_NET35
        public override object CreateInstance(string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
        {
            throw new NotSupportedException("Use AppDomain.CreateInstance instead");
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotSupportedException("Custom attributes are not supported");
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotSupportedException("Custom attributes are not supported");
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotSupportedException("Custom attributes are not supported");
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            throw new NotSupportedException("Custom attributes are not supported");
        }

        public override Module[] GetLoadedModules(bool getResourceModules)
        {
            throw new NotSupportedException("Trivial CLR has no concept of modules");
        }

        public override Module GetModule(string name)
        {
            throw new NotSupportedException("Trivial CLR has no concept of modules");
        }

        public override Module[] GetModules(bool getResourceModules)
        {
            throw new NotSupportedException("Trivial CLR has no concept of modules");
        }

        public override Module LoadModule(string moduleName, byte[] rawModule, byte[] rawSymbolStore)
        {
            throw new NotSupportedException("Trivial CLR has no concept of modules");
        }
#endif

#if API_NET35
        public AssemblyName GetName()
#else
        public override AssemblyName GetName()
#endif
        {
            return assemblyName;
        }

#if API_NET35
        public AssemblyName GetName(bool copiedName)
#else
        public override AssemblyName GetName(bool copiedName)
#endif
        {
            if (copiedName == true)
                return new AssemblyName(assembly.FullName);

            return assemblyName;
        }

#if API_NET35
        public AssemblyName[] GetReferencedAssemblies()
#else
        public override AssemblyName[] GetReferencedAssemblies()
#endif
        {
            return referenceAssemblyNames;
        }

#if !API_NET35
        public override FileStream GetFile(string name)
        {
            throw new NotSupportedException("Trivial CLR does not support encapsulated file storage");
        }

        public override FileStream[] GetFiles()
        {
            throw new NotSupportedException("Trivial CLR does not support encapsulated file storage");
        }

        public override FileStream[] GetFiles(bool getResourceModules)
        {
            throw new NotSupportedException("Trivial CLR does not support encapsulated file storage");
        }

        public override ManifestResourceInfo GetManifestResourceInfo(string resourceName)
        {
            throw new NotSupportedException("Trivial CLR does not support manifest resources");
        }

        public override string[] GetManifestResourceNames()
        {
            throw new NotSupportedException("Trivial CLR does not support manifest resources");
        }

        public override Stream GetManifestResourceStream(string name)
        {
            throw new NotSupportedException("Trivial CLR does not support manifest resources");
        }

        public override Stream GetManifestResourceStream(Type type, string name)
        {
            throw new NotSupportedException("Trivial CLR does not support manifest resources");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotSupportedException("Serialization is not supported");
        }

        public override Assembly GetSatelliteAssembly(CultureInfo culture)
        {
            throw new NotSupportedException("Satellite assemblies are not supported");
        }

        public override Assembly GetSatelliteAssembly(CultureInfo culture, Version version)
        {
            throw new NotSupportedException("Satellite assemblies are not supported");
        }
#endif
#endregion

        public override string ToString()
        {
            return assembly.ToString();
        }
    }
}
