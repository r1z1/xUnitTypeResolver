
using System;
using System.Diagnostics;


namespace Xunit.Extensions {

    /// <summary>
    /// Indicates a <see cref="Type"/> that should be limited from instance data creation.
    /// </summary>
    /// <seealso cref="TypeResolver.TypeCreator.LimitInstances"/>
    [AttributeUsage( AttributeTargets.Assembly, AllowMultiple = true )]
    public class LimitInstanceDataAttribute : Attribute {

        /// <summary>
        /// Initializes a new instance of the <see cref="LimitInstanceDataAttribute"/> class with the specified type.
        /// </summary>
        /// <param name="targetType">The <see cref="Type"/> to limit.</param>
        /// <param name="availableTypes">The only sources allowed for the target type.</param>
        public LimitInstanceDataAttribute( Type targetType, params Type[] availableTypes ) {
            Debug.Assert( targetType != null );

            this.TargetType = targetType;
            this.AvailableTypes = availableTypes ?? Type.EmptyTypes;
        }

        /// <summary>
        /// Gets the <see cref="Type"/> to limit.
        /// </summary>
        public Type TargetType { get; private set; }

        /// <summary>
        /// Gets the only sources allowed for the target type.
        /// </summary>
        public Type[] AvailableTypes { get; private set; }

    }

}
