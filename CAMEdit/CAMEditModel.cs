using DataStructure;
using OCC.TopoDS;
using System.Collections.Generic;

namespace CAMEdit
{

	public class CAMEditModel
	{
		public CAMEditModel( TopoDS_Shape partShape, List<CADData> rawCADDataList )
		{
			if( partShape == null || rawCADDataList == null ) {
				throw new System.ArgumentException( ToString() + "Constructior: Null Ref" );
			}
			if( rawCADDataList.Count == 0 ) {
				throw new System.ArgumentException( ToString() + "Constructior: Empty Collection" );
			}
			m_PartShape = partShape;
			m_RawCADDataList = rawCADDataList;
			BuildCAMDataList();
		}

		public TopoDS_Shape PartShape
		{
			get
			{
				return m_PartShape;
			}
		}

		public List<CAMData> CAMDataList
		{
			get
			{
				return m_CAMDataList;
			}
		}

		// fields
		TopoDS_Shape m_PartShape = null;
		List<CADData> m_RawCADDataList = new List<CADData>();
		List<CAMData> m_CAMDataList = new List<CAMData>();

		void BuildCAMDataList()
		{
			m_CAMDataList.Clear();
			for( int i = 0; i < m_RawCADDataList.Count; i++ ) {
				CADData cadData = m_RawCADDataList[ i ];
				CAMData camData = new CAMData( cadData );
				m_CAMDataList.Add( camData );
			}
		}
	}
}
