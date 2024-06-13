namespace Psychoflow.Util {
	/// <summary>
	/// The interface follows System.HashCode in .Net 5
	/// HashCode.Combine Method: https://docs.microsoft.com/zh-tw/dotnet/api/system.hashcode.combine?view=net-5.0
	/// The implementation is based on the answer of stackoverflow: https://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-overriding-gethashcode/263416#263416
	/// </summary>
	public static class HashCode {
        public static int Combine<T1, T2>(T1 value1, T2 value2) {
			unchecked {
				int hash = 17;
				hash = hash * 23 + value1.GetHashCode();
				hash = hash * 23 + value2.GetHashCode();
				return hash;
			}
		}

		public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3) {
			unchecked {
				int hash = 17;
				hash = hash * 23 + value1.GetHashCode();
				hash = hash * 23 + value2.GetHashCode();
				hash = hash * 23 + value3.GetHashCode();
				return hash;
			}
		}

		public static int Combine<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4) {
			unchecked {
				int hash = 17;
				hash = hash * 23 + value1.GetHashCode();
				hash = hash * 23 + value2.GetHashCode();
				hash = hash * 23 + value3.GetHashCode();
				hash = hash * 23 + value4.GetHashCode();
				return hash;
			}
		}

		public static int Combine<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5) {
			unchecked {
				int hash = 17;
				hash = hash * 23 + value1.GetHashCode();
				hash = hash * 23 + value2.GetHashCode();
				hash = hash * 23 + value3.GetHashCode();
				hash = hash * 23 + value4.GetHashCode();
				hash = hash * 23 + value5.GetHashCode();
				return hash;
			}
		}

		public static int Combine<T1, T2, T3, T4, T5, T6>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6) {
			unchecked { 
				int hash = 17;
				hash = hash * 23 + value1.GetHashCode();
				hash = hash * 23 + value2.GetHashCode();
				hash = hash * 23 + value3.GetHashCode();
				hash = hash * 23 + value4.GetHashCode();
				hash = hash * 23 + value5.GetHashCode();
				hash = hash * 23 + value6.GetHashCode();
				return hash;
			}
		}

		public static int Combine<T1, T2, T3, T4, T5, T6, T7>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7) {
			unchecked {
				int hash = 17;
				hash = hash * 23 + value1.GetHashCode();
				hash = hash * 23 + value2.GetHashCode();
				hash = hash * 23 + value3.GetHashCode();
				hash = hash * 23 + value4.GetHashCode();
				hash = hash * 23 + value5.GetHashCode();
				hash = hash * 23 + value6.GetHashCode();
				hash = hash * 23 + value7.GetHashCode();
				return hash;
			}
		}

		public static int Combine<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8) {
			unchecked {
				int hash = 17;
				hash = hash * 23 + value1.GetHashCode();
				hash = hash * 23 + value2.GetHashCode();
				hash = hash * 23 + value3.GetHashCode();
				hash = hash * 23 + value4.GetHashCode();
				hash = hash * 23 + value5.GetHashCode();
				hash = hash * 23 + value6.GetHashCode();
				hash = hash * 23 + value7.GetHashCode();
				hash = hash * 23 + value8.GetHashCode();
				return hash;
			}
		}
	}
}