using System.Collections.Generic;

namespace MyCAM.Data
{
	internal class ProcessPathGroup
	{
		readonly List<string> m_PathIDs = new List<string>();
		readonly List<int> m_PathIndices = new List<int>();

		public ProcessPathGroup( int index, ExternalAxisPos externalAxisPos )
		{
			Index = index;
			ExternalAxisPos = externalAxisPos;
		}

		public int Index { get; }
		public ExternalAxisPos ExternalAxisPos { get; }
		public IReadOnlyList<string> PathIDs => m_PathIDs.AsReadOnly();
		public IReadOnlyList<int> PathIndices => m_PathIndices.AsReadOnly();

		internal void AddPath( string pathID, int pathIndex )
		{
			m_PathIDs.Add( pathID );
			m_PathIndices.Add( pathIndex );
		}
	}
}
