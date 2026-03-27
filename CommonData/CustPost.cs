using System.Collections.Generic;

namespace CommonData
{
	public class CustPost
	{
		public string Header
		{
			get;
		} = string.Empty;

		public string Tail
		{
			get;
		} = string.Empty;

		public string PathHeader
		{
			get;
		} = string.Empty;

		public string PathTail
		{
			get;
		} = string.Empty;

		public string StdPathHeader
		{
			get;
		} = string.Empty;

		public string StdPathTail
		{
			get;
		} = string.Empty;

		public string TraverseHeader
		{
			get;
		} = string.Empty;

		public string TraverseTail
		{
			get;
		} = string.Empty;

		// constructor with default content
		public CustPost()
		{
			Header = DEFAULT_Header;
			Tail = DEFAULT_Tail;
			PathHeader = DEFAULT_PathHeader;
			PathTail = DEFAULT_PathTail;
			StdPathHeader = DEFAULT_StdPathHeader;
			StdPathTail = DEFAULT_StdPathTail;
			TraverseHeader = DEFAULT_TraverseHeader;
			TraverseTail = DEFAULT_TraverseTail;
		}

		public CustPost( Dictionary<string, string> custPostMap)
		{
			if ( custPostMap.TryGetValue( EPostSection.Header.ToString(), out var headerContent ) ) {
				Header = headerContent;
			}
			if ( custPostMap.TryGetValue( EPostSection.Tail.ToString(), out var tailContent ) ) {
				Tail = tailContent;
			}
			if ( custPostMap.TryGetValue( EPostSection.PathHeader.ToString(), out var pathHeaderContent ) ) {
				PathHeader = pathHeaderContent;
			}
			if ( custPostMap.TryGetValue( EPostSection.PathTail.ToString(), out var pathTailContent ) ) {
				PathTail = pathTailContent;
			}
			if ( custPostMap.TryGetValue( EPostSection.StdPathHeader.ToString(), out var stdPathHeaderContent ) ) {
				StdPathHeader = stdPathHeaderContent;
			}
			if ( custPostMap.TryGetValue( EPostSection.StdPathTail.ToString(), out var stdPathTailContent ) ) {
				StdPathTail = stdPathTailContent;
			}
			if ( custPostMap.TryGetValue( EPostSection.TraverseHeader.ToString(), out var traverseHeaderContent ) ) {
				TraverseHeader = traverseHeaderContent;
			}
			if ( custPostMap.TryGetValue( EPostSection.TraverseTail.ToString(), out var traverseTailContent ) ) {
				TraverseTail = traverseTailContent;
			}
		}

		public const string DEFAULT_Header = "G43.4 P1;\nG65 P\"FileStart\" X\"Material1\" Y\"1.0\";";
		public const string DEFAULT_Tail = "G65 P\"FileEnd\";\nM30;";
		public const string DEFAULT_PathHeader = "G65 P\"LASER_ON\";";
		public const string DEFAULT_PathTail = "G65 P\"LASER_OFF\";";
		public const string DEFAULT_StdPathHeader = "";
		public const string DEFAULT_StdPathTail = "";
		public const string DEFAULT_TraverseHeader = "// Cutting{PathIndex}\nN{PathIndex}\nG65 P\"LoadParameter\" H{LayerIndex};";
		public const string DEFAULT_TraverseTail = "";

		public enum EPostSection
		{
			Header,
			Tail,
			PathHeader,
			PathTail,
			StdPathHeader,
			StdPathTail,
			TraverseHeader,
			TraverseTail
		}
	}
}
