using System;
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
				if( !m_Map.ContainsKey( key ) ) {
					return new ToolVecModifyData();
				}
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
			if( value == null ) {
				throw new ArgumentNullException( nameof( value ), "ToolVecModifyData cannot be null" );
			}
			if( m_Map.ContainsKey( key ) ) {
				throw new ArgumentException( $"Key {key} already exists in ToolVecModifyMap" );
			}
			m_Map[ key ] = value;
		}

		public void Set( int key, ToolVecModifyData value )
		{
			if( value == null ) {
				throw new ArgumentNullException( nameof( value ), "ToolVecModifyData cannot be null" );
			}
			m_Map[ key ] = value;
		}

		// remove the key and set the interpolate type to the be overwrite key
		public void Remove( int removeKey, int beOverwriteIdx )
		{
			if( !m_Map.ContainsKey( removeKey ) ) {
				return;
			}
			EToolVecInterpolateType removeType = m_Map[ removeKey ].InterpolateType;
			m_Map.Remove( removeKey );
			if( !m_Map.ContainsKey( beOverwriteIdx ) ) {
				return;
			}
			m_Map[ beOverwriteIdx ].InterpolateType = removeType;
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
		public ToolVecAngleData AngleData
		{
			get
			{
				return m_AngleData;
			}
			set
			{
				m_AngleData = value;
			}
		}

		public EToolVecInterpolateType InterpolateType
		{
			get; set;
		} = EToolVecInterpolateType.Normal;

		public ToolVecModifyData()
		{
			m_AngleData = null;
			InterpolateType = EToolVecInterpolateType.Normal;
		}

		// allow angle data can be null ( it can just set interpolate type)
		public ToolVecModifyData( EToolVecInterpolateType interpolateType )
		{
			m_AngleData = null;
			InterpolateType = interpolateType;
		}

		public ToolVecModifyData( ToolVecAngleData toolVecAngleData, EToolVecInterpolateType interpolateType )
		{
			m_AngleData = toolVecAngleData;
			InterpolateType = interpolateType;
		}

		public ToolVecModifyData Clone()
		{
			if( m_AngleData == null ) {
				return new ToolVecModifyData( InterpolateType );
			}
			return new ToolVecModifyData( m_AngleData.Clone(), InterpolateType );
		}

		ToolVecAngleData m_AngleData;
	}

	public class ToolVecAngleData
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

		public ToolVecAngleData()
		{
		}

		public ToolVecAngleData( double ra_deg, double rb_deg, double master_deg, double slave_deg )
		{
			RA_deg = ra_deg;
			RB_deg = rb_deg;
			Master_deg = master_deg;
			Slave_deg = slave_deg;
		}

		public ToolVecAngleData Clone()
		{
			return new ToolVecAngleData( RA_deg, RB_deg, Master_deg, Slave_deg );
		}
	}
}
