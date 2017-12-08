
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Extensions;


namespace TypeResolver {

    public partial class TestGenericTypeResolver {

        [Theory]
        [InlineData( typeof( int ), 0 )]
        [InlineData( typeof( int? ), 0 )]
        [InlineData( typeof( Nullable<> ), 1 )]
        [InlineData( typeof( ICollection<> ), 1 )]
        [InlineData( typeof( IDictionary<,> ), 2 )]
        [InlineData( typeof( IDictionary<int, int> ), 0 )]
        public void GetOpenGenericArguments_returns_expected_number_of_arguments_for_type( Type type, int expectedCount ) {
            var arguments = GenericTypeResolver.GetOpenGenericArguments( type );

            Assert.NotNull( arguments );
            Assert.Equal( expectedCount, arguments.Count );
        }


        [Fact]
        public void CreateConcreteType_succeeds_for_concrete_type( ) {
            Type type = typeof( int );
            var openArguments = GenericTypeResolver.GetOpenGenericArguments( type );
            var bindings = Binding.EmptyBindings;

            var concreteType = GenericTypeResolver.CreateConcreteType( type, openArguments, bindings );

            Assert.NotNull( concreteType );
            Assert.Equal( type, concreteType );
        }

        [Fact]
        public void CreateConcreteType_returns_null_for_generic_type_with_no_bindings( ) {
            Type type = typeof( Nullable<> );
            var openArguments = GenericTypeResolver.GetOpenGenericArguments( type );
            var bindings = Binding.EmptyBindings;

            var concreteType = GenericTypeResolver.CreateConcreteType( type, openArguments, bindings );

            Assert.Null( concreteType );
        }

        [Fact]
        public void CreateConcreteType_returns_null_for_generic_type_with_partial_bindings( ) {
            Type type = typeof( IDictionary<,> );
            var openArguments = GenericTypeResolver.GetOpenGenericArguments( type );
            var bindings = Binding.EmptyBindings.Add( new Binding( openArguments.Value, typeof( int ) ) );

            var concreteType = GenericTypeResolver.CreateConcreteType( type, openArguments, bindings );

            Assert.Null( concreteType );
        }

        [Theory]
        [InlineData( typeof( Nullable<> ) )]
        [InlineData( typeof( ICollection<> ) )]
        [InlineData( typeof( IDictionary<,> ) )]
        public void CreateConcreteType_succeeds_for_generic_type_with_full_bindings( Type genericType ) {
            var openArguments = GenericTypeResolver.GetOpenGenericArguments( genericType );
            var bindings = GetBindingList( openArguments.Select( ( a, i ) => new Binding( a, typeof( int ) ) ) );

            var concreteType = GenericTypeResolver.CreateConcreteType( genericType, openArguments, bindings );

            Assert.NotNull( concreteType );
            Assert.Equal( genericType, concreteType.GetGenericTypeDefinition( ) );

            Type[] typeArguments = new Type[bindings.Count];
            for( int i = 0; i < typeArguments.Length; ++i )
                typeArguments[i] = typeof( int );
            var constructedType = genericType.MakeGenericType( typeArguments );
            Assert.Equal( constructedType, concreteType );
        }

        [Fact]
        public void CreateConcreteType_succeeds_for_generic_type_with_duplicate_bindings( ) {
            Type genericType = typeof( Nullable<> );
            var openArguments = GenericTypeResolver.GetOpenGenericArguments( genericType );
            var bindings = GetBindingList( new[] { new Binding( openArguments.Value, typeof( int ) ), new Binding( openArguments.Value, typeof( int ) ) } );

            var concreteType = GenericTypeResolver.CreateConcreteType( genericType, openArguments, bindings );

            Assert.NotNull( concreteType );
            Assert.Equal( genericType, concreteType.GetGenericTypeDefinition( ) );

            var constructedType = genericType.MakeGenericType( typeof( int ) );
            Assert.Equal( constructedType, concreteType );
        }


        private static LinkList<Binding> GetBindingList( IEnumerable<Binding> bindings ) {
            return bindings.Aggregate( Binding.EmptyBindings, ( l, b ) => l.Add( b ) );
        }

    }

}
