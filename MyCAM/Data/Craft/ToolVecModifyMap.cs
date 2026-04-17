using System.Collections.Generic;

namespace MyCAM.Data
{
	public class ToolVecModifyMap
	{
		readonly SortedDictionary<int, ToolVecModifyData> m_Map = new SortedDictionary<int, ToolVecModifyData>();

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

		public ICollection<ToolVecModifyData> Values
		{
			get
			{
				return m_Map.Values;
			}
		}

		public ToolVecModifyData this[ int key ]
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

		public bool TryGetValue( int key, out ToolVecModifyData value )
		{
			return m_Map.TryGetValue( key, out value );
		}

		public void Add( int key, ToolVecModifyData value )
		{
			Set( key, value );
		}

		public void Set( int key, ToolVecModifyData value )
		{
			m_Map[ key ] = value;
		}

		
		public void Remove( int removeKey, int nBeOverwriteIdx )
		{
			if( !m_Map.ContainsKey( removeKey ) ) {
				return;
			}
			EToolVecInterpolateType removeType = m_Map[ removeKey ].InterpolateType;
			m_Map.Remove( removeKey );
			if( !m_Map.ContainsKey( nBeOverwriteIdx ) ) {
				return;
			}
			m_Map[ nBeOverwriteIdx ].InterpolateType = removeType;
		}

		public void Remove( int removeKey )
		{
			m_Map.Remove( removeKey );
		}


		public void Clear()
		{
			m_Map.Clear();
		}

		public Dictionary<int, ToolVecModifyData> ToDictionary()
		{
			return new Dictionary<int, ToolVecModifyData>( m_Map );
		}

		public IEnumerator<KeyValuePair<int, ToolVecModifyData>> GetEnumerator()
		{
			return m_Map.GetEnumerator();
		}
	}

	public class ToolVecModifyData
	{
		public double RA_deg
		{
			get; set;
		}

		public double RB_deg
		{
			get; set;
		}

		public double Master_deg
		{
			get; set;
		}

		public double Slave_deg
		{
			get; set;
		}

		public EToolVecInterpolateType InterpolateType
		{
			get; set;
		} = EToolVecInterpolateType.Normal;

		public ToolVecModifyData()
		{
			RA_deg = 0;
			RB_deg = 0;
			Master_deg = 0;
			Slave_deg = 0;
		}

		public ToolVecModifyData( double ra_deg, double rb_deg, double master_deg, double slave_deg, EToolVecInterpolateType interpolateType )
		{
			RA_deg = ra_deg;
			RB_deg = rb_deg;
			Master_deg = master_deg;
			Slave_deg = slave_deg;
			InterpolateType = interpolateType;
		}

		public ToolVecModifyData Clone()
		{
			return new ToolVecModifyData( RA_deg, RB_deg, Master_deg, Slave_deg, InterpolateType );
		}
	}


}
