
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Extensions;


namespace TypeResolver {

    public partial class TestGenericTypeResolver {

        private static MethodInfo GetMethod( string name ) {
            var method = typeof( TestGenericTypeResolver ).GetMethod( name );

            Assert.NotNull( method );

            return method;
        }


        [Fact]
        public void GetConcreteMethods_succeeds_for_concrete_method( ) {
            var method = GetMethod( "ConcreteMethod" );

            var concreteMethods = GenericTypeResolver.GetConcreteMethods( method );

            Assert.Equal( method, Assert.Single( concreteMethods ) );
        }

        [Theory]
        // Naming convention: <method name>#_<generic argument count>_<generic parameter count>
        [InlineData( "GenericMethod_SimpleConstraint_1_0", new[] { typeof( SimpleParameter ) } )]
        [InlineData( "GenericMethod_SimpleConstraint_1_1", new[] { typeof( SimpleParameter ) } )]
        [InlineData( "GenericMethod_SimpleConstraint_2_0", new[] { typeof( SimpleParameter ), typeof( SimpleParameter ) } )]
        [InlineData( "GenericMethod_SecondSimpleConstraint_2_0", new[] { typeof( SimpleParameter ), typeof( SecondSimpleParameter ) } )]
        [InlineData( "GenericMethod_GenericConstraint1_1_1", new[] { typeof( double ) } )]
        [InlineData( "GenericMethod_GenericConstraint1_2_0", new[] { typeof( double ), typeof( GenericParameter1 ) } )]
        [InlineData( "GenericMethod_GenericConstraint1_2_2", new[] { typeof( double ), typeof( GenericParameter1 ) } )]
        [InlineData( "GenericMethod_GenericConstraint2_2_2", new[] { typeof( GenericParameter1 ), typeof( GenericParameter2 ) } )]
        [InlineData( "GenericMethod_GenericConstraint2_1_1", new[] { typeof( GenericParameter2 ) } )]
        [InlineData( "GenericMethod_PartialConstraints1_3_1", new[] { typeof( double ), typeof( double ), typeof( GenericParameter3 ) } )]
        [InlineData( "GenericMethod_PartialConstraints2_3_1", new[] { typeof( double ), typeof( double ), typeof( GenericParameter4 ) } )]
        [InlineData( "GenericMethod_ImpliedConstraint1_1_1", new[] { typeof( GenericParameter6<long> ) } )]
        [InlineData( "GenericMethod_UnconstrainedType1_1_1", new[] { typeof( ulong ) } )]
        [InlineData( "GenericMethod_UnconstrainedType1_1_2", new[] { typeof( ulong ) } )]
        [InlineData( "GenericMethod_UnconstrainedType2_1_2", new[] { typeof( int ) } )]
        [InlineData( "GenericMethod_UnconstrainedType3_1_2", null )]
        [InlineData( "GenericMethod_UnconstrainedType3_2_2", new[] { typeof( ulong ), typeof( int ) } )]
        [InlineData( "UnimplementedParameter", null )]
        public void GetConcreteMethods_succeeds_for_generic_method( string methodName, Type[] genericArguments ) {
            var method = GetMethod( methodName );

            var concreteMethods = GenericTypeResolver.GetConcreteMethods( method )
                .ToArray( );

            Assert.NotNull( concreteMethods );
            if( genericArguments == null ) {
                Assert.Empty( concreteMethods );
            }
            else {
                Assert.NotEmpty( concreteMethods );
                Assert.Equal( 1, concreteMethods.Length );

                var constructedMethod = method.MakeGenericMethod( genericArguments );
                Assert.Equal( constructedMethod, concreteMethods[0] );

                foreach( var parameter in constructedMethod.GetParameters( ) ) {
                    var instances = TypeCreator.GetCreators( parameter.ParameterType );
                    Assert.Equal( 1, instances.Count );
                }
            }
        }

        [Fact]
        public void GetConcreteMethods_succeeds_for_generic_method_with_multiple_resolutions( ) {
            const string methodName = "GenericMethod_MultipleFactoryResolutions";
            var method = GetMethod( methodName );

            var concreteMethods = GenericTypeResolver.GetConcreteMethods( method )
                .ToArray( );

            Assert.Equal( 2, concreteMethods.Length );

            foreach( var concreteMethod in concreteMethods ) {
                var parameter = concreteMethod.GetParameters( ).Single( );
                var instances = TypeCreator.GetCreators( parameter.ParameterType );
                Assert.Equal( 1, instances.Count );
            }
        }

        [Theory]
        [MemberData( "MultipleGenericMethodResolutions" )]
        public void GetConcreteMethods_succeeds_for_generic_method_with_multiple_generic_resolutions( MethodInfo method ) {
            Type[] resolutionArguments = new[] { typeof( int ), typeof( double ) };

            var concreteMethods = GenericTypeResolver.GetConcreteMethods( method )
                .ToArray( );

            Assert.NotNull( concreteMethods );
            Assert.NotEmpty( concreteMethods );
            Assert.Equal( 2, concreteMethods.Length );

            foreach( var m in concreteMethods ) {
                Type[] genericArguments = m.GetGenericArguments( );
                Assert.NotEmpty( genericArguments );
                Assert.Contains( genericArguments[0], resolutionArguments );
            }
        }

        [Fact( Skip = "Generic class methods do not work." )]
        public void GetConcreteMethods_succeeds_for_generic_class_method( ) {
            string methodName = "GenericMethod";
            Type type = typeof( GenericMethodClass<> );
            var method = type.GetMethod( methodName );
            Assert.NotNull( method );

            var concreteMethods = GenericTypeResolver.GetConcreteMethods( method );

            var constructedType = type.MakeGenericType( typeof( SimpleParameter ) );
            var constructedMethod = type.GetMethod( methodName );
            Assert.Equal( constructedMethod, Assert.Single( concreteMethods ) );
        }



        public void ConcreteMethod( ) { }


        public interface ISimpleConstraint { }

        public struct SimpleParameter : ISimpleConstraint { }

        public void GenericMethod_SimpleConstraint_1_0<T>( ) where T : ISimpleConstraint { }

        public void GenericMethod_SimpleConstraint_1_1<T>( T parameter ) where T : ISimpleConstraint { }

        public void GenericMethod_SimpleConstraint_2_0<T, U>( )
            where T : ISimpleConstraint
            where U : ISimpleConstraint { }


        public interface ISecondSimpleParameter { }

        public sealed class SecondSimpleParameter : ISecondSimpleParameter { }

        public void GenericMethod_SecondSimpleConstraint_2_0<T, U>( )
            where T : ISimpleConstraint
            where U : ISecondSimpleParameter { }


        public interface IGenericConstraint1<T> { }

        public struct GenericParameter1 : IGenericConstraint1<double> { }

        public void GenericMethod_GenericConstraint1_1_1<T>( IGenericConstraint1<T> p ) { }

        public void GenericMethod_GenericConstraint1_2_0<T, D>( ) where D : IGenericConstraint1<T> { }

        public void GenericMethod_GenericConstraint1_2_2<T, D>( T t, D d ) where D : IGenericConstraint1<T> { }


        public interface IGenericConstraint2<T, U> { }

        public sealed class GenericParameter2 : IGenericConstraint2<GenericParameter1, double> { }

        public void GenericMethod_GenericConstraint2_2_2<T, D>( T t, D d )
            where D : IGenericConstraint2<T, double> { }

        public void GenericMethod_GenericConstraint2_1_1<D>( D d )
            where D : IGenericConstraint2<GenericParameter1, double> { }


        public interface IGenericConstraint3<T> { }

        public interface IGenericConstraint4<T, H> { }

        public sealed class GenericParameter3 : IGenericConstraint3<double>, IGenericConstraint4<double, double> { }

        public sealed class GenericTargetParameter1<T, H, D>
            where D : IGenericConstraint3<T>, IGenericConstraint4<T, H> { }

        public void GenericMethod_PartialConstraints1_3_1<T, H, D>( GenericTargetParameter1<T, H, D> p )
            where D : IGenericConstraint3<T>, IGenericConstraint4<T, H> { }


        public interface IGenericConstraint5<T, H> { }

        public interface IGenericConstraint6<T, H> { }

        public struct GenericParameter4 : IGenericConstraint3<double>, IGenericConstraint5<double, double>, IGenericConstraint6<double, long> { }

        public struct GenericParameter5 : IGenericConstraint3<double>, IGenericConstraint5<double, double>, IGenericConstraint6<double, double> { }

        public void GenericMethod_PartialConstraints2_3_1<T, H, D>( )
            where D : IGenericConstraint3<T>, IGenericConstraint5<T, H>, IGenericConstraint6<T, long> { }


        public interface IGenericConstraint7<T> { }

        public struct GenericParameter6<T> : IGenericConstraint7<T> { }

        public void GenericMethod_ImpliedConstraint1_1_1<D>( D d )
            where D : IGenericConstraint7<long> { }


        public class UnconstrainedGenericType1<T> { internal UnconstrainedGenericType1( ) { } }
        public class UnconstrainedGenericType2<T> { internal UnconstrainedGenericType2( ) { } }
        public class UnconstrainedGenericType3<T> { internal UnconstrainedGenericType3( ) { } }
        public class UnconstrainedGenericType4<T> { internal UnconstrainedGenericType4( ) { } }
        public class UnconstrainedGenericType5<T> { internal UnconstrainedGenericType5( ) { } }

        public static class FactoryOfUnconstrainedGenericType1_ulong {
            public static IEnumerable<Func<UnconstrainedGenericType1<ulong>>> GetInstances( ) { yield return ( ) => new UnconstrainedGenericType1<ulong>( ); }
        }

        public static class FactoryOfUnconstrainedGenericType2_ulong {
            public static IEnumerable<Func<UnconstrainedGenericType2<ulong>>> GetInstances( ) { yield return ( ) => new UnconstrainedGenericType2<ulong>( ); }
        }
        public static class FactoryOfUnconstrainedGenericType2_int {
            public static IEnumerable<Func<UnconstrainedGenericType2<int>>> GetInstances( ) { yield return ( ) => new UnconstrainedGenericType2<int>( ); }
        }

        public static class FactoryOfUnconstrainedGenericType3_int {
            public static IEnumerable<Func<UnconstrainedGenericType3<int>>> GetInstances( ) { yield return ( ) => new UnconstrainedGenericType3<int>( ); }
        }
        public static class FactoryOfUnconstrainedGenericType3_string {
            public static IEnumerable<Func<UnconstrainedGenericType3<string>>> GetInstances( ) { yield return ( ) => new UnconstrainedGenericType3<string>( ); }
        }

        public static class FactoryOfUnconstrainedGenericType4_ulong {
            public static IEnumerable<Func<UnconstrainedGenericType4<ulong>>> GetInstances( ) { yield return ( ) => new UnconstrainedGenericType4<ulong>( ); }
        }

        public static class FactoryOfUnconstrainedGenericType5_int {
            public static IEnumerable<Func<UnconstrainedGenericType5<int>>> GetInstances( ) { yield return ( ) => new UnconstrainedGenericType5<int>( ); }
        }

        public void GenericMethod_UnconstrainedType1_1_1<T>( UnconstrainedGenericType1<T> u ) { }

        public void GenericMethod_UnconstrainedType1_1_2<T>( UnconstrainedGenericType1<T> u, T t ) { }

        public void GenericMethod_UnconstrainedType2_1_2<T>( UnconstrainedGenericType2<T> u1, UnconstrainedGenericType3<T> u2 ) { }

        public void GenericMethod_UnconstrainedType3_1_2<T>( UnconstrainedGenericType4<T> u1, UnconstrainedGenericType5<T> u2 ) { }

        public void GenericMethod_UnconstrainedType3_2_2<T, U>( UnconstrainedGenericType4<T> u1, UnconstrainedGenericType5<U> u2 ) { }


        public interface IUnimplemented<T> { }

        public void UnimplementedParameter<T>( IUnimplemented<T> p ) { }


        public class GenericMethodClass<T> where T : ISimpleConstraint {
            public void GenericMethod( T item ) { }
        }


        public interface IMultipleFactoryResolutions<T> { }
        public interface IMultipleFactoryResolutionsParameter<T> { }

        public struct MultipleFactoryResolutionsParameterInt32 : IMultipleFactoryResolutionsParameter<int> { }
        public struct MultipleFactoryResolutionsParameterDouble : IMultipleFactoryResolutionsParameter<double> { }

        public class MultipleFactoryResolutions<T> : IMultipleFactoryResolutions<T> { internal MultipleFactoryResolutions( IMultipleFactoryResolutionsParameter<T> arg ) { } }

        public static class FactoryOfMultipleFactoryResolutions {
            public static IEnumerable<Func<IMultipleFactoryResolutions<T>>> GetMultiInstances<T, D>( D arg ) where D : IMultipleFactoryResolutionsParameter<T> { yield return ( ) => new MultipleFactoryResolutions<T>( arg ); }
        }

        public void GenericMethod_MultipleFactoryResolutions<T>( IMultipleFactoryResolutions<T> arg ) { }


        public interface IMultipleResolutionsConstraint<T> { }
        public interface IMultipleResolutionsConstraint<T, H> { }

        public struct MultipleResolutionsGenericParameterInt32 : IMultipleResolutionsConstraint<int>, IMultipleResolutionsConstraint<int, int> { }
        public struct MultipleResolutionsGenericParameterDouble : IMultipleResolutionsConstraint<double>, IMultipleResolutionsConstraint<double, double> { }

        public interface IMultipleResolutions<T> { }
        public abstract class MultipleResolutions<T, D> : IMultipleResolutions<T> where D : IMultipleResolutionsConstraint<T> { protected MultipleResolutions( ) { } }
        public sealed class MultipleResolutionsImpl<T, H, D> : MultipleResolutions<T, D> where D : IMultipleResolutionsConstraint<T>, IMultipleResolutionsConstraint<T, H> { public MultipleResolutionsImpl( ) { } }

        public void GenericMethod_MultipleGenericResolutions_Interface<T>( IMultipleResolutions<T> resolved ) { }
        public void GenericMethod_MultipleGenericResolutions_Type<T, D>( MultipleResolutions<T, D> resolved ) where D : IMultipleResolutionsConstraint<T> { }

        public static IEnumerable<object[]> MultipleGenericMethodResolutions {
            get {
                var methodNames = new[] { "GenericMethod_MultipleGenericResolutions_Interface", "GenericMethod_MultipleGenericResolutions_Type" };
                foreach( string methodName in methodNames ) {
                    MethodInfo method = GetMethod( methodName );
                    yield return new object[] { method };
                }
            }
        }

    }

}
