
using System;
using System.Collections.Generic;
using System.Reflection;


namespace TypeResolver {

    sealed class ArrayComparer<T> : IEqualityComparer<T[]> {

        public static ArrayComparer<T> Instance = new ArrayComparer<T>( );


        private ArrayComparer( ) { }


        public bool Equals( T[] x, T[] y ) {
            if( x.Length != y.Length )
                return false;

            var itemComparer = GetComparer( );
            for( int i = 0; i < x.Length; ++i ) {
                T xItem = x[i];
                T yItem = y[i];
                bool equal = itemComparer.Equals( xItem, yItem );
                if( !equal )
                    return false;
            }

            return true;
        }

        public int GetHashCode( T[] obj ) {
            throw new NotImplementedException( );
        }


        private static IEqualityComparer<T> GetComparer( ) {
            IEqualityComparer<T> comparer;

            if( typeof( T ).IsArray ) {
                Type arrayElementType = typeof( T ).GetElementType( );
                Type arrayComparerType = typeof( ArrayComparer<> ).MakeGenericType( arrayElementType );
                FieldInfo arrayComparerInstanceField = arrayComparerType.GetField( "Instance", BindingFlags.Public | BindingFlags.Static );
                object arrayComparerInstance = arrayComparerInstanceField.GetValue( null );

                comparer = (IEqualityComparer<T>)arrayComparerInstance;
            }
            else {
                comparer = EqualityComparer<T>.Default;
            }

            return comparer;
        }
    }

}
