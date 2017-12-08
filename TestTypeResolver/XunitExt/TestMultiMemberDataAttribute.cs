
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TypeResolver;


namespace Xunit.Extensions {

    public class TestMultiMemberDataAttribute {

        private static readonly IEqualityComparer<object[][]> Comparer = ArrayComparer<object[]>.Instance;

        private const string IntsName = "Ints";
        private const string CharsName = "Chars";
        private const string MultiDoublesName = "MultiDoubles";
        private const string OneMutableName = "OneMutable";
        private const string TwoMutablesName = "TwoMutables";

        private static readonly int[] ints_ = new[] { 1, 2, 3 };
        private static readonly char[] chars_ = new[] { 'c', 'h', 'a', 'r' };
        private static readonly object[][] multiDoubles_ = new[] { new object[] { 1.0, 2.0 }, new object[] { 10.0, 20.0 } };

        public static IEnumerable<object[]> Ints { get { return Yield( ints_ ); } }
        public static IEnumerable<object[]> Chars { get { return Yield( chars_ ); } }
        public static IEnumerable<object[]> MultiDoubles { get { return multiDoubles_; } }
        public static IEnumerable<object[]> OneMutable { get { return Yield( new[] { new Mutable( ) } ); } }
        public static IEnumerable<object[]> TwoMutables { get { return Yield( new[] { new Mutable( ), new Mutable( ) } ); } }


        [Fact]
        public void MultiMemberDataAttribute_matches_PropertyDataAttribute_for_single_data_source( ) {
            var method = IntMethod( default( int ) );

            var memberDataAttribute = new MemberDataAttribute( IntsName );
            var multiMemberDataAttribute = new MultiMemberDataAttribute( IntsName );

            object[][] propertyData = memberDataAttribute.GetData( method ).ToArray( );
            object[][] multiMemberData = multiMemberDataAttribute.GetData( method ).ToArray( );

            Assert.Equal( propertyData, multiMemberData, Comparer );
        }

        [Fact]
        public void MultiMemberDataAttribute_succeeds_for_multiple_data_sources( ) {
            var method = IntCharMethod( default( int ), default( char ) );
            var attribute = new MultiMemberDataAttribute( IntsName, CharsName );
            var rawPropertyData = new[] { ToObjectArray( ints_ ), ToObjectArray( chars_ ) };
            var propertyData = Permuter.Permute( rawPropertyData );

            var attributeData = attribute.GetData( method ).ToArray( );

            Assert.Equal( propertyData, attributeData, Comparer );
        }

        [Fact]
        public void MultiMemberDataAttribute_succeeds_for_data_sources_with_multple_values( ) {
            var method = DoubleDoubleMethod( default( double ), default( double ) );
            var attribute = new MultiMemberDataAttribute( MultiDoublesName );
            var propertyData = multiDoubles_;

            var attributeData = attribute.GetData( method ).ToArray( );

            Assert.Equal( propertyData, attributeData, Comparer );
        }

        [Fact]
        public void MultiMemberDataAttribute_succeeds_for_multiple_data_sources_with_multple_values( ) {
            var method = IntDoubleDoubleMethod( default( int ), default( double ), default( double ) );
            var attribute = new MultiMemberDataAttribute( IntsName, MultiDoublesName );
            var rawPropertyData = new[] { Ints.ToArray( ), multiDoubles_ };
            var propertyData = GetFlattenedPermutations( rawPropertyData );

            var attributeData = attribute.GetData( method ).ToArray( );

            Assert.Equal( propertyData, attributeData, Comparer );
        }

        [Fact( Skip = "MultiMemberData for mutable data sources not yet supported." )]
        public void MultiMemberDataAttribute_succeeds_for_data_sources_of_mutable_values( ) {
            var method = MutableMutableMethod( default( Mutable ), default( Mutable ) );
            var attribute = new MultiMemberDataAttribute( OneMutableName, TwoMutablesName );

            var attributeData = attribute.GetData( method ).ToArray( );

            foreach( object[] data in attributeData )
                foreach( object item in data ) {
                    var mutable = Assert.IsType<Mutable>( item );
                    Assert.False( mutable.IsMutated );
                    mutable.IsMutated = true;
                }
        }


        private MethodInfo IntMethod( int i ) {
            return MethodBase.GetCurrentMethod( ) as MethodInfo;
        }

        private MethodInfo IntCharMethod( int i, char c ) {
            return MethodBase.GetCurrentMethod( ) as MethodInfo;
        }

        private MethodInfo DoubleDoubleMethod( double d1, double d2 ) {
            return MethodBase.GetCurrentMethod( ) as MethodInfo;
        }

        private MethodInfo MutableMutableMethod( Mutable m1, Mutable m2 ) {
            return MethodBase.GetCurrentMethod( ) as MethodInfo;
        }

        private MethodInfo IntDoubleDoubleMethod( int i, double d1, double d2 ) {
            return MethodBase.GetCurrentMethod( ) as MethodInfo;
        }


        private static object[] ToObjectArray<T>( T[] array ) {
            object[] objectArray = new object[array.Length];
            Array.Copy( array, objectArray, array.Length );
            return objectArray;
        }

        private static object[][] GetFlattenedPermutations( object[][][] rawData ) {
            var permutations = Permuter.Permute( rawData );
            var flattenedPermutations = new object[permutations.Length][];

            for( int i = 0; i < permutations.Length; ++i ) {
                object[][] permutation = permutations[i];
                object[] flattened = permutation.SelectMany( data => data ).ToArray( );
                flattenedPermutations[i] = flattened;
            }

            return flattenedPermutations;
        }

        private static IEnumerable<object[]> Yield<T>( T[] items ) {
            foreach( T item in items ) {
                var data = new object[] { item };
                yield return data;
            }
        }


        public sealed class Mutable {
            private static int id_ = 0;
            public readonly int ID = id_++;
            public bool IsMutated { get; set; }
            public override string ToString( ) { return "Mutable " + ID + " is " + (this.IsMutated ? "mutated" : "normal"); }
        }

    }

}
