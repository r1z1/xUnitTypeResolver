
using System;
using System.Linq;
using Xunit;
using Xunit.Extensions;


namespace TypeResolver {

    public class TestBinding {

        [Fact]
        public void Binding_succeeds_for_null_argument( ) {
            var argument = (Type)null;
            var type = typeof( int? );

            var binding = new Binding( argument, type );

            Assert.Same( argument, binding.Argument );
            Assert.Same( type, binding.Type );
        }

        [Fact]
        public void Binding_succeeds_for_generic_argument( ) {
            var argument = typeof( Nullable<> ).GetGenericArguments( )[0];
            var type = typeof( int );

            var binding = new Binding( argument, type );

            Assert.Same( argument, binding.Argument );
            Assert.Same( type, binding.Type );
        }


        [Theory]
        [InlineData( new Type[] { }, typeof( Nullable<> ) )]
        [InlineData( new Type[] { typeof( Nullable<> ) }, typeof( Nullable<> ) )]
        [InlineData( new Type[] { typeof( Nullable<> ) }, typeof( IEquatable<> ) )]
        [InlineData( new Type[] { typeof( IEquatable<> ), typeof( Nullable<> ) }, typeof( Nullable<> ) )]
        public void ForArgument_returns_expected_result( Type[] bindingTypes, Type targetType ) {
            Type argument = targetType.GetGenericArguments( )[0];
            bool isContained = bindingTypes.Contains( argument );
            var bindings = bindingTypes.Aggregate( Binding.EmptyBindings, ( l, t ) => l.Add( new Binding( t.GetGenericArguments( )[0], typeof( int ) ) ) );

            Binding foundBinding = Binding.ForArgument( bindings, targetType );

            if( isContained )
                Assert.NotNull( foundBinding );
            else
                Assert.Null( foundBinding );
        }

        [Theory]
        [InlineData( new Type[] { }, typeof( Nullable<> ) )]
        [InlineData( new Type[] { typeof( Nullable<> ) }, typeof( Nullable<> ) )]
        [InlineData( new Type[] { typeof( Nullable<> ) }, typeof( IEquatable<> ) )]
        [InlineData( new Type[] { typeof( IEquatable<> ), typeof( Nullable<> ) }, typeof( Nullable<> ) )]
        public void ContainsArgument_returns_expected_result( Type[] bindingTypes, Type targetType ) {
            Type argument = targetType.GetGenericArguments( )[0];
            bool isContained = bindingTypes.Contains( argument );
            var bindings = bindingTypes.Aggregate( Binding.EmptyBindings, ( l, t ) => l.Add( new Binding( t.GetGenericArguments( )[0], typeof( int ) ) ) );

            Assert.Equal( isContained, Binding.ContainsType( bindings, targetType ) );
        }

        [Theory]
        [InlineData( new Type[] { }, typeof( int ) )]
        [InlineData( new Type[] { typeof( int ) }, typeof( int ) )]
        [InlineData( new Type[] { typeof( int ) }, typeof( bool ) )]
        [InlineData( new Type[] { typeof( bool ), typeof( int ) }, typeof( int ) )]
        [InlineData( new Type[] { typeof( bool ), typeof( int ) }, typeof( bool ) )]
        [InlineData( new Type[] { typeof( bool ), typeof( int ) }, typeof( long ) )]
        public void ContainsType_returns_expected_result( Type[] bindingTypes, Type targetType ) {
            bool isContained = bindingTypes.Contains( targetType );
            var bindings = bindingTypes.Aggregate( Binding.EmptyBindings, ( l, t ) => l.Add( new Binding( null, t ) ) );

            Assert.Equal( isContained, Binding.ContainsType( bindings, targetType ) );
        }

    }

}
