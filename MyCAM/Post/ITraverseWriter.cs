using MyCAM.Data;
using System.IO;

namespace MyCAM.Post
{
	internal interface ITraverseWriter
	{
		StreamWriter Writer
		{
			get;
		}

		// traverse writing methods
		void WriteLinearTraverse( PostPoint point, double followSafeDistance );

		void WriteFrogLeap( PostPoint midPoint, PostPoint endPoint, double followSafeDistance );

		// NC command formatting
		string GetRotaryAxisCommand( double master_deg, double slave_deg, string szAxisCommandFix = "" );
	}
}
