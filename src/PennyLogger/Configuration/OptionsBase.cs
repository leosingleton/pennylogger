// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System.Runtime.CompilerServices;

namespace PennyLogger.Configuration
{
    /// <summary>
    /// Base class for all options classes. Implements an efficient value comparison to detect changes to dynamic
    /// configuration.
    /// </summary>
    /// <typeparam name="T">Type of the derived class</typeparam>
    public abstract class OptionsBase<T>
    {
        /// <summary>
        /// Derived classes must implement this method to return all properties as a tuple of value types. This tuple
        /// is used to overload <see cref="Equals(object)"/> and <see cref="GetHashCode"/>.
        /// </summary>
        /// <returns>Tuple containing the value of all properties</returns>
        protected abstract ITuple ToTuple();

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            ReferenceEquals(this, obj) || (obj is OptionsBase<T> options && options.ToTuple().Equals(ToTuple()));

        /// <inheritdoc/>
        public override int GetHashCode() => ToTuple().GetHashCode();

        /// <inheritdoc/>
        public static bool operator ==(OptionsBase<T> options1, OptionsBase<T> options2) =>
            ReferenceEquals(options1, options2) ||
            (options1 is object && options2 is object && options1.ToTuple().Equals(options2.ToTuple()));

        /// <inheritdoc/>
        public static bool operator !=(OptionsBase<T> options1, OptionsBase<T> options2) =>
            !ReferenceEquals(options1, options2) &&
            (options1 is null || options2 is null || !options1.ToTuple().Equals(options2.ToTuple()));
    }
}
