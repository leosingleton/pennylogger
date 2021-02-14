// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

namespace PennyLogger.Internals.Estimator.Cuckoo
{
    /// <summary>
    /// A 64-bit bucket in a counting cuckoo filter, consisting of a 48-bit fingerprint and 16-bit counter
    /// </summary>
    public struct CuckooBucket64Counting : ICuckooBucket
    {
        private ulong Value;

        /// <inheritdoc/>
        public ulong Fingerprint
        {
            get => BitTwiddle.GetBits(ref Value, FingerprintShift, FingerprintBitmask);
            set => BitTwiddle.SetBits(ref Value, FingerprintShift, FingerprintBitmask, value);
        }

        /// <inheritdoc/>
        public ulong Count
        {
            get => BitTwiddle.GetBits(ref Value, CountShift, CountBitmask);
            set => BitTwiddle.SetBits(ref Value, CountShift, CountBitmask, value);
        }

        /// <inheritdoc/>
        public ulong MaxFingerprint => FingerprintBitmask >> FingerprintShift;

        /// <inheritdoc/>
        public ulong MaxCount => CountBitmask >> CountShift;

        /// <inheritdoc/>
        public int SizeOf => sizeof(ulong);

        /// <inheritdoc/>
        public bool IsEmpty => Value == 0;

        private const int FingerprintShift = 0;
        private const ulong FingerprintBitmask = 0xffff_ffff_ffffUL;

        private const int CountShift = 48;
        private const ulong CountBitmask = 0xffff_0000_0000_0000UL;
    }
}
