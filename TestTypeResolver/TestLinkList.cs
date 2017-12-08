
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Extensions;


namespace TypeResolver {

    public class TestLinkList {

        public static IEnumerable<object[]> TestElements {
            get {
                yield return new object[] { new int[] { } };
                yield return new object[] { new int[] { 1 } };
                yield return new object[] { new int[] { 6, 7, 8 } };
            }
        }


        [Theory]
        [MemberData( "TestElements" )]
        public void IsEmpty_returns_expected_value( int[] elements ) {
            bool expectedIsEmpty = (elements.Length == 0);

            var list = CreateLinkList( elements );

            Assert.Equal( expectedIsEmpty, list.IsEmpty );
        }

        [Theory]
        [MemberData( "TestElements" )]
        public void Count_returns_number_of_elements_in_list( int[] elements ) {
            var list = CreateLinkList( elements );

            Assert.Equal( elements.Length, list.Count );
        }

        [Theory]
        [MemberData( "TestElements" )]
        public void ToArray_returns_same_elements_used_to_create_list( int[] elements ) {
            var list = CreateLinkList( elements );
            var array = list.ToArray( );

            Assert.NotNull( array );
            Assert.Equal( elements.Length, array.Length );
            for( int i = 0; i < elements.Length; ++i )
                Assert.Equal( elements[i], array[i] );
        }

        [Theory]
        [MemberData( "TestElements" )]
        public void GetEnumerator_returns_same_elements_used_to_create_list( int[] elements ) {
            var list = CreateLinkList( elements );

            int visited = 0;
            foreach( var item in list ) {
                ++visited;
                Assert.Contains( item, elements );
            }

            Assert.Equal( elements.Length, visited );
        }


        [Fact]
        public void Value_fails_for_empty_list( ) {
            var emptyList = LinkList<bool>.Empty;

            Assert.Throws<InvalidOperationException>( ( ) => { var v = emptyList.Value; } );
        }

        [Fact]
        public void Tail_fails_for_empty_list( ) {
            var emptyList = LinkList<bool>.Empty;

            Assert.Throws<InvalidOperationException>( ( ) => { var t = emptyList.Tail; } );
        }


        private static LinkList<T> CreateLinkList<T>( IEnumerable<T> elements ) {
            var list = LinkList<T>.Empty;
            foreach( T item in elements )
                list = list.Add( item );
            return list;
        }

    }

}
