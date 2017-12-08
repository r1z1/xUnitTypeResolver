
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Extensions;


namespace TypeResolver.Extensions {

    public class TestTypeExtensions {

        private static MethodBase GetMethod( Type type, string name ) {
            MethodBase method = (name == null
                ? (MethodBase)type.GetConstructor( Type.EmptyTypes )
                : (MethodBase)type.GetMethod( name ));

            Assert.NotNull( method );

            return method;
        }


        [Theory]
        [InlineData( null, typeof( object ), false )]
        [InlineData( 0, typeof( int ), true )]
        [InlineData( 0, typeof( int? ), true )]
        [InlineData( 0, typeof( object ), true )]
        [InlineData( "string", typeof( int ), false )]
        [InlineData( "string", typeof( string ), true )]
        public void Is_returns_expected_value_for_simple_objects( object obj, Type targetType, bool derivesFromTarget ) {
            bool result = obj.Is( targetType );

            Assert.Equal( derivesFromTarget, result );
        }

        [Theory]
        [InlineData( null, typeof( object ), false )]
        [InlineData( typeof( int ), typeof( int ), true )]
        [InlineData( typeof( int ), typeof( ValueType ), true )]
        [InlineData( typeof( ValueType ), typeof( int ), false )]
        [InlineData( typeof( int? ), typeof( Nullable<> ), true )]
        [InlineData( typeof( Nullable<> ), typeof( int? ), true )]
        [InlineData( typeof( int? ), typeof( object ), true )]
        public void Is_returns_expected_value_for_types( Type type, Type targetType, bool derivesFromTarget ) {
            bool result = type.Is( targetType );

            Assert.Equal( derivesFromTarget, result );
        }

        [Theory]
        [InlineData( typeof( GenericInterfaceImpl<GenericInterfaceArgument> ), typeof( IGenericInterface<GenericInterfaceArgument> ), true )]
        [InlineData( typeof( GenericInterfaceImpl<GenericInterfaceArgument> ), typeof( IGenericInterface<> ), true )]
        [InlineData( typeof( GenericInterfaceImpl<> ), typeof( IGenericInterface<GenericInterfaceArgument> ), true )]
        [InlineData( typeof( GenericInterfaceImpl<> ), typeof( IGenericInterface<> ), true )]
        [InlineData( typeof( IGenericInterface<GenericInterfaceArgument> ), typeof( GenericInterfaceImpl<GenericInterfaceArgument> ), false )]
        [InlineData( typeof( IGenericInterface<> ), typeof( GenericInterfaceImpl<GenericInterfaceArgument> ), false )]
        [InlineData( typeof( IGenericInterface<GenericInterfaceArgument> ), typeof( GenericInterfaceImpl<> ), false )]
        [InlineData( typeof( IGenericInterface<> ), typeof( GenericInterfaceImpl<> ), false )]
        [InlineData( typeof( GenericClass<NormalClass> ), typeof( GenericClass<> ), true )]
        [InlineData( typeof( GenericClass<> ), typeof( GenericClass<NormalClass> ), true )]
        [InlineData( typeof( DerivedGenericClass<NormalClass> ), typeof( GenericClass<NormalClass> ), true )]
        [InlineData( typeof( DerivedGenericClass<NormalClass> ), typeof( GenericClass<> ), true )]
        [InlineData( typeof( DerivedGenericClass<> ), typeof( GenericClass<NormalClass> ), true )]
        [InlineData( typeof( DerivedGenericClass<> ), typeof( GenericClass<> ), true )]
        [InlineData( typeof( DerivedGenericClass<NormalClass> ), typeof( SiblingGenericClass<NormalClass> ), false )]
        [InlineData( typeof( DerivedGenericClass<NormalClass> ), typeof( SiblingGenericClass<> ), false )]
        [InlineData( typeof( DerivedGenericClass<> ), typeof( SiblingGenericClass<NormalClass> ), false )]
        [InlineData( typeof( DerivedGenericClass<> ), typeof( SiblingGenericClass<> ), false )]
        public void Is_returns_expected_value_for_constrained_generic_types( Type type, Type targetType, bool derivesFromTarget ) {
            bool result = type.Is( targetType );

            Assert.Equal( derivesFromTarget, result );
        }

        public interface INormalInterface { }

        public interface IGenericInterfaceArgument { }

        public interface IGenericInterface<T> where T : IGenericInterfaceArgument { }

        public struct GenericInterfaceArgument : IGenericInterfaceArgument { }

        public struct GenericInterfaceImpl<T> : INormalInterface, IGenericInterface<T> where T : IGenericInterfaceArgument { }


        public abstract class NormalAbstractClass { }

        public sealed class NormalClass : NormalAbstractClass { }

        public class GenericClass<T> where T : NormalAbstractClass { }

        public class DerivedGenericClass<T> : GenericClass<T> where T : NormalAbstractClass { }

        public class SiblingGenericClass<T> : GenericClass<T> where T : NormalAbstractClass { }


        [Theory]
        [InlineData( typeof( int[,] ), typeof( ArrayConstraint<> ), true )]
        [InlineData( typeof( int[] ), typeof( ArrayConstraint<> ), false )]
        [InlineData( typeof( IUnconstrainedGenericInterface<int> ), typeof( InterfaceParameterConstraint<,> ), true )]
        [InlineData( typeof( UnconstrainedGenericInterfaceImpl ), typeof( InterfaceParameterConstraint<,> ), true )]
        [InlineData( typeof( GenericInterfaceArgument ), typeof( InterfaceParameterConstraint<,> ), false )]
        [InlineData( typeof( Nullable<int> ), typeof( InterfaceParameterConstraint<,> ), false )]
        [InlineData( typeof( Nullable<> ), typeof( InterfaceParameterConstraint<,> ), false )]
        public void Is_returns_expected_value_for_constrained_generic_argument_types( Type type, Type constrainingType, bool derivesFromTarget ) {
            Type targetType = GetConstrainingTypeArgument( constrainingType );

            bool result = type.Is( targetType );

            Assert.Equal( derivesFromTarget, result );
        }

        [Theory]
        [InlineData( typeof( IUnconstrainedGenericInterface<> ), typeof( InterfaceParameterConstraint<,> ), true )]
        [InlineData( typeof( Nullable<int> ), typeof( IUnconstrainedGenericInterface<> ), true )]
        [InlineData( typeof( UnconstrainedGenericInterfaceImpl ), typeof( InterfaceParameterConstraint<,> ), false )]
        [InlineData( typeof( GenericInterfaceArgument ), typeof( InterfaceParameterConstraint<,> ), false )]
        [InlineData( typeof( Nullable<int> ), typeof( InterfaceParameterConstraint<,> ), false )]
        [InlineData( typeof( Nullable<> ), typeof( InterfaceParameterConstraint<,> ), false )]
        public void Is_returns_expected_value_for_reverse_constrained_generic_argument_types( Type type, Type constrainingType, bool derivesFromTarget ) {
            Type targetType = GetConstrainingTypeArgument( constrainingType );

            bool result = targetType.Is( type );

            Assert.Equal( derivesFromTarget, result );
        }


        public interface IUnconstrainedGenericInterface<T> { }

        public struct UnconstrainedGenericInterfaceImpl : IUnconstrainedGenericInterface<double> { }

        public struct ArrayConstraint<T> : IUnconstrainedGenericInterface<T[,]> { }

        public struct InterfaceParameterConstraint<D, T> where D : IUnconstrainedGenericInterface<T> { }

        private static Type GetConstrainingTypeArgument( Type constrainingType ) {
            Type targetType = constrainingType.GetGenericArguments( )[0];
            if( targetType.GetGenericParameterConstraints( ).Length == 0 ) {
                Type[] constrainingInterfaces = constrainingType.GetInterfaces( );
                if( constrainingInterfaces.Any( ) ) {
                    targetType = constrainingInterfaces.Single( ).GetGenericArguments( ).Single( );
                }
            }

            return targetType;
        }


        [Theory]
        [InlineData( typeof( int ), typeof( Func<> ), true )]
        [InlineData( typeof( EventArgs ), typeof( Func<> ), true )]
        [InlineData( typeof( int ), typeof( EventHandlerMethod<> ), false )]
        [InlineData( typeof( EventArgs ), typeof( EventHandlerMethod<> ), true )]
        public void Is_returns_expected_value_for_generic_arguments( Type type, Type targetType, bool derivesFromTarget ) {
            Type targetArgument = targetType.GetGenericArguments( )[0];

            bool result = type.Is( targetArgument );

            Assert.Equal( derivesFromTarget, result );
        }

        public delegate void EventHandlerMethod<TEventArgs>( object sender, TEventArgs e ) where TEventArgs : EventArgs;


        [Theory]
        [InlineData( "PartialGenericMethod", typeof( Dictionary<bool, bool> ), false )]
        [InlineData( "PartialGenericMethod", typeof( Dictionary<int, int> ), true )]
        public void Is_returns_expected_value_for_partial_generic_arguments( string methodName, Type type, bool derivesFromTarget ) {
            var method = (MethodInfo)GetMethod( this.GetType( ), methodName );
            Type targetArgument = method.GetGenericArguments( )[0];

            bool result = type.Is( targetArgument );

            Assert.Equal( derivesFromTarget, result );
        }

        public void PartialGenericMethod<T>( )
            where T : IDictionary<int, T> { }


        [Theory]
        [InlineData( typeof( object ), "Object" )]
        [InlineData( typeof( int ), "Int32" )]
        [InlineData( typeof( int? ), "Nullable<Int32>" )]
        [InlineData( typeof( IEnumerable<> ), "IEnumerable<T>" )]
        [InlineData( typeof( Dictionary<int, string> ), "Dictionary<Int32,String>" )]
        public void GetDescriptiveName_succeeds_for_types( Type type, string expectedName ) {
            string name = type.GetDescriptiveName( );

            Assert.Equal( expectedName, name );
        }

        [Theory]
        [InlineData( typeof( Nullable<> ) )]
        [InlineData( typeof( IEnumerable<> ) )]
        [InlineData( typeof( Dictionary<,> ) )]
        public void GetDescriptiveName_succeeds_for_generic_arguments( Type genericType ) {
            string typeDescrptiveName = genericType.GetDescriptiveName( );

            Type[] genericTypeArguments = genericType.GetGenericArguments( );
            Assert.NotEmpty( genericTypeArguments );
            foreach( Type genericArgument in genericTypeArguments ) {
                string expectedName = genericArgument.Name + " on " + typeDescrptiveName;

                string name = genericArgument.GetDescriptiveName( );

                Assert.Equal( expectedName, name );
            }
        }

        [Theory]
        [InlineData( "Concrete" )]
        [InlineData( "GenericArgument" )]
        [InlineData( "GenericParameter" )]
        [InlineData( "ComplexGenericParameter" )]
        [InlineData( "ConcreteReturn" )]
        [InlineData( "GenericReturn" )]
        [InlineData( "ComplexGenericReturn" )]
        public void GetDescriptiveName_succeeds_for_methods( string methodName ) {
            var method = (MethodInfo)GetMethod( this.GetType( ), methodName );

            var name = method.GetDescriptiveName( );

            Assert.NotNull( name );
            Assert.Contains( methodName, name );

            Type[] genericMethodArguments = method.GetGenericArguments( );
            Assert.True( genericMethodArguments.All( ( a ) => name.Contains( "<" + a.Name + ">" ) ) );

            var returnType = method.ReturnType;
            if( returnType != typeof( void ) ) {
                string returnTypeName = (returnType.IsGenericParameter
                    ? returnType.Name
                    : returnType.GetDescriptiveName( ));
                Assert.True( name.EndsWith( " : " + returnTypeName ) );
            }
        }

        [Theory]
        [InlineData( typeof( ConcreteType ) )]
        [InlineData( typeof( GenericType<> ) )]
        [InlineData( typeof( GenericType<int> ) )]
        public void GetDescriptiveName_succeeds_for_constructors( Type type ) {
            var method = (ConstructorInfo)GetMethod( type, null );

            var name = method.GetDescriptiveName( );
            var typeName = type.GetDescriptiveName( );

            Assert.NotNull( name );
            Assert.Contains( typeName, name );
            Assert.True( name.StartsWith( "new " ) );

            Type[] genericMethodArguments = type.GetGenericArguments( );
            Assert.True( genericMethodArguments.All( ( a ) => name.Contains( "<" + a.Name + ">" ) ) );
        }


        public void Concrete( ) { }

        public void GenericArgument<T>( ) { }

        public void GenericParameter<T>( T parameter ) { }

        public void ComplexGenericParameter<T>( IEnumerable<T> parameter ) { }

        public int ConcreteReturn( ) { return default( int ); }

        public T GenericReturn<T>( ) { return default( T ); }

        public IEnumerable<T> ComplexGenericReturn<T>( ) { return default( IEnumerable<T> ); }


        public class ConcreteType { }

        public class GenericType<T> { }

    }

}
