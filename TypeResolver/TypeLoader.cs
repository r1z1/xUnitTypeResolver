
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;


namespace TypeResolver {

    /// <summary>
    /// Loads and caches usable <see cref="Type"/>s.
    /// </summary>
    public static partial class TypeLoader {

        /// <summary>
        /// Returns <see langword="true"/> if the <paramref name="type"/> could be used to create instances;
        /// otherwise, <see langword="false"/>.
        /// </summary>
        public static bool IsUsableType( Type type ) {
            if( type == null )
                return false;

            // Ignore interfaces.
            if( type.IsInterface )
                return false;

            // If the type is abstract, allow only static Factory classes.
            if( type.IsAbstract )
                return type.IsSealed
                    && type.Name.StartsWith( "Factory" );

            // Allow structures.
            if( type.IsValueType )
                return true;

            // Allow any other types with accessible constructors.
            var typeConstructors = type.GetConstructors( );
            return typeConstructors.Length > 0;
        }


        /// <summary>
        /// Returns a collection of <see cref="Type"/>s that could potentially be used
        /// to create instances of the <paramref name="referenceType"/>.
        /// </summary>
        public static IEnumerable<Type> GetUsableTypes( Type referenceType ) {
            Debug.Assert( referenceType != null );

            Assembly[] referenceAssemblies =
                referenceType.IsGenericParameter
                    ? referenceType.GetGenericParameterConstraints( ).Select( c => c.Assembly ).Distinct( ).ToArray( )
                    : new[] { referenceType.Assembly };
            return TypeLoader.GetUsableTypes( referenceAssemblies );
        }

        /// <summary>
        /// Returns a collection of <see cref="Type"/>s that could potentially be used
        /// to create instances of types using the <paramref name="referenceAssemblies"/>.
        /// </summary>
        /// <remarks>
        /// Only types from assemblies that use the reference assembly are checked,
        /// as types that do not reference that assembly could not implement the type
        /// (i.e. a type from mscorlib could not implement <c>IMyCustomInterface</c>
        /// since mscorlib does not reference any assemblies).
        /// </remarks>
        public static IEnumerable<Type> GetUsableTypes( params Assembly[] referenceAssemblies ) {
            Debug.Assert( referenceAssemblies != null );
            Debug.Assert( referenceAssemblies.All( ( a ) => a != null ) );

            // Find all assemblies that use the reference type's assembly.
            var usableAssemblies = TypeLoader.GetUsableAssemblies( referenceAssemblies );

            // Retrieve all usable types from referencing assemblies.
            var usableTypes = usableAssemblies
                .SelectMany( assembly => TypeLoader.GetTypes( assembly ) );

            return usableTypes;
        }

    }

}
