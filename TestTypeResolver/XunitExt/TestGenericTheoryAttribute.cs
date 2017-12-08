
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;


namespace Xunit.Extensions {

    public class TestGenericTheoryAttribute {

        private static MethodInfo GetMethod( string name ) {
            var method = typeof( TestGenericTheoryAttribute ).GetMethod( name );

            Assert.NotNull( method );

            return method;
        }


        [Theory]
        [InlineData( "ConcreteMethod" )]
        [InlineData( "OneGenericArgument" )]
        [InlineData( "OneGenericParameter" )]
        [InlineData( "TwoGenericArguments" )]
        [InlineData( "TwoDifferentGenericArguments" )]
        [InlineData( "OneComplexGenericParameter" )]
        [InlineData( "TwoComplexGenericArguments" )]
        [InlineData( "TwoComplexGenericParameters" )]
        [InlineData( "PartiallySpecifiedGenericParameter" )]
        [InlineData( "MultipleUnconstrainedParameters" )]
        [InlineData( "UnimplementedParameter" )]
        public void GenericTheory_returns_expected_number_of_test_commands( string methodName ) {
            var methodInfo = GetMethod( methodName );
            var method = Reflector.Wrap( methodInfo );
            var testCollection = new TestCollection( new TestAssembly( Reflector.Wrap( GetType( ).Assembly ) ), null, null );
            var testMethod = new TestMethod( new TestClass( testCollection, Reflector.Wrap( GetType( ) ) ), method );
            var theory = new GenericTheoryAttribute( );
            var attributeInfo = new MockAttributeInfo( theory );
            var discoverer = new GenericTheoryDiscoverer( new NullMessageSink( ) );

            var testCommands = discoverer.Discover( TestFrameworkOptions.ForDiscovery( ), testMethod, attributeInfo );

            Assert.NotNull( testCommands );

            var testCommandsArray = testCommands.ToArray( );
            Assert.NotEmpty( testCommandsArray );
            Assert.Equal( 1, testCommandsArray.Length );
        }


        [InstanceData]
        public void ConcreteMethod( ) { }


        public interface IMethodParameter { }

        public struct MethodParameter : IMethodParameter { }

        [InstanceData]
        public void OneGenericArgument<T>( ) where T : IMethodParameter { }

        [InstanceData]
        public void OneGenericParameter<T>( T parameter ) where T : IMethodParameter { }

        [InstanceData]
        public void TwoGenericArguments<T, U>( )
            where T : IMethodParameter
            where U : IMethodParameter { }


        public interface ISecondMethodParameter { }

        public sealed class SecondMethodParameter : ISecondMethodParameter { }

        [InstanceData]
        public void TwoDifferentGenericArguments<T, U>( )
            where T : IMethodParameter
            where U : ISecondMethodParameter { }


        public interface IComplexParameter<T> { }

        public struct ComplexParameter : IComplexParameter<double> { }

        [InstanceData]
        public void OneComplexGenericParameter<T>( IComplexParameter<T> p ) { }

        [InstanceData]
        public void TwoComplexGenericArguments<T, D>( ) where D : IComplexParameter<T> { }

        [InstanceData]
        public void TwoComplexGenericParameters<T, D>( T t, D d ) where D : IComplexParameter<T> { }


        public interface ITwoComplexParameters<T, U> { }

        public sealed class TwoComplexParameters : ITwoComplexParameters<ComplexParameter, double> { }

        public void PartiallySpecifiedGenericParameter<T, D>( T t, D d )
            where D : ITwoComplexParameters<T, double> { }


        public interface IOneUnconstrainedParameters<T> { }

        public interface ITwoUnconstrainedParameters<T, H> { }

        public sealed class ThreeComplexParameters : IOneUnconstrainedParameters<double>, ITwoUnconstrainedParameters<double, double> { }

        public sealed class MultipleUnconstrainedParametersTarget<T, H, D>
            where D : IOneUnconstrainedParameters<T>, ITwoUnconstrainedParameters<T, H> { }

        public void MultipleUnconstrainedParameters<T, H, D>( MultipleUnconstrainedParametersTarget<T, H, D> p )
            where D : IOneUnconstrainedParameters<T>, ITwoUnconstrainedParameters<T, H> { }


        public interface IUnimplemented<T> { }

        [InstanceData]
        public void UnimplementedParameter<T>( IUnimplemented<T> p ) { }

        private sealed class MockAttributeInfo : IAttributeInfo {
            private readonly Attribute attribute_;

            public MockAttributeInfo( Attribute attribute ) {
                this.attribute_ = attribute;
            }

            public TValue GetNamedArgument<TValue>( string argumentName ) {
                var property = attribute_.GetType( ).GetProperty( argumentName );
                object value = property.GetValue( attribute_ );
                return (TValue)value;
            }

            public IEnumerable<object> GetConstructorArguments( ) { throw new NotSupportedException( ); }
            public IEnumerable<IAttributeInfo> GetCustomAttributes( string assemblyQualifiedAttributeTypeName ) { throw new NotSupportedException( ); }
        }

    }

}
