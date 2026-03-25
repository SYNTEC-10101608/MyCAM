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

		// constructor with default content
		public CustPost()
		{
			Header = DEFAULT_Header;
			Tail = DEFAULT_Tail;
			PathHeader = DEFAULT_PathHeader;
			PathTail = DEFAULT_PathTail;
			StdPathHeader = DEFAULT_StdPathHeader;
			StdPathTail = DEFAULT_StdPathTail;
		}

		public CustPost( string header, string tail, string pathHeader, string pathTail, string stdPathHeader, string stdPathTail )
		{
			Header = header;
			Tail = tail;
			PathHeader = pathHeader;
			PathTail = pathTail;
			StdPathHeader = stdPathHeader;
			StdPathTail = stdPathTail;
		}

		public const string DEFAULT_Header = "%@MACRO\nG43.4 P1;\nG65 P\"FileStart\" X\"Material1\" Y\"1.0\";";
		public const string DEFAULT_Tail = "G65 P\"FileEnd\";\nM30;";
		public const string DEFAULT_PathHeader = "// Cutting{PathIndex}\nN{PathIndex}\nG65 P\"LoadParameter\" H{LayerIndex};";
		public const string DEFAULT_PathTail = "G65 P\"LASER_OFF\";";
		public const string DEFAULT_StdPathHeader = "// Cutting{PathIndex}\nN{PathIndex}\nG65 P\"LoadParameter\" H{LayerIndex};";
		public const string DEFAULT_StdPathTail = "";
	}
}
