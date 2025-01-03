using OCC.gp;

namespace DataStructure
{
	public enum EProcessType
	{
		ProcessType_None,
		ProcessType_Cutting,
		ProcessType_Traverse,
	}

	public interface IProcessData
	{
		EProcessType ProcessType
		{
			get;
		}
	}

	public class CuttingProcessData : IProcessData
	{
		public CuttingProcessData( CAMData camData )
		{
			m_CamData = camData;
		}

		public EProcessType ProcessType
		{
			get
			{
				return EProcessType.ProcessType_Cutting;
			}
		}

		public CAMData CAMData
		{
			get
			{
				return m_CamData;
			}
		}

		CAMData m_CamData;
	}

	public class TraverseProcessData : IProcessData
	{
		public TraverseProcessData( gp_Pnt Point_MCS )
		{
			m_Point_MCS = Point_MCS;
		}

		public EProcessType ProcessType
		{
			get
			{
				return EProcessType.ProcessType_Traverse;
			}
		}

		public gp_Pnt Point_MCS
		{
			get
			{
				return m_Point_MCS;
			}
		}

		gp_Pnt m_Point_MCS;
	}
}
