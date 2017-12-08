
using System;
using System.Linq;
using Xunit;
using Xunit.Extensions;


namespace TypeResolver {

    public class TestBindingCollection {

        [Fact]
        public void ctor_WithNoArguments_ReturnsEmptyCollection( ) {
            var collection = new BindingCollection( );

            Assert.Equal( collection.Count, 0 );
            Assert.Empty( collection );
        }

        [Fact]
        public void ctor_GivenSingleBinding_InitializesCollectionWithBinding( ) {
            var expected = List( typeof( int ) );

            var collection = new BindingCollection( expected );

            Assert.Equal( collection.Count, 1 );
            Assert.Same( Assert.Single( collection ), expected );
        }

        [Fact]
        public void ctor_GivenMultipleBindings_InitializesCollectionWithBindings( ) {
            var a = List( typeof( int ) );
            var b = List( typeof( double ) );
            var c = List( typeof( DateTime ) );
            var expected = new[] { a, b, c };

            var collection = new BindingCollection( expected );
            var actual = collection.ToArray( );

            Assert.Equal( actual, expected );
        }


        [Fact]
        public void Reduce_GivenItemProcessor_UpdatesBindingsAgainstAllInputs( ) {
            var initial = List( typeof( int ) );
            var expected = new[] { List( typeof( Tuple<Tuple<int>> ) ) };
            var collection = new BindingCollection( initial );

            collection.Reduce( Enumerable.Repeat( 1, 2 ), ( b, i ) => List( typeof( Tuple<> ).MakeGenericType( b.Value.Type ) ) );
            var actual = collection.ToArray( );

            Assert.Equal( actual, expected );
        }

        [Fact]
        public void Reduce_GivenItemProcessor_RemovesBindingsWhenProcessorReturnsNull( ) {
            var initial = new[] { List( typeof( int ) ), List( typeof( double ) ) };
            var expected = new[] { initial[1] };
            var collection = new BindingCollection( initial );

            collection.Reduce( Enumerable.Repeat( 1, 2 ), ( b, i ) => i == 1 && b.Value.Type == typeof( int ) ? null : b );
            var actual = collection.ToArray( );

            Assert.Equal( actual, expected );
        }

        [Fact]
        public void Reduce_GivenCollectionProcessor_UpdatesBindingsAgainstAllInputs( ) {
            var initial = List( typeof( int ) );
            var expected = new[] { List( typeof( Tuple<Tuple<int>> ) ) };
            var collection = new BindingCollection( initial );

            collection.Reduce( Enumerable.Repeat( 1, 2 ), ( b, i ) => new[] { List( typeof( Tuple<> ).MakeGenericType( b.Value.Type ) ) } );
            var actual = collection.ToArray( );

            Assert.Equal( actual, expected );
        }

        [Fact]
        public void Reduce_GivenCollectionProcessor_UpdatesAllBindings( ) {
            var initial = List( typeof( int ) );
            var expected = new[] { List( typeof( Tuple<int> ) ) };
            var collection = new BindingCollection( initial );

            collection.Reduce( ( b ) => new[] { List( typeof( Tuple<> ).MakeGenericType( b.Value.Type ) ) } );
            var actual = collection.ToArray( );

            Assert.Equal( actual, expected );
        }


        [Fact]
        public void Expand_GivenCollectionProcessor_UpdatesWithAllBindings( ) {
            var initial = List( typeof( int ) );
            var expected = new[] { List( typeof( Tuple<int> ) ), List( typeof( Tuple<int> ) ) };
            var collection = new BindingCollection( initial );

            collection.Expand( Enumerable.Repeat( 1, 2 ), ( bs, i ) => bs.Reduce( b => List( typeof( Tuple<> ).MakeGenericType( b.Value.Type ) ) ) );
            var actual = collection.ToArray( );

            Assert.Equal( actual, expected );
        }


        [Fact]
        public void Transform_GivenItemTransformer_RemovesNullResults( ) {
            var initial = new[] { List( typeof( int ) ), List( typeof( double ) ) };
            var expected = new[] { initial[1].Value };
            var collection = new BindingCollection( initial );

            var results = collection.Transform( ( b ) => b.Value.Type == typeof( int ) ? null : b.Value );
            var actual = results.ToArray( );

            Assert.Equal( actual, expected );
        }

        [Fact]
        public void Transform_GivenItemTransformer_RemovesDuplicateResults( ) {
            var initial = new[] { List( typeof( double ) ), List( typeof( double ) ) };
            var expected = new[] { initial[1].Value };
            var collection = new BindingCollection( initial );

            var results = collection.Transform( ( b ) => b.Value );
            var actual = results.ToArray( );

            Assert.Equal( actual, expected );
        }


        private static LinkList<Binding> List( Type t ) {
            return LinkList<Binding>.Empty.Add( new Binding( null, t ) );
        }

    }

}
