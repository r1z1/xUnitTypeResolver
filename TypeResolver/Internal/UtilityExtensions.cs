
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TypeResolver.Extensions;


namespace TypeResolver.Internal {

    internal static class UtilityExtensions {

        public static bool IsNullOrEmpty<T>( this IEnumerable<T> collection ) {
            return collection == null
                || !collection.Any( );
        }

        public static void ForEach<T>( this IEnumerable<T> source, Action<T> action ) {
            if( source != null )
                foreach( var item in source )
                    action( item );
        }

        [DebuggerHidden]
        public static LinkList<T> ToLinkList<T>( this IEnumerable<T> source ) {
            Debug.Assert( source != null );

            return source
                .Reverse( )
                .Aggregate( LinkList<T>.Empty, ( l, v ) => l.Add( v ) );
        }

        [DebuggerHidden]
        public static LinkList<T> MakeLinkList<T>( this T item ) {
            return LinkList<T>.Empty.Add( item );
        }

        [DebuggerHidden]
        public static IEnumerable<T> MakeEnumerable<T>( this T item ) {
            return new[] { item };
        }

        [DebuggerHidden]
        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>( this IList<T> list ) {
            return new ReadOnlyCollection<T>( list );
        }

        [DebuggerHidden]
        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>( this IEnumerable<T> source ) {
            return new ReadOnlyCollection<T>( source.ToArray( ) );
        }


        public static string Join<T>( this IEnumerable<T> source, string separator ) {
            if( typeof( T ) == typeof( Type ) )
                return source.Cast<Type>( ).Join( separator, TypeExtensions.GetDescriptiveName );
            else
                return source.Join( separator, ( item ) => ((object)item ?? "").ToString( ) );
        }

        public static string Join<T>( this IEnumerable<T> source, string separator, Func<T, string> toString ) {
            var sb = new StringBuilder( );
            source.Join( separator, sb, ( item ) => sb.Append( toString( item ) ) );
            return sb.ToString( );
        }

        public static void Join<T>( this IEnumerable<T> source, string separator, StringBuilder sb, Action<T> append ) {
            Debug.Assert( source != null );
            Debug.Assert( separator != null );
            Debug.Assert( sb != null );
            Debug.Assert( append != null );

            bool prefixSeparator = false;
            foreach( T item in source ) {
                if( prefixSeparator )
                    sb.Append( separator );
                prefixSeparator = true;
                append( item );
            }
        }

        public static string With( this string format, params object[] args ) {
            return string.Format( format, args );
        }


        [DebuggerHidden]
        public static bool OrdinalEqual( this string first, string second ) {
            return string.Compare( first, second, StringComparison.OrdinalIgnoreCase ) == 0;
        }

    }

}
