using System.Collections.Generic;

namespace MyCAM.Data
{
	public class ToolVecModifyMap
	{
		readonly SortedDictionary<int, ToolVecModifyData2> m_Map = new SortedDictionary<int, ToolVecModifyData2>();

		public int Count
		{
			get
			{
				return m_Map.Count;
			}
		}

		public ICollection<int> Keys
		{
			get
			{
				return m_Map.Keys;
			}
		}

		public ICollection<ToolVecModifyData2> Values
		{
			get
			{
				return m_Map.Values;
			}
		}

		public ToolVecModifyData2 this[ int key ]
		{
			get
			{
				return m_Map[ key ];
			}
			set
			{
				m_Map[ key ] = value;
			}
		}

		public bool ContainsKey( int key )
		{
			return m_Map.ContainsKey( key );
		}

		public bool TryGetValue( int key, out ToolVecModifyData2 value )
		{
			return m_Map.TryGetValue( key, out value );
		}

		public void Add( int key, ToolVecModifyData2 value )
		{
			Set( key, value );
		}

		public void Set( int key, ToolVecModifyData2 value )
		{
			m_Map[ key ] = value;
		}

		/// <summary>
		/// Remove the item at the given key.
		/// Before removal, transfer the removed item's InterpolateType
		/// to the next item whose key is greater than the removed key.
		/// </summary>
		/// 
		public void Remove( int removeKey, int nNextIdxKey )
		{
			if( !m_Map.ContainsKey( removeKey ) ) {
				return;
			}
			EToolVecInterpolateType removeType = m_Map[ removeKey ].InterpolateType;
			m_Map.Remove( removeKey );
			if( !m_Map.ContainsKey( nNextIdxKey ) ) {
				return;
			}
			m_Map[ nNextIdxKey ].InterpolateType = removeType;
		}

		public void Remove( int removeKey )
		{
			m_Map.Remove( removeKey );
		}

		public bool Remove( int key, int StartPntIdx, bool isPathReverse,out EToolVecInterpolateType removedType )
		{
			removedType = EToolVecInterpolateType.Normal;
			if( !m_Map.ContainsKey( key ) ) {
				return false;
			}
			 removedType = m_Map[ key ].InterpolateType;

			// find the smallest key that is greater than the removed key
			int nextKey = -1;
			bool found = false;

			// 瞷indexゑ癬翴
			if( key > StartPntIdx ) {
				// 隔畖タ
				if( isPathReverse == false ) {

					// 眖瞷竚т隔畖Юい程
					foreach( int k in m_Map.Keys ) {
						if( k > key ) {
							nextKey = k;
							found = true;
							break;
						}
					}

					// 眖ヘ玡癬翴程常⊿Τ,т0~癬翴玡程
					if( found == false ) {
						foreach( int k in m_Map.Keys ) {
							if( k > StartPntIdx ) {
								break;
							}
							if( k < key ) {
								nextKey = k;
								found = true;
								break;
							}
						}
					}
				}

				// は
				else {

					// 眖癬翴竚т瞷
					foreach( int k in m_Map.Keys ) {
						if( k > key ) {
							break;
						}
						if( k < key && k> StartPntIdx ) {
							nextKey = k;
							found = true;
						}
					}
				}
			}

			// 瞷竚癬翴ぇ玡
			else {

				// 隔畖タ
				if( isPathReverse == false ) {
					foreach( int k in m_Map.Keys ) {
						if( k > StartPntIdx ) {
							break;
						}
						if( k > key ) {
							nextKey = k;
							found = true;
							break;
						}
					}
				}

				// 隔畖は
				else {

					// 眖瞷竚┕玡т0
					foreach( int k in m_Map.Keys ) {
						if( k > key ) {
							break;
						}
						nextKey = k;
						found = true;
					}

					// ⊿Τт,眖隔畖Ютヘ玡癬翴竚い程
					if( found == false ) {
						
						foreach( int k in m_Map.Keys ) {
							if( k < StartPntIdx ) {
								continue;
							}
							nextKey = k;
							found = true;
						}
					}
				}

			}


			// transfer InterpolateType to the next item
			if( found ) {
				m_Map[ nextKey ].InterpolateType = removedType;
			}
			m_Map.Remove( key );
			return true;
		}

		public void Clear()
		{
			m_Map.Clear();
		}

		public Dictionary<int, ToolVecModifyData2> ToDictionary()
		{
			return new Dictionary<int, ToolVecModifyData2>( m_Map );
		}

		public IEnumerator<KeyValuePair<int, ToolVecModifyData2>> GetEnumerator()
		{
			return m_Map.GetEnumerator();
		}
	}
}
