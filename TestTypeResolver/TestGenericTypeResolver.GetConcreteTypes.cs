
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;
using Xunit.Extensions;


namespace TypeResolver {

    public partial class TestGenericTypeResolver {

        private static void TestConcreteTypes( Type genericType, Type[] expectedConcreteTypes, ReadOnlyCollection<Type> concreteTypes ) {
            Assert.NotNull( concreteTypes );

            Assert.Equal( expectedConcreteTypes.Length, concreteTypes.Count );

            int maxArgumentLength = genericType.GetGenericArguments( ).Length;
            foreach( var concreteType in concreteTypes ) {
                Assert.NotNull( concreteType );

                var genericArguments = concreteType.GetGenericArguments( );
                Assert.InRange( genericArguments.Length, 1, maxArgumentLength );
                if( maxArgumentLength <= 2 )
                    Assert.Contains( genericArguments[genericArguments.Length - 1], expectedConcreteTypes );

                Type type;
                for( type = concreteType; type != null; type = type.BaseType )
                    if( type.GetGenericTypeDefinition( ).Equals( genericType ) )
                        break;
                Assert.NotNull( type );
            }
        }

        [Fact]
        public void GetConcreteTypes_returns_argument_for_concrete_type( ) {
            Type concreteType = typeof( int );

            var types = GenericTypeResolver.GetConcreteTypes( concreteType );

            Assert.Equal( concreteType, Assert.Single( types ) );
        }

        [Fact]
        public void GetConcreteTypes_returns_no_types_for_unconstrained_generic_type( ) {
            Type genercType = typeof( UnconstrainedGenericType<> );

            var types = GenericTypeResolver.GetConcreteTypes( genercType );

            Assert.Empty( types );
        }

        [Fact]
        public void GetConcreteTypes_returns_one_type_for_bound_generic_type( ) {
            Type genercType = typeof( UnconstrainedGenericType<> );
            Type genercArgument = genercType.GetGenericArguments( )[0];
            var genercArgumentBinding = new Binding( genercArgument, typeof( int ) );
            var bindings = Binding.EmptyBindings.Add( genercArgumentBinding );
            var concreteType = genercType.MakeGenericType( genercArgumentBinding.Type );

            var types = GenericTypeResolver.GetConcreteTypes( genercType, bindings );

            Assert.Equal( concreteType, Assert.Single( types ) );
        }

        [Theory]
        [MemberData( "GenericTypes" )]
        public void GetConcreteTypes_returns_expected_types_for_generic_type( Type genericType, Type[] expectedConcreteTypes ) {
            var concreteTypes = GenericTypeResolver.GetConcreteTypes( genericType );

            TestConcreteTypes( genericType, expectedConcreteTypes, concreteTypes );
        }



        public sealed class UnconstrainedGenericType<T> { }


        public interface IGenericTypeConstraint { }

        public struct GenericTypeConstraint_StructImpl : IGenericTypeConstraint { }
        public class GenericTypeConstraint_ClassImpl : IGenericTypeConstraint {
            public GenericTypeConstraint_ClassImpl( int parameter ) { }
        }

        public sealed class GenericClassOfClass<T> where T : class, IGenericTypeConstraint { }
        public sealed class GenericClassOfStruct<T> where T : struct, IGenericTypeConstraint { }
        public sealed class GenericClassOfConstructor<T> where T : IGenericTypeConstraint, new( ) { }
        public sealed class GenericClassOfInterface<T> where T : IGenericTypeConstraint { }
        public sealed class GenericClassOfBaseClass<T> where T : GenericTypeConstraint_ClassImpl { }

        public struct GenericStructOfClass<T> where T : class, IGenericTypeConstraint { }
        public struct GenericStructOfStruct<T> where T : struct, IGenericTypeConstraint { }
        public struct GenericStructOfConstructor<T> where T : IGenericTypeConstraint, new( ) { }
        public struct GenericStructOfInterface<T> where T : IGenericTypeConstraint { }
        public struct GenericStructOfBaseClass<T> where T : GenericTypeConstraint_ClassImpl { }

        public interface GenericInterfaceOfClass<T> where T : class, IGenericTypeConstraint { }
        public interface GenericInterfaceOfStruct<T> where T : struct, IGenericTypeConstraint { }
        public interface GenericInterfaceOfConstructor<T> where T : IGenericTypeConstraint, new( ) { }
        public interface GenericInterfaceOfInterface<T> where T : IGenericTypeConstraint { }
        public interface GenericInterfaceOfBaseClass<T> where T : GenericTypeConstraint_ClassImpl { }
        public class GenericInterfaceOfClassImpl<T> : GenericInterfaceOfClass<T> where T : class, IGenericTypeConstraint { }
        public class GenericInterfaceOfStructImpl<T> : GenericInterfaceOfStruct<T> where T : struct, IGenericTypeConstraint { }
        public class GenericInterfaceOfConstructorImpl<T> : GenericInterfaceOfConstructor<T> where T : IGenericTypeConstraint, new( ) { }
        public class GenericInterfaceOfInterfaceImpl<T> : GenericInterfaceOfInterface<T> where T : IGenericTypeConstraint { }
        public class GenericInterfaceOfBaseClassImpl<T> : GenericInterfaceOfBaseClass<T> where T : GenericTypeConstraint_ClassImpl { }


        public abstract class GenericClass<T> { }

        public class GenericClass_ClassImpl : GenericClass<GenericClass_ClassImpl> { }

        public sealed class GenericClass_ClassUser<T> where T : GenericClass<T> { }

        public struct GenericClass_StructUser<T> where T : GenericClass<T> { }

        public interface GenericClass_InterfaceUser<T> where T : GenericClass<T> { }
        public class GenericClass_InterfaceUserImpl<T> : GenericClass_InterfaceUser<T> where T : GenericClass<T> { }


        public interface IGenericInterface<T> { }

        public struct GenericInterface_StructImpl : IGenericInterface<GenericInterface_StructImpl> { }
        public class GenericInterface_ClassImpl : IGenericInterface<GenericInterface_ClassImpl> { }

        public sealed class GenericInterface_ClassUser<T> where T : IGenericInterface<T> { }

        public struct GenericInterface_StructUser<T> where T : IGenericInterface<T> { }

        public interface GenericInterface_InterfaceUser<T> where T : IGenericInterface<T> { }
        public class GenericInterface_InterfaceUserImpl<T> : GenericInterface_InterfaceUser<T> where T : IGenericInterface<T> { }


        public interface IComplexGenericInterface<T> { }

        public struct ComplexGenericInterface_StructImpl : IComplexGenericInterface<double> { }
        public sealed class ComplexGenericInterface_ClassImpl : IComplexGenericInterface<double> { }

        public sealed class ComplexGenericInterface_ClassUser<T, D> where D : IComplexGenericInterface<T> { }

        public struct ComplexGenericInterface_StructUser<T, D> where D : IComplexGenericInterface<T> { }

        public interface ComplexGenericInterface_InterfaceUser<T, D> where D : IComplexGenericInterface<T> { }
        public class ComplexGenericInterface_InterfaceUserImpl<T, D> : ComplexGenericInterface_InterfaceUser<T, D> where D : IComplexGenericInterface<T> { }


        public interface IDerivedGenericInterface<T> { }
        public interface IDerivedGenericInterface<T, H> { }

        public struct DerivedGenericInterface_StructImpl : IDerivedGenericInterface<double>, IDerivedGenericInterface<double, double> { }

        public abstract class DerivedGenericInterface_ClassUserBase<T, D> where D : IDerivedGenericInterface<T> { }
        public class DerivedGenericInterface_ClassUserDerived<T, H, D> : DerivedGenericInterface_ClassUserBase<T, D> where D : IDerivedGenericInterface<T>, IDerivedGenericInterface<T, H> { }


        public interface IMultipleImplementationsInterface<T, H> { }

        public struct MultipleImplementationsInterface_StructImpl : IMultipleImplementationsInterface<int, string>, IMultipleImplementationsInterface<double, string>, IMultipleImplementationsInterface<string, double> { }

        public class MultipleImplementationsInterface_ClassUser<T, H, D> where D : IMultipleImplementationsInterface<T, H>, IMultipleImplementationsInterface<H, T> { }


        public static IEnumerable<object[]> GenericTypes {
            get {
                yield return new object[] { typeof( GenericClassOfClass<> ), new[] { typeof( GenericTypeConstraint_ClassImpl ) } };
                yield return new object[] { typeof( GenericClassOfStruct<> ), new[] { typeof( GenericTypeConstraint_StructImpl ) } };
                yield return new object[] { typeof( GenericClassOfConstructor<> ), new[] { typeof( GenericTypeConstraint_StructImpl ) } };
                yield return new object[] { typeof( GenericClassOfInterface<> ), new[] { typeof( GenericTypeConstraint_ClassImpl ), typeof( GenericTypeConstraint_StructImpl ) } };
                yield return new object[] { typeof( GenericClassOfBaseClass<> ), new[] { typeof( GenericTypeConstraint_ClassImpl ) } };

                yield return new object[] { typeof( GenericStructOfClass<> ), new[] { typeof( GenericTypeConstraint_ClassImpl ) } };
                yield return new object[] { typeof( GenericStructOfStruct<> ), new[] { typeof( GenericTypeConstraint_StructImpl ) } };
                yield return new object[] { typeof( GenericStructOfConstructor<> ), new[] { typeof( GenericTypeConstraint_StructImpl ) } };
                yield return new object[] { typeof( GenericStructOfInterface<> ), new[] { typeof( GenericTypeConstraint_ClassImpl ), typeof( GenericTypeConstraint_StructImpl ) } };
                yield return new object[] { typeof( GenericStructOfBaseClass<> ), new[] { typeof( GenericTypeConstraint_ClassImpl ) } };

                yield return new object[] { typeof( GenericInterfaceOfClass<> ), new[] { typeof( GenericTypeConstraint_ClassImpl ) } };
                yield return new object[] { typeof( GenericInterfaceOfStruct<> ), new[] { typeof( GenericTypeConstraint_StructImpl ) } };
                yield return new object[] { typeof( GenericInterfaceOfConstructor<> ), new[] { typeof( GenericTypeConstraint_StructImpl ) } };
                yield return new object[] { typeof( GenericInterfaceOfInterface<> ), new[] { typeof( GenericTypeConstraint_ClassImpl ), typeof( GenericTypeConstraint_StructImpl ) } };
                yield return new object[] { typeof( GenericInterfaceOfBaseClass<> ), new[] { typeof( GenericTypeConstraint_ClassImpl ) } };

                yield return new object[] { typeof( GenericClass_ClassUser<> ), new[] { typeof( GenericClass_ClassImpl ) } };
                yield return new object[] { typeof( GenericClass_StructUser<> ), new[] { typeof( GenericClass_ClassImpl ) } };
                yield return new object[] { typeof( GenericClass_InterfaceUser<> ), new[] { typeof( GenericClass_ClassImpl ) } };

                yield return new object[] { typeof( GenericInterface_ClassUser<> ), new[] { typeof( GenericInterface_ClassImpl ), typeof( GenericInterface_StructImpl ) } };
                yield return new object[] { typeof( GenericInterface_StructUser<> ), new[] { typeof( GenericInterface_ClassImpl ), typeof( GenericInterface_StructImpl ) } };
                yield return new object[] { typeof( GenericInterface_InterfaceUser<> ), new[] { typeof( GenericInterface_ClassImpl ), typeof( GenericInterface_StructImpl ) } };

                yield return new object[] { typeof( ComplexGenericInterface_ClassUser<,> ), new[] { typeof( ComplexGenericInterface_ClassImpl ), typeof( ComplexGenericInterface_StructImpl ) } };
                yield return new object[] { typeof( ComplexGenericInterface_StructUser<,> ), new[] { typeof( ComplexGenericInterface_ClassImpl ), typeof( ComplexGenericInterface_StructImpl ) } };
                yield return new object[] { typeof( ComplexGenericInterface_InterfaceUser<,> ), new[] { typeof( ComplexGenericInterface_ClassImpl ), typeof( ComplexGenericInterface_StructImpl ) } };

                yield return new object[] { typeof( DerivedGenericInterface_ClassUserBase<,> ), new[] { typeof( DerivedGenericInterface_StructImpl ) } };

                yield return new object[] { typeof( MultipleImplementationsInterface_ClassUser<,,> ), new[] { typeof( MultipleImplementationsInterface_ClassUser<double, string, MultipleImplementationsInterface_StructImpl> ), typeof( MultipleImplementationsInterface_ClassUser<string, double, MultipleImplementationsInterface_StructImpl> ) } };
            }
        }

    }

}
