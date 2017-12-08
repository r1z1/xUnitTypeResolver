
using System;
using Xunit;


namespace TypeResolver {

    public class TestInstanceCreator {

        private static T TestCreator<T>( IInstanceCreator<T> creator ) {
            var type = typeof( T );

            Assert.NotNull( creator );
            Assert.Equal( type, creator.InstanceType );

            var result = (creator as IInstanceCreator).CreateInstance( );
            Assert.NotNull( result );
            Assert.True( type.IsInstanceOfType( result ) );

            return (T)result;
        }


        [Fact]
        public void CreateInstance_succeeds_for_default_struct_constructor( ) {
            var creator = InstanceCreator.ForType<InstantiableStruct>( );

            TestCreator( creator );
        }

        [Fact]
        public void CreateInstance_succeeds_for_parameterized_struct_constructor( ) {
            var type = typeof( InstantiableStruct );
            var constructor = type.GetConstructor( new[] { type } );
            Assert.NotNull( constructor );
            var parameter = InstanceCreator.ForType<InstantiableStruct>( );

            var creator = InstanceCreator.ForMethod<InstantiableStruct>( constructor, parameter );

            TestCreator( creator );
        }


        [Fact]
        public void CreateInstance_succeeds_for_default_class_constructor( ) {
            var creator = InstanceCreator.ForType<InstantiableClass>( );

            TestCreator( creator );
        }

        [Fact]
        public void CreateInstance_succeeds_for_delegate( ) {
            var creator = InstanceCreator.ForDelegate( ( ) => new InstantiableStruct( ) );

            TestCreator( creator );
        }

        [Fact]
        public void CreateInstance_succeeds_for_parameterized_class_constructor( ) {
            var type = typeof( InstantiableClass );
            var constructor = type.GetConstructor( new[] { type } );
            Assert.NotNull( constructor );
            var parameter = InstanceCreator.ForType<InstantiableClass>( );

            var creator = InstanceCreator.ForMethod<InstantiableClass>( constructor, parameter );

            TestCreator( creator );
        }

        [Fact]
        public void CreateInstance_succeeds_for_derived_class_constructor( ) {
            var type = typeof( DerivedInstantiableClass );
            var constructor = type.GetConstructor( new[] { type } );
            Assert.NotNull( constructor );
            var parameter = InstanceCreator.ForType<InstantiableClass>( );

            var creator = InstanceCreator.ForMethod<InstantiableClass>( constructor, parameter );

            var result = TestCreator( creator );

            Assert.True( type.IsInstanceOfType( result ) );
        }


        [Fact]
        public void CreateInstance_succeeds_for_parameterless_method( ) {
            var type = typeof( InstantiableClass );
            var method = type.GetMethod( "Method" );
            Assert.NotNull( method );

            var creator = InstanceCreator.ForMethod<InstantiableClass>( method );

            TestCreator( creator );
        }

        [Fact]
        public void CreateInstance_succeeds_for_parameterized_method( ) {
            var type = typeof( InstantiableClass );
            var method = type.GetMethod( "ParameterizedMethod" );
            Assert.NotNull( method );
            var parameter = InstanceCreator.ForType<InstantiableClass>( );

            var creator = InstanceCreator.ForMethod<InstantiableClass>( method, parameter );

            TestCreator( creator );
        }


        [Fact]
        public void CreateInstance_returns_exception_for_failing_method( ) {
            IInstanceCreator creator = InstanceCreator.ForType<FailingClass>( );

            object result = creator.CreateInstance( );

            var actual = Assert.IsType<Exception>( result );
            Assert.Equal( typeof( FailingClass ).FullName, actual.Message );
        }



        public struct InstantiableStruct {

            public InstantiableStruct( InstantiableStruct s ) { }

        }

        public class InstantiableClass {

            public InstantiableClass( ) { }

            public InstantiableClass( InstantiableClass c ) { }

            public static InstantiableClass Method( ) { return new InstantiableClass( ); }

            public static InstantiableClass ParameterizedMethod( InstantiableClass c ) { return c; }

        }

        public class DerivedInstantiableClass : InstantiableClass {

            public DerivedInstantiableClass( InstantiableClass c ) { }

        }

        public class FailingClass {

            public FailingClass( ) {
                throw new Exception( typeof( FailingClass ).FullName );
            }

        }

    }

}
