// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

namespace PennyLogger.Internals.Estimator.Cuckoo
{
    /// <summary>
    /// An 8-bit bucket in a standard cuckoo filter, consisting of a fingerprint and no counter
    /// </summary>
    public struct CuckooBucket8 : ICuckooBucket
    {
        private byte Value;

        /// <inheritdoc/>
        public ulong Fingerprint
        {
            get => BitTwiddle.GetBits(ref Value, FingerprintShift, FingerprintBitmask);
            set => BitTwiddle.SetBits(ref Value, FingerprintShift, FingerprintBitmask, value);
        }

        /// <inheritdoc/>
        public ulong Count
        {
            get => 1;
            set {}
        }

        /// <inheritdoc/>
        public ulong MaxFingerprint => FingerprintBitmask >> FingerprintShift;

        /// <inheritdoc/>
        public ulong MaxCount => 1;

        /// <inheritdoc/>
        public int SizeOf => sizeof(byte);

        /// <inheritdoc/>
        public bool IsEmpty => Value == 0;

        private const int FingerprintShift = 0;
        private const ulong FingerprintBitmask = 0xffUL;
    }
}
