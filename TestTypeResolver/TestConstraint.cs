
using System;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Extensions;


namespace TypeResolver {

    public class TestConstraint {

        [Theory]
        [InlineData( typeof( int ), true )]
        [InlineData( typeof( int? ), true )]
        [InlineData( typeof( ImplicitConstructor ), true )]
        [InlineData( typeof( ExplicitConstructor ), true )]
        [InlineData( typeof( PrivateConstructor ), false )]
        [InlineData( typeof( ParameterizedConstructor ), false )]
        public void HasDefaultConstructor_returns_expected_result( Type type, bool hasDefaultConstructor ) {
            Assert.Equal( hasDefaultConstructor, Constraint.HasDefaultConstructor( type ) );
        }

        [Theory]
        [InlineData( typeof( int ), true )]
        [InlineData( typeof( int? ), false )]
        [InlineData( typeof( ImplicitConstructor ), false )]
        public void IsNonNullableValueType_returns_expected_result( Type type, bool isNonNullableValueType ) {
            Assert.Equal( isNonNullableValueType, Constraint.IsNonNullableValueType( type ) );
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IComparable ) )]
        [InlineData( typeof( Exception ) )]
        public void GetInheritanceConstraint_succeeds_for_valid_types( Type baseType ) {
            var constraint = Constraint.GetInheritanceConstraint( baseType );

            Assert.NotNull( constraint );
        }

        [Theory]
        [InlineData( typeof( GenericTypeOfClass<> ), 1 )]
        [InlineData( typeof( GenericTypeOfStruct<> ), 3 )]  // "where T : struct" => is value type, has default constructor, inherits from ValueType
        [InlineData( typeof( GenericTypeOfConstructor<> ), 1 )]
        [InlineData( typeof( GenericTypeOfInterface<> ), 1 )]
        [InlineData( typeof( GenericTypeOfBaseClass<> ), 1 )]
        public void GetConstraints_returns_expected_result( Type sourceType, int expectedCount ) {
            Type argumentType = sourceType.GetGenericArguments( )[0];
            Assembly[] referencedAssemblies;

            var constraints = Constraint.GetConstraints( argumentType, out referencedAssemblies );

            Assert.NotNull( constraints );
            Assert.Equal( expectedCount, constraints.Count );

            Assert.NotNull( referencedAssemblies );
        }

        [Theory]
        [InlineData( typeof( GenericTypeOfClass<> ), typeof( int ), false )]
        [InlineData( typeof( GenericTypeOfClass<> ), typeof( int? ), false )]
        [InlineData( typeof( GenericTypeOfClass<> ), typeof( object ), true )]
        [InlineData( typeof( GenericTypeOfStruct<> ), typeof( int ), true )]
        [InlineData( typeof( GenericTypeOfStruct<> ), typeof( int? ), false )]
        [InlineData( typeof( GenericTypeOfStruct<> ), typeof( object ), false )]
        [InlineData( typeof( GenericTypeOfConstructor<> ), typeof( int ), true )]
        [InlineData( typeof( GenericTypeOfConstructor<> ), typeof( int? ), true )]
        [InlineData( typeof( GenericTypeOfConstructor<> ), typeof( object ), true )]
        [InlineData( typeof( GenericTypeOfConstructor<> ), typeof( PrivateConstructor ), false )]
        [InlineData( typeof( GenericTypeOfInterface<> ), typeof( int ), true )]
        [InlineData( typeof( GenericTypeOfInterface<> ), typeof( object ), false )]
        [InlineData( typeof( GenericTypeOfInterface<> ), typeof( IComparable ), true )]
        [InlineData( typeof( GenericTypeOfBaseClass<> ), typeof( int ), false )]
        [InlineData( typeof( GenericTypeOfBaseClass<> ), typeof( object ), false )]
        [InlineData( typeof( GenericTypeOfBaseClass<> ), typeof( Exception ), true )]
        [InlineData( typeof( GenericTypeOfBaseClass<> ), typeof( ArgumentException ), true )]
        public void Satisfy_returns_expected_result( Type sourceType, Type boundType, bool isSatisfied ) {
            Type argumentType = sourceType.GetGenericArguments( )[0];
            Assembly[] referencedAssemblies;

            var constraints = Constraint.GetConstraints( argumentType, out referencedAssemblies );
            var bindings = new BindingCollection( Binding.EmptyBindings );

            Constraint.SatisfyConstraints( bindings, boundType, constraints );
            bool satisfied = bindings.Any( );

            Assert.Equal( isSatisfied, satisfied );
        }


        private sealed class ImplicitConstructor { }

        private sealed class ExplicitConstructor {
            public ExplicitConstructor( ) { }
        }

        private sealed class PrivateConstructor {
            private PrivateConstructor( ) { }
        }

        private sealed class ParameterizedConstructor {
            public ParameterizedConstructor( int parameter ) { }
        }


        private sealed class GenericTypeOfClass<T> where T : class { }

        private sealed class GenericTypeOfStruct<T> where T : struct { }

        private sealed class GenericTypeOfConstructor<T> where T : new( ) { }

        private sealed class GenericTypeOfInterface<T> where T : IComparable { }

        private sealed class GenericTypeOfBaseClass<T> where T : Exception { }

    }

}
