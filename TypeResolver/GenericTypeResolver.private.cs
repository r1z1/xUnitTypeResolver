
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TypeResolver.Internal;


namespace TypeResolver {
    using TypeCache = System.Collections.Concurrent.ConcurrentDictionary<Type, ReadOnlyCollection<Type>>;

    public static partial class GenericTypeResolver {

        private static readonly TypeCache concreteTypeCache_ = new TypeCache( );


        private static ReadOnlyCollection<Type> GetConcreteTypesCore( Type genericType, LinkList<Binding> bindings ) {
            Debug.Assert( genericType != null );
            Debug.Assert( bindings != null );

            bool cached = concreteTypeCache_.ContainsKey( genericType );
            bool unbound = genericType.GetGenericArguments( )
                .All( ( a ) => !Binding.ContainsArgument( bindings, a ) );

            // If the type has already been evaluated and there are no interfering bindings, return cached value.
            if( cached && unbound )
                return concreteTypeCache_[genericType];


            // Otherwise, get a concrete type for each set of bindings.
            LinkList<Type> openArguments;
            BindingCollection concreteTypeBindngs = GenericTypeResolver.GetConcreteTypeBindings( genericType, bindings, out openArguments );
            var concreteTypes = concreteTypeBindngs
                .Transform( ( argumentBindings ) => MakeConcreteType( genericType, openArguments, argumentBindings ) )
                .ToReadOnlyCollection( );

            // If there are no interfering bindings, cache the concrete types.
            if( !cached && unbound ) {
                concreteTypeCache_.TryAdd( genericType, concreteTypes );
                concreteTypes = concreteTypeCache_[genericType];
            }

            return concreteTypes;
        }

        private static ICollection<MethodInfo> GetConcreteMethods( MethodInfo method, LinkList<Binding> bindings ) {
            Debug.Assert( method != null );
            Debug.Assert( bindings != null );


            // Try to resolve generic arguments on method.
            var openArguments = GenericTypeResolver.GetOpenGenericArguments( method.GetGenericArguments( ) );
            var concreteBindings = new BindingCollection( bindings );
            GenericTypeResolver.BindGenericArguments( concreteBindings, openArguments );

            var concreteMethods = concreteBindings.Transform( ( b ) => MakeConcreteMethod( method, openArguments, b ) );


            // If generic arguments cannot be resolved statically, try to bind method arguments by searching for creatable parameter types.
            if( concreteMethods.Count == 0 ) {
                var genericParameterTypes = method.GetParameters( )
                    .Select( p => p.ParameterType )
                    .Where( t => t.IsGenericType )
                    .ToArray( );
                Debug.Assert( genericParameterTypes.All( t => !t.IsGenericParameter ) );

                var creatableParameterTypes = genericParameterTypes
                    .Select( t => TypeCreator.GetCreators( t )
                        .Select( c => c.InstanceType )
                        .Distinct( )
                    );

                var parameterBindings = new BindingCollection( bindings );
                parameterBindings.Expand( Permuter.Permute( creatableParameterTypes ), ( b, permutation ) => {
                    for( int i = 0; i < permutation.Length; ++i ) {
                        Type permutationType = permutation[i];
                        Type genericParameterType = genericParameterTypes[i];
                        GenericTypeResolver.GetAssignedGenericArguments( b, permutationType, genericParameterType );
                    }
                } );

                concreteMethods = parameterBindings.Transform( ( b ) => MakeConcreteMethod( method, openArguments, b ) );
            }


            return concreteMethods;
        }


        private static LinkList<Type> GetOpenGenericArguments( Type[] genericArguments ) {
            Debug.Assert( genericArguments != null );

            return genericArguments
                .Where( ( a ) => a.ContainsGenericParameters )
                .ToLinkList( );
        }

        private static Type[] BindConcreteArguments( LinkList<Type> openArguments, LinkList<Binding> bindings ) {
            Debug.Assert( openArguments != null );
            Debug.Assert( bindings != null );

            var boundArguments =
                from argument in openArguments
                join binding in bindings.Distinct( ) on argument equals binding.Argument
                select binding.Type;

            return boundArguments.ToArray( );
        }

        private static Type MakeConcreteType( Type genericType, LinkList<Type> openArguments, LinkList<Binding> bindings ) {
            Debug.Assert( genericType != null );
            Debug.Assert( openArguments != null );
            Debug.Assert( bindings != null );

            // If the type is concrete, return it immediately.
            if( !genericType.ContainsGenericParameters ) {
                Debug.Assert( openArguments.IsEmpty );
                return genericType;
            }

            // Otherwise, retrieve bindings for open arguments and construct the type.
            var boundArguments = GenericTypeResolver.BindConcreteArguments( openArguments, bindings );

            Debug.Assert( boundArguments.Length <= openArguments.Count );
            var concreteType = (boundArguments.Length == openArguments.Count
                ? genericType.MakeGenericType( boundArguments )
                : null);

            return concreteType;
        }

        private static MethodInfo MakeConcreteMethod( MethodInfo method, LinkList<Type> openArguments, LinkList<Binding> bindings ) {
            Debug.Assert( method != null );
            Debug.Assert( openArguments != null );
            Debug.Assert( bindings != null );

            // If the method is not generic, return it immediately.
            if( !method.IsGenericMethodDefinition ) {
                Debug.Assert( openArguments.IsEmpty );
                return method;
            }

            // Otherwise, retrieve bindings for open arguments and construct the method.
            var boundArguments = GenericTypeResolver.BindConcreteArguments( openArguments, bindings );

            var concreteMethod = (boundArguments.Length == openArguments.Count
                ? method.MakeGenericMethod( boundArguments )
                : null);

            return concreteMethod;
        }

        private static BindingCollection GetConcreteTypeBindings( Type type, LinkList<Binding> bindings, out LinkList<Type> openArguments ) {
            Debug.Assert( type != null );
            Debug.Assert( bindings != null );


            // Avoid recursive searches for generic type.
            if( type.ContainsGenericParameters ) {
                if( Binding.ContainsType( bindings, type ) ) {
                    openArguments = LinkList<Type>.Empty;
                    return new BindingCollection( );
                }
                bindings = bindings.Add( new Binding( null, type ) );
            }

            // Bind all unassigned generic argument on the generic type.
            openArguments = GenericTypeResolver.GetOpenGenericArguments( type );
            var collection = new BindingCollection( bindings );
            GenericTypeResolver.BindGenericArguments( collection, openArguments );
            return collection;
        }

        private static void BindGenericArguments( BindingCollection bindings, LinkList<Type> openArguments ) {
            Debug.Assert( bindings != null );
            Debug.Assert( openArguments != null );

            foreach( Type argument in openArguments ) {
                // Get all types satisfying the current constraint.
                Assembly[] referenceAssemblies;
                var constraints = Constraint.GetConstraints( argument, out referenceAssemblies );

                // If the argument has no limiting constraints, skip it.
                if( constraints.IsEmpty || referenceAssemblies.Length == 0 )
                    continue;


                // Add each type satisfying the current generic argument to the list of bindings.
                bool onlyUsageConstraints = constraints.All( ( c ) => c.IsUsageConstraint );
                bindings.Expand( TypeLoader.GetUsableTypes( referenceAssemblies ), ( constraintBindings, type ) => {
                    LinkList<Type> openTypeArguments = GetOpenGenericArguments( type );
                    Constraint.SatisfyConstraints( constraintBindings, type, constraints );
                    constraintBindings.Reduce( constraintBinding => {
                        Type concreteType = MakeConcreteType( type, openTypeArguments, constraintBinding );
                        if( concreteType == null )
                            return null;
                        else if( onlyUsageConstraints || Binding.ContainsArgument( constraintBinding, argument ) )
                            return constraintBinding;
                        else
                            return constraintBinding.Add( new Binding( argument, concreteType ) );
                    } );
                } );

                if( bindings.Count == 0 )
                    break;
            }
        }

    }

}
