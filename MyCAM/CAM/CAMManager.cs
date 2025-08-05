using MyCAM.CAD;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.CAM
{
	internal class CAMManager
	{
		public CAMManager()
		{
			CAMDataContainer = new List<CAMData>();
			CAMDataMap = new Dictionary<string, CAMData>();

			// view manager
			PathNode = new TreeNode( "Path" );
			ViewObjectMap = new Dictionary<string, ViewObject>();
			TreeNodeMap = new Dictionary<string, TreeNode>();
		}

		// TODO: it is not good to giving out the pointer of field
		public List<CAMData> CAMDataContainer
		{
			get; private set;
		}

		public Dictionary<string, CAMData> CAMDataMap
		{
			get; private set;
		}

		public void AddCAMData( List<CAMData> newCAMDataList )
		{
		}

		// view manager
		public Dictionary<string, ViewObject> ViewObjectMap
		{
			get; private set;
		}

		public Dictionary<string, TreeNode> TreeNodeMap
		{
			get; private set;
		}

		public TreeNode PathNode
		{
			get; private set;
		}
	}
}
