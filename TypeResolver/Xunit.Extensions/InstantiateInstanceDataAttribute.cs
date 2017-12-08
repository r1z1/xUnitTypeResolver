
using System;
using System.Diagnostics;
using TypeResolver.Extensions;


namespace Xunit.Extensions {

    /// <summary>
    /// Indicates a <see cref="Type"/> that should be used to assist in instantiating instance data.
    /// </summary>
    /// <seealso cref="IInstantiateInstanceData"/>
    [AttributeUsage( AttributeTargets.Assembly, AllowMultiple = false )]
    public class InstantiateInstanceDataAttribute : Attribute {

        /// <summary>
        /// Initializes a new instance of the <see cref="InstantiateInstanceDataAttribute"/> class with the specified type.
        /// </summary>
        /// <param name="instantiatorType">The <see cref="Type"/> to use when instantiating instance data.</param>
        public InstantiateInstanceDataAttribute( Type instantiatorType ) {
            Debug.Assert( instantiatorType != null && instantiatorType.Is<IInstantiateInstanceData>( ) );

            this.InstantiatorType = instantiatorType;
        }

        /// <summary>
        /// Gets the <see cref="Type"/> to use when instantiating instance data.
        /// </summary>
        public Type InstantiatorType { get; private set; }

    }

}
