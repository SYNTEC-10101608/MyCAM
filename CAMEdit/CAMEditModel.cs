using DataStructure;
using OCC.TopoDS;
using System.Collections.Generic;

namespace CAMEdit
{

	public class CAMEditModel
	{
		public CAMEditModel( TopoDS_Shape model, List<CADData> rawCADDataList )
		{
			if( model == null || rawCADDataList == null ) {
				throw new System.ArgumentException( ToString() + "Constructior: Null Ref" );
			}
			if( rawCADDataList.Count == 0 ) {
				throw new System.ArgumentException( ToString() + "Constructior: Empty Collection" );
			}
			m_ModelShape = model;
			m_RawCADDataList = rawCADDataList;
			BuildCAMData();
		}

		public TopoDS_Shape ModelShape
		{
			get
			{
				return m_ModelShape;
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
		TopoDS_Shape m_ModelShape = null;
		List<CADData> m_RawCADDataList = new List<CADData>();
		List<CAMData> m_CAMDataList = new List<CAMData>();

		void BuildCAMData()
		{
			m_CAMDataList.Clear();
			foreach( CADData cadData in m_RawCADDataList ) {
				CAMData camData = new CAMData( cadData );
				m_CAMDataList.Add( camData );
			}
		}
	}
}
