
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TypeResolver.Internal;
using Xunit.Extensions;


namespace TypeResolver {
    using AssemblyCache = System.Collections.Concurrent.ConcurrentDictionary<Assembly, IEnumerable<Assembly>>;
    using TypeCache = System.Collections.Concurrent.ConcurrentDictionary<Assembly, IEnumerable<Type>>;

    public static partial class TypeLoader {

        private static readonly TypeCache types_ = new TypeCache( );
        private static readonly HashSet<string> excludedAssemblies_ = new HashSet<string>( StringComparer.OrdinalIgnoreCase ) { "xunit.runner.visualstudio.testadapter" };
        private static readonly AssemblyCache referencedAssemblies_ = new AssemblyCache( );
        private static readonly AssemblyCache referringAssemblies_ = new AssemblyCache( );


        private static bool IsExcludedAssembly( Assembly assembly ) {
            return IsExcludedAssembly( assembly.FullName );
        }

        private static bool IsExcludedAssembly( string assemblyFullName ) {
            return excludedAssemblies_.Contains( assemblyFullName )
                || excludedAssemblies_.Any( excluded => assemblyFullName.StartsWith( excluded, StringComparison.OrdinalIgnoreCase ) );
        }

        private static IEnumerable<Assembly> GetReferencedAssemblies( Assembly assembly ) {
            if( !referencedAssemblies_.ContainsKey( assembly ) ) {
                referencedAssemblies_[assembly] =
                    IsExcludedAssembly( assembly )
                        ? new Assembly[0]
                        : assembly.GetReferencedAssemblies( )
                                  .Where( assemblyName => !IsExcludedAssembly( assemblyName.FullName ) )
                                  .Select( Assembly.Load )
                                  .ToArray( );

                ProcessExclusionAttributes( referencedAssemblies_[assembly] );
            }

            return referencedAssemblies_[assembly];
        }

        private static IEnumerable<Assembly> GetReferringAssemblies( Assembly referenceAssembly ) {
            if( !TypeLoader.referringAssemblies_.ContainsKey( referenceAssembly ) ) {
                // Retrieve currently loaded assemblies.
                var allAssemblies = new List<Assembly>( TypeLoader.referringAssemblies_.Keys );
                allAssemblies.AddRange( AppDomain.CurrentDomain.GetAssemblies( ) );

                // Check for excluded assemblies, before processing references.
                ProcessExclusionAttributes( allAssemblies );

                // Retrieve all referenced assemblies.
                for( int i = 0; i < allAssemblies.Count; ++i ) {
                    var assembly = allAssemblies[i];
                    foreach( var reference in GetReferencedAssemblies( assembly ) ) {
                        if( !allAssemblies.Contains( reference ) )
                            allAssemblies.Add( reference );
                    }
                }

                foreach( var assembly in allAssemblies ) {
                    var assemblyName = assembly.FullName;
                    var referrers =
                        IsExcludedAssembly( assemblyName )
                            ? new Assembly[0].AsEnumerable( )
                            : allAssemblies
                                .Where( ( a ) => TypeLoader.DoesAssemblyUseTargetAssembly( a, assemblyName ) )
                                .ToReadOnlyCollection( );
                    TypeLoader.referringAssemblies_[assembly] = referrers;
                }
            }

            return referenceAssembly.FullName.StartsWith( "mscorlib," )
                 ? TypeLoader.referringAssemblies_.Keys
                 : TypeLoader.referringAssemblies_[referenceAssembly];
        }

        private static IEnumerable<Type> GetTypes( Assembly assembly ) {
            if( !types_.ContainsKey( assembly ) ) {
                try {
                    types_[assembly] =
                        IsExcludedAssembly( assembly )
                            ? Type.EmptyTypes
                            : GetTypesCore( assembly );
                }
                catch( NotSupportedException ex ) {
                    // Ignore types from dynamic assemblies.
                    Debug.Assert( ex.Message.Contains( "dynamic" ) );
                    TypeLoader.types_[assembly] = Type.EmptyTypes;
                }
            }

            return TypeLoader.types_[assembly];
        }

        private static IEnumerable<Type> GetTypesCore( Assembly assembly ) {
            // When types are requested, add to type cache.
            var usableTypes = assembly
                .GetExportedTypes( )
                .Where( TypeLoader.IsUsableType )
                .ToReadOnlyCollection( );

            ProcessExclusionAttributes( assembly );

            return usableTypes;
        }

        private static void ProcessExclusionAttributes( IEnumerable<Assembly> assemblies ) {
            var orderedAssemblies = assemblies.OrderByDescending( assembly => {
                string assemblyName = assembly.FullName;
                return assemblyName.StartsWith( "System" )
                    || assemblyName.StartsWith( "Microsoft" )
                     ? "Z" + assemblyName
                     : assemblyName;
            } );

            foreach( Assembly assembly in orderedAssemblies ) {
                if( !IsExcludedAssembly( assembly ) )
                    ProcessExclusionAttributes( assembly );
            }
        }

        private static void ProcessExclusionAttributes( Assembly assembly ) {
            // Check for invocation helpers.
            var assistAttributes = assembly.GetCustomAttributes( typeof( Xunit.Extensions.InstantiateInstanceDataAttribute ), inherit: false );
            foreach( Xunit.Extensions.InstantiateInstanceDataAttribute attribute in assistAttributes )
                InstanceCreator.InstanceDataInvoker = (IInstantiateInstanceData)Activator.CreateInstance( attribute.InstantiatorType );


            // Check for excluded types.
            var limitAttributes = assembly.GetCustomAttributes( typeof( Xunit.Extensions.LimitInstanceDataAttribute ), inherit: false );
            foreach( Xunit.Extensions.LimitInstanceDataAttribute attribute in limitAttributes )
                TypeCreator.LimitInstances( attribute.TargetType, attribute.AvailableTypes );


            // Check for excluded assemblies.
            var excludeAttributes = assembly.GetCustomAttributes( typeof( Xunit.Extensions.ExcludeAssemblyAttribute ), inherit: false );
            foreach( Xunit.Extensions.ExcludeAssemblyAttribute attribute in excludeAttributes )
                excludedAssemblies_.Add( attribute.AssemblyName );

            // Reset any excluded examined assemblies.
            foreach( Assembly excluded in types_.Keys.Where( IsExcludedAssembly ).ToArray( ) )
                types_[excluded] = Type.EmptyTypes;
        }

        private static bool DoesAssemblyUseTargetAssembly( Assembly assembly, string targetAssemblyName ) {
            Debug.Assert( assembly != null );
            Debug.Assert( targetAssemblyName != null );

            // Check if assembly is the target assembly,
            //  or if it directly references the target assembly.
            return assembly.FullName.OrdinalEqual( targetAssemblyName )
                || GetReferencedAssemblies( assembly )
                    .Any( ( r ) => r.FullName.OrdinalEqual( targetAssemblyName ) );
        }

        private static IEnumerable<Assembly> GetUsableAssemblies( IEnumerable<Assembly> referenceAssemblies ) {
            var referrers = referenceAssemblies
                .Select( GetReferringAssemblies )
                .Aggregate( ( current, next ) => current.Intersect( next ) );
            return referrers;
        }

    }

}
