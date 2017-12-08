
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;


namespace TypeResolver.Extensions {

    /// <summary>
    /// Contains useful extensions for dealing with <see cref="Type"/>s.
    /// </summary>
    public static partial class TypeExtensions {

        /// <summary>
        /// Returns <see langword="true"/> if the object derives from <typeparamref name="T"/>;
        /// otherwise, <see langword="false"/>.
        /// </summary>
        [DebuggerStepThrough]
        public static bool Is<T>( this object obj ) {
            return obj.Is( typeof( T ) );
        }

        /// <summary>
        /// Returns <see langword="true"/> if the object derives from the target <see cref="Type"/>;
        /// otherwise, <see langword="false"/>.
        /// </summary>
        [DebuggerStepThrough]
        public static bool Is( this object obj, Type targetType ) {
            return obj != null
                && obj.GetType( ).Is( targetType );
        }

        /// <summary>
        /// Returns <see langword="true"/> if the <see cref="Type"/> derives from <typeparamref name="T"/>;
        /// otherwise, <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// In addition to closed <see cref="Type"/>s, <see cref="TypeExtensions.Is{T}(Type)"/> will return <see langword="true"/> for
        /// open generic types. E.g. <c>typeof(int?).Is&lt;Nullable&lt;&gt;&gt;( )</c> will be <see langword="true"/>.
        /// </remarks>
        [DebuggerStepThrough]
        public static bool Is<T>( this Type type ) {
            return type.Is( typeof( T ) );
        }

        /// <summary>
        /// Returns <see langword="true"/> if the <see cref="Type"/> derives from the target <see cref="Type"/>;
        /// otherwise, <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// In addition to closed <see cref="Type"/>s, <see cref="TypeExtensions.Is(Type,Type)"/> will return <see langword="true"/> for
        /// open generic types. E.g. <c>typeof(int?).Is( typeof(Nullable&lt;&gt;) )</c> will be <see langword="true"/>.
        /// </remarks>
        [DebuggerStepThrough]
        public static bool Is( this Type type, Type targetType ) {
            if( type != null && targetType != null )
                if( targetType.IsArray )
                    return type.IsArray
                        && targetType.GetArrayRank( ) == type.GetArrayRank( )
                        && type.GetElementType( ).Is( targetType.GetElementType( ) );
                else if( targetType.IsGenericParameter )
                    return targetType.GetGenericParameterConstraints( ).All( ( c ) => type.Is( c ) );
                else if( type.IsGenericParameter )
                    return type.GetGenericParameterConstraints( ).All( ( c ) => c.Is( targetType ) );
                else if( targetType.ContainsGenericParameters )
                    return type.IsGenericType( targetType, false );
                else if( type.ContainsGenericParameters && !type.IsInterface )
                    return type.IsGenericType( targetType, true );
                else
                    return targetType.IsAssignableFrom( type );
            return false;
        }

        [DebuggerStepThrough]
        private static bool IsGenericType( this Type type, Type targetType, bool typeIsGeneric ) {
            Debug.Assert( type != null );
            Debug.Assert( targetType != null );
            Debug.Assert( type.ContainsGenericParameters || targetType.ContainsGenericParameters );

            if( typeIsGeneric && !type.IsInterface && targetType.IsInterface ) {
                bool isInterface = type.GetInterfaces( ).Any( ( i ) => i.Is( targetType ) || (i.ContainsGenericParameters && i.IsGenericType( targetType, typeIsGeneric )) );
                return isInterface;
            }

            for( Type t = type; t != null; t = t.BaseType ) {
                if( typeIsGeneric && !t.ContainsGenericParameters )
                    break;

                bool isMatch =
                    typeIsGeneric
                        ? targetType.MatchesGenericType( t )
                        : t.MatchesGenericType( targetType );
                if( isMatch )
                    return true;
            }

            return false;
        }

        [DebuggerStepThrough]
        private static bool MatchesGenericType( this Type type, Type genericType ) {
            if( genericType.IsAssignableFrom( type ) || type.MatchesGenericTypeDefinition( genericType ) )
                return true;

            return genericType.IsInterface
                && type.GetInterfaces( ).Any( ( i ) => i.MatchesGenericTypeDefinition( genericType ) );
        }

        [DebuggerStepThrough]
        private static bool MatchesGenericTypeDefinition( this Type type, Type genericType ) {
            bool matchesDefinition = type.IsGenericType
                && genericType.GetGenericTypeDefinition( ) == type.GetGenericTypeDefinition( );
            if( !matchesDefinition )
                return false;

            var typeArguments = type.GetGenericArguments( );
            var genericArguments = genericType.GetGenericArguments( );
            bool satisfiesConstraints = Enumerable.Range( 0, genericArguments.Length )
                .All( ( i ) => genericArguments[i].IsGenericParameter || typeArguments[i].Is( genericArguments[i] ) );
            return satisfiesConstraints;
        }


        /// <summary>
        /// Returns a descriptive name of the <see cref="Type"/>, including generic parameters.
        /// </summary>
        public static string GetDescriptiveName( this Type type ) {
            var descrptiveName = new DescriptiveName( type );
            return descrptiveName.ToString( );
        }

        /// <summary>
        /// Returns a descriptive name of the <see cref="MethodBase"/>, including generic parameters.
        /// </summary>
        public static string GetDescriptiveName( this MethodBase method ) {
            var descrptiveName = new DescriptiveName( method );
            return descrptiveName.ToString( );
        }

    }

}
