
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using TypeResolver.Extensions;
using Xunit;
using Xunit.Extensions;

[assembly: InstantiateInstanceData( typeof( TypeResolver.TestTypeCreator.InstantiateInstanceDataType ) )]
[assembly: LimitInstanceData( typeof( TypeResolver.TestTypeCreator.AssemblyExcludedType ) )]
[assembly: ExcludeAssembly( typeof( System.Threading.ReaderWriterLockSlim ) )]


namespace TypeResolver {

    public class TestTypeCreator {

        private static void TestCreators( Type baseType, Type[] expectedInstanceTypes, ReadOnlyCollection<IInstanceCreator> creators ) {
            Assert.NotNull( creators );

            bool sufficientCreators = (creators.Count >= expectedInstanceTypes.Length);
            Assert.True( sufficientCreators );

            foreach( var creator in creators ) {
                Assert.NotNull( creator );

                bool isCorrectType = (baseType.ContainsGenericParameters || baseType.IsAssignableFrom( creator.InstanceType ));
                Assert.True( isCorrectType );

                var instance = creator.CreateInstance( );
                Assert.NotNull( instance );

                Type instanceType = instance.GetType( );
                bool isExpectedType = expectedInstanceTypes.Any( ( t ) => t.IsAssignableFrom( instanceType ) );
                Assert.True( isExpectedType );
            }
        }


        [Theory]
        [InlineData( typeof( bool ), new object[] { false, true } )]
        [InlineData( typeof( DateTimeKind ), new object[] { DateTimeKind.Unspecified, DateTimeKind.Utc, DateTimeKind.Local } )]
        public void GetCreators_returns_all_enum_values( Type enumType, object[] values ) {
            var creators = TypeCreator.GetCreators( enumType );

            Assert.Equal( values.Length, creators.Count );

            foreach( var creator in creators ) {
                Assert.Equal( enumType, creator.InstanceType );

                var value = creator.CreateInstance( );
                Assert.Contains( value, values );
            }
        }

        [Theory]
        [InlineData( typeof( SealedStruct ) )]
        [InlineData( typeof( DerivedSealedClass ) )]
        public void GetCreators_returns_single_creator_for_sealed_types( Type sealedBaseType ) {
            var expectedInstanceTypes = new[] { sealedBaseType };

            var creators = TypeCreator.GetCreators( sealedBaseType );

            TestCreators( sealedBaseType, expectedInstanceTypes, creators );
        }

        [Fact]
        public void GetCreators_returns_single_derived_type_for_abstract_class( ) {
            var type = typeof( AbstractBaseClass );
            var expectedInstanceTypes = new[] { typeof( DerivedSealedClass ) };

            var creators = TypeCreator.GetCreators( type );

            TestCreators( type, expectedInstanceTypes, creators );
        }

        [Fact]
        public void GetCreators_returns_single_derived_type_for_abstract_class_constrained_type_argument( ) {
            var type = typeof( AbstractBaseClassTypeParameter<> ).GetGenericArguments( ).Single( );
            var expectedInstanceTypes = new[] { typeof( DerivedSealedClass ) };

            var creators = TypeCreator.GetCreators( type );

            TestCreators( type, expectedInstanceTypes, creators );
        }

        [Fact]
        public void GetCreators_returns_all_derived_types_for_base_class( ) {
            var type = typeof( BaseClass );
            var expectedInstanceTypes = new[] { type, typeof( DerivedClass ) };

            var creators = TypeCreator.GetCreators( type );

            TestCreators( type, expectedInstanceTypes, creators );
        }

        [Theory]
        [InlineData( typeof( ParameterizedConstructorClass ) )]
        [InlineData( typeof( ParameterizedConstructorStruct ) )]
        public void GetCreators_returns_creator_for_types_with_parameterized_constructors( Type type ) {
            var expectedInstanceTypes = new[] { type };

            var creators = TypeCreator.GetCreators( type );

            TestCreators( type, expectedInstanceTypes, creators );
        }

        [Theory]
        [InlineData( typeof( TooManyParameterizedConstructorsClass ) )]
        [InlineData( typeof( TooManyParameterizedConstructorsStruct ) )]
        [InlineData( typeof( DateTime ) )]
        [InlineData( typeof( TimeSpan ) )]
        public void GetCreators_returns_only_default_constructor_creator_for_types_with_too_many_parameterized_constructors( Type type ) {
            var expectedInstanceTypes = new[] { type };

            var creators = TypeCreator.GetCreators( type );

            TestCreators( type, expectedInstanceTypes, creators );
            var creator = Assert.Single( creators );
            Assert.NotNull( creator );
        }

        [Fact]
        public void GetCreators_ignores_recursive_parameters( ) {
            var type = typeof( RecursiveClass );
            var expectedInstanceTypes = Type.EmptyTypes;

            var creators = TypeCreator.GetCreators( type );

            TestCreators( type, expectedInstanceTypes, creators );
        }

        [Fact]
        public void GetCreators_returns_expected_instances_from_factory_class( ) {
            var type = typeof( ClassFromFactory );
            var expectedInstanceTypes = new[] { type };

            var creators = TypeCreator.GetCreators( type );

            TestCreators( type, expectedInstanceTypes, creators );
        }

        [Theory]
        [MemberData( "GenericTypes" )]
        public void GetCreators_returns_expected_instances_for_generic_types( Type genericType, Type[] expectedInstanceTypes ) {
            Type[] arguments = new Type[genericType.GetGenericArguments( ).Length];
            if( arguments.Length > 2 )
                return;

            arguments[0] = typeof( double );
            for( int i = 0; i < expectedInstanceTypes.Length; ++i ) {
                arguments[arguments.Length - 1] = expectedInstanceTypes[i];
                expectedInstanceTypes[i] = genericType.MakeGenericType( arguments );
            }

            var creators = TypeCreator.GetCreators( genericType );

            TestCreators( genericType, expectedInstanceTypes, creators );
        }

        [Theory]
        [MemberData( "MultipleGenericMethodResolutions" )]
        public void GetCreators_returns_expected_instances_for_generic_method_argument_types( MethodInfo genericMethod ) {
            var concreteMethods = GenericTypeResolver.GetConcreteMethods( genericMethod );
            foreach( var method in concreteMethods ) {
                Type genericType = method.GetParameters( ).Single( ).ParameterType;

                var creators = TypeCreator.GetCreators( genericType );

                TestCreators( genericType, new[] { genericType }, creators );
            }
        }

        [Theory]
        [InlineData( typeof( IMultipleResolutions<> ), new[] { typeof( MultipleResolutions<int, MultipleResolutionsParameterInt32> ), typeof( MultipleResolutions<double, MultipleResolutionsParameterDouble> ) } )]
        [InlineData( typeof( IMultipleResolutions<int> ), new[] { typeof( MultipleResolutions<int, MultipleResolutionsParameterInt32> ) } )]
        [InlineData( typeof( IMultipleResolutions<double> ), new[] { typeof( MultipleResolutions<double, MultipleResolutionsParameterDouble> ) } )]
        public void GetCreators_returns_expected_instances_for_generic_type_with_multiple_resolutions( Type genericType, Type[] expectedInstanceTypes ) {
            var creators = TypeCreator.GetCreators( genericType );

            TestCreators( genericType, expectedInstanceTypes, creators );
        }

        [Theory]
        [InlineData( typeof( IConstructorParameterResolution<> ), new[] { typeof( ConstructorParameterResolution ) } )]
        [InlineData( typeof( IConstructorParameterResolution<bool> ), new[] { typeof( ConstructorParameterResolution ) } )]
        public void GetCreators_returns_expected_instances_for_generic_type_with_constructor_parameters( Type type, Type[] expectedInstanceTypes ) {
            var creators = TypeCreator.GetCreators( type );

            TestCreators( type, expectedInstanceTypes, creators );
        }

        [Fact]
        public void GetCreators_returns_expected_instances_for_interface_implemented_by_generic_type( ) {
            Type targetType = typeof( ITargetInterface );
            Type[] expectedInstanceTypes = new[] { typeof( TargetInterfaceImpl<double, ParameterInterfaceImpl> ) };

            var creators = TypeCreator.GetCreators( targetType );

            TestCreators( targetType, expectedInstanceTypes, creators );
        }

        [Fact]
        public void GetCreators_returns_expected_instances_for_unconstrained_generic_type( ) {
            Type targetType = typeof( UnconstrainedGenericType<> );
            Type[] expectedInstanceTypes = new[] { typeof( UnconstrainedGenericType<bool> ) };

            var creators = TypeCreator.GetCreators( targetType );

            TestCreators( targetType, expectedInstanceTypes, creators );
        }

        [Fact]
        public void GetCreators_returns_expected_instances_for_generic_factory_method( ) {
            Type targetType = typeof( IFactoryInterfaceWrapper );
            Type[] expectedInstanceTypes = new[] { typeof( FactoryInterfaceWrapperImpl ) };

            var creators = TypeCreator.GetCreators( targetType );

            TestCreators( targetType, expectedInstanceTypes, creators );
        }

        [Fact]
        public void GetCreators_returns_expected_instances_for_generic_factory_method_returning_generic_type( ) {
            Type targetType = typeof( IGenericFactoryInterfaceWrapper<> );
            Type[] expectedInstanceTypes = new[] { typeof( GenericFactoryInterfaceWrapperImpl<double> ) };

            var creators = TypeCreator.GetCreators( targetType );

            TestCreators( targetType, expectedInstanceTypes, creators );
        }

        [Fact]
        public void GetCreators_returns_expected_instances_for_generic_factory_taking_IInstanceCreator_argument( ) {
            Type targetType = typeof( ILazyGenericFactoryInterfaceWrapper<> );
            Type[] expectedInstanceTypes = new[] { typeof( LazyGenericFactoryInterfaceWrapperImpl<double> ) };

            var creators = TypeCreator.GetCreators( targetType );

            TestCreators( targetType, expectedInstanceTypes, creators );
        }

        [Fact]
        public void GetCreators_returns_expected_instances_for_generic_factory_taking_partial_generic_argument( ) {
            Type targetType = typeof( PartialGenericArgumentWrapperImpl<> );
            Type[] expectedInstanceTypes = new[] { typeof( PartialGenericArgumentWrapperImpl<string> ), typeof( PartialGenericArgumentWrapperImpl<int> ) };

            var creators = TypeCreator.GetCreators( targetType );

            TestCreators( targetType, expectedInstanceTypes, creators );
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( SealedStruct ) )]
        [InlineData( typeof( DerivedSealedClass ) )]
        [InlineData( typeof( AbstractBaseClass ) )]
        [InlineData( typeof( BaseClass ) )]
        [InlineData( typeof( ParameterizedConstructorClass ) )]
        [InlineData( typeof( ParameterizedConstructorStruct ) )]
        [InlineData( typeof( ClassFromFactory ) )]
        [InlineData( typeof( ITargetInterface ) )]
        [InlineData( typeof( IFactoryInterfaceWrapper ) )]
        [InlineData( typeof( IGenericFactoryInterfaceWrapper<> ) )]
        [InlineData( typeof( ILazyGenericFactoryInterfaceWrapper<> ) )]
        [InlineData( typeof( PartialGenericArgumentWrapperImpl<> ) )]
        public void GetCreators_returns_expected_instances_for_generic_IInstanceCreator( Type innerTargetType ) {
            Type targetType = typeof( IInstanceCreator<> ).MakeGenericType( innerTargetType );

            var innerCreators = TypeCreator.GetCreators( innerTargetType );
            var creators = TypeCreator.GetCreators( targetType );

            Assert.NotNull( creators );
            Assert.NotNull( innerCreators );
            Assert.Equal( innerCreators.Count, creators.Count );

            foreach( var creator in creators ) {
                Assert.True( creator.InstanceType.Is( typeof( IInstanceCreator ) ) );

                var inner = (IInstanceCreator)creator.CreateInstance( );
                Assert.True( inner.InstanceType.Is( innerTargetType ) );
            }
        }


        [Fact]
        public void GetCreators_returns_empty_collection_for_excluded_type( ) {
            TypeCreator.LimitInstances( typeof( ExcludedType ) );

            var creators = TypeCreator.GetCreators( typeof( ExcludedType ) );

            Assert.Empty( creators );
        }

        [Fact]
        public void GetCreators_returns_normal_collection_for_type_derived_from_excluded_type( ) {
            TypeCreator.LimitInstances( typeof( ExcludedType ) );

            var creators = TypeCreator.GetCreators( typeof( DerivedExcludedType ) );

            Assert.NotEmpty( creators );
        }

        [Fact]
        public void GetCreators_returns_empty_collection_for_assembly_excluded_type( ) {
            var creators = TypeCreator.GetCreators( typeof( AssemblyExcludedType ) );

            Assert.Empty( creators );
        }

        [Fact]
        public void GetCreators_returns_expected_collection_for_limited_type( ) {
            TypeCreator.LimitInstances( typeof( LimitedType ), typeof( FactoryLimitedType ) );

            var creators = TypeCreator.GetCreators( typeof( LimitedType ) );

            Assert.Equal( 1, creators.Count );
        }


        [Fact]
        public void GetCreators_returns_expected_collection_for_excluded_assemblies( ) {
            TypeCreator.GetCreators( GetType( ) );

            var creators = TypeCreator.GetCreators( typeof( System.Threading.ReaderWriterLockSlim ) );

            Assert.Equal( 0, creators.Count );
        }


        [Fact]
        public void GetCreators_uses_instance_data_instantiator( ) {
            int current = InstantiateInstanceDataType.InvokeCount;
            var creators = TypeCreator.GetCreators( typeof( SealedStruct ) );

            creators[0].CreateInstance( );
            int actual = InstantiateInstanceDataType.InvokeCount;

            Assert.True( actual > current, typeof( InstantiateInstanceDataType ).Name + " was not used by instance data creator: " + actual );
        }


        public struct SealedStruct { }

        public abstract class AbstractBaseClass { }

        public sealed class DerivedSealedClass : AbstractBaseClass { }

        public sealed class AbstractBaseClassTypeParameter<T> where T : AbstractBaseClass { }


        public class BaseClass { }

        public class DerivedClass : BaseClass { }


        public sealed class ParameterizedConstructorClass {
            public ParameterizedConstructorClass( int parameter ) { }
        }

        public struct ParameterizedConstructorStruct {
            public ParameterizedConstructorStruct( int parameter ) { }
        }


        public sealed class TooManyParameterizedConstructorsClass {
            public TooManyParameterizedConstructorsClass( ) { }
            public TooManyParameterizedConstructorsClass( int a ) { throw new InvalidOperationException( ); }
            public TooManyParameterizedConstructorsClass( int a, int b ) { throw new InvalidOperationException( ); }
            public TooManyParameterizedConstructorsClass( int a, int b, int c ) { throw new InvalidOperationException( ); }
        }

        public struct TooManyParameterizedConstructorsStruct {
            public TooManyParameterizedConstructorsStruct( int a ) { throw new InvalidOperationException( ); }
            public TooManyParameterizedConstructorsStruct( int a, int b ) { throw new InvalidOperationException( ); }
            public TooManyParameterizedConstructorsStruct( int a, int b, int c ) { throw new InvalidOperationException( ); }
        }

        public class RecursiveClass {
            public RecursiveClass( RecursiveClass c ) { }
        }


        public class ClassFromFactory { }

        public static class FactoryOfClassFromFactory {
            public static IEnumerable<Func<ClassFromFactory>> GetInstances( ) { yield return ( ) => new ClassFromFactory( ); }
            public static IEnumerable<Func<ClassFromFactory>> GetInstances( int parameter ) { yield return ( ) => new ClassFromFactory( ); }
        }


        public interface ITargetInterface { }

        public interface IParameterInterface<T> { }

        public struct ParameterInterfaceImpl : IParameterInterface<double> { }

        public class TargetInterfaceImpl<T, D> : ITargetInterface where D : IParameterInterface<T> { }


        public class UnconstrainedGenericType<T> { }

        public static class FactoryOfUnconstrainedGenericType {
            public static IEnumerable<Func<UnconstrainedGenericType<bool>>> GetUnconstrainedInstances( ) { yield return ( ) => new UnconstrainedGenericType<bool>( ); }
        }


        public interface IFactoryInterface<T> { }

        public struct FactoryInterfaceImpl : IFactoryInterface<double> {
            public FactoryInterfaceImpl( FactoryInterfaceImpl parameter ) { }
        }

        public interface IFactoryInterfaceWrapper { }

        public sealed class FactoryInterfaceWrapperImpl : IFactoryInterfaceWrapper {
            internal FactoryInterfaceWrapperImpl( ) { }
        }

        public static class FactoryOfIFactoryInterface {
            public static IEnumerable<Func<IFactoryInterfaceWrapper>> GetInstances<T>( IFactoryInterface<T> instance ) { yield return ( ) => new FactoryInterfaceWrapperImpl( ); }
        }


        public interface IGenericFactoryInterfaceWrapper<T> { }

        public sealed class GenericFactoryInterfaceWrapperImpl<T> : IGenericFactoryInterfaceWrapper<T> {
            internal GenericFactoryInterfaceWrapperImpl( ) { }
        }

        public static class FactoryOfIGenericFactoryInterface {
            public static IEnumerable<Func<IGenericFactoryInterfaceWrapper<T>>> GetInstances<T>( IFactoryInterface<T> instance ) { yield return ( ) => new GenericFactoryInterfaceWrapperImpl<T>( ); }
        }


        public interface ILazyGenericFactoryInterfaceWrapper<T> { }

        public sealed class LazyGenericFactoryInterfaceWrapperImpl<T> : ILazyGenericFactoryInterfaceWrapper<T> {
            internal LazyGenericFactoryInterfaceWrapperImpl( ) { }
        }

        public static class FactoryOfILazyGenericFactoryInterface {
            public static IEnumerable<Func<ILazyGenericFactoryInterfaceWrapper<T>>> GetInstances<T>( IInstanceCreator<IFactoryInterface<T>> instance ) { yield return ( ) => new LazyGenericFactoryInterfaceWrapperImpl<T>( ); }
        }


        public interface IPartialGenericArgument<T, U> { }

        public struct PartialGenericArgumentImpl_String : IPartialGenericArgument<double, string> { }

        public struct PartialGenericArgumentImpl_Int32 : IPartialGenericArgument<double, int>, IPartialGenericArgument<int, string> { }

        public sealed class PartialGenericArgumentWrapperImpl<T> {
            internal PartialGenericArgumentWrapperImpl( ) { }
        }

        public static class FactoryOfPartialGenericArgumentWrapper {
            public static IEnumerable<Func<PartialGenericArgumentWrapperImpl<T>>> GetInstances<T>( IPartialGenericArgument<double, T> converter ) { yield return ( ) => new PartialGenericArgumentWrapperImpl<T>( ); }
        }



        public interface IMultipleResolutionsConstraint<T> { }

        public struct MultipleResolutionsParameterInt32 : IMultipleResolutionsConstraint<int> { }

        public struct MultipleResolutionsParameterDouble : IMultipleResolutionsConstraint<double> { }

        public interface IMultipleResolutions<T> { }

        public struct MultipleResolutions<T, D> : IMultipleResolutions<T> where D : IMultipleResolutionsConstraint<T> { }


        public interface IConstructorParameterResolution<T> { }

        public sealed class ConstructorParameterResolution : IConstructorParameterResolution<bool> {
            public ConstructorParameterResolution( bool parameter ) { }
        }


        public class ExcludedType { }

        public class DerivedExcludedType : ExcludedType { }

        public class AssemblyExcludedType { }

        public class LimitedType { }

        public static class FactoryLimitedType {
            public static IEnumerable<Func<LimitedType>> GetInstances( ) {
                yield return ( ) => new LimitedType( );
            }
        }


        public class InstantiateInstanceDataType : IInstantiateInstanceData {
            private static int invokeCount_;

            public static int InvokeCount { get { return invokeCount_; } }

            public T Invoke<T>( Func<object[], T> instantiate, object[] arguments ) {
                Interlocked.Increment( ref invokeCount_ );
                return instantiate( arguments );
            }
        }


        public static IEnumerable<object[]> GenericTypes {
            get {
                return TestGenericTypeResolver.GenericTypes;
            }
        }

        public static IEnumerable<object[]> MultipleGenericMethodResolutions {
            get {
                return TestGenericTypeResolver.MultipleGenericMethodResolutions;
            }
        }

    }

}
