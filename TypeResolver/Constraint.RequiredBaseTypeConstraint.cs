
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TypeResolver.Extensions;


namespace TypeResolver {

    public abstract partial class Constraint {

        private sealed class RequiredBaseTypeConstraint : Constraint {
            private readonly Type requiredBaseType_;

            public RequiredBaseTypeConstraint( Type requiredBaseType, bool isUsageConstraint )
                : base( isUsageConstraint ) {
                Debug.Assert( requiredBaseType != null );

                this.requiredBaseType_ = requiredBaseType;
            }

            protected override IEnumerable<LinkList<Binding>> SatisfyCore( Type availableType, LinkList<Binding> bindings ) {
                var newBindings = new List<LinkList<Binding>>( );
                var currentBindngs = new BindingCollection( bindings );

                // Check if the available type derives from the base type.
                if( availableType.Is( this.requiredBaseType_ ) ) {
                    // Add any generic arguments assigned by the base type.
                    if( this.requiredBaseType_.IsGenericType && !this.requiredBaseType_.ContainsGenericParameters ) {
                        IEnumerable<Type> correspondingTypes = GenericTypeResolver.GetCorrespondingBaseTypes( availableType, this.requiredBaseType_ );
                        foreach( Type correspondingType in correspondingTypes ) {
                            if( correspondingType.ContainsGenericParameters )
                                GenericTypeResolver.GetAssignedGenericArguments( currentBindngs, this.requiredBaseType_, correspondingType );
                        }
                    }

                    // Check all concrete version of the available type.
                    foreach( LinkList<Binding> b in currentBindngs ) {
                        var concreteTypes = GenericTypeResolver.GetConcreteTypes( availableType, b );
                        foreach( Type concreteAvailableType in concreteTypes ) {
                            // Add any newly assigned bindings from concrete and base types.
                            var assignedBindings = new BindingCollection( b );
                            GenericTypeResolver.GetAssignedGenericArguments( assignedBindings, concreteAvailableType, availableType );
                            if( this.requiredBaseType_.ContainsGenericParameters )
                                GenericTypeResolver.GetAssignedGenericArguments( assignedBindings, concreteAvailableType, this.requiredBaseType_ );

                            newBindings.AddRange( assignedBindings );
                        }
                    }
                }

                return newBindings;
            }

            protected override string ToStringCore( ) {
                string prefix = (this.IsUsageConstraint
                    ? "Used in "
                    : "Derives from ");
                return prefix + this.requiredBaseType_.GetDescriptiveName( );
            }
        }

    }

}
