using System.Collections.Generic;

namespace Psychoflow.Util {
    public abstract class SharedResources<TKey, TValue> {
		protected Dictionary<TKey, TValue> m_Dictionary;

		public SharedResources() {
			m_Dictionary = new Dictionary<TKey, TValue>();
		}

		/// <summary>
		/// Initialize the shared resouces with the specific keys.
		/// </summary>
		/// <param name="warmStartingKeys"></param>
		public SharedResources(params TKey[] warmStartingKeys) {
			foreach (var key in warmStartingKeys) {
				Get(key);
			}
		}

		/// <summary>
		/// Get the shared resource by the key.
		/// </summary>
		/// <param name="keyValue"></param>
		/// <returns></returns>
		public virtual TValue Get(TKey keyValue) { 
			if (!m_Dictionary.ContainsKey(keyValue)) {
				m_Dictionary.Add(keyValue, CreateInstance(keyValue));
			}
			if (m_Dictionary[keyValue] == null || m_Dictionary[keyValue].Equals(null)) {
				m_Dictionary[keyValue] = CreateInstance(keyValue);
			}
			return m_Dictionary[keyValue];

		}

		/// <summary>
		/// Create a new instnace as lazying loading.
		/// </summary>
		/// <param name="keyValue"></param>
		/// <returns></returns>
		protected abstract TValue CreateInstance(TKey keyValue);

		/// <summary>
		/// Remove all instances in the resources.
		/// </summary>
		public void ClearAllInstances() {
			m_Dictionary.Clear();
		}
    }

}