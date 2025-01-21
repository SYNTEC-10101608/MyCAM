namespace OCCTool
{
	public enum AISActiveMode
	{
		Default = 0,
		Vertex = 1,
		Edge = 2,
		Wire = 3,
		Face = 4,
	}

	public enum Graphic3d_ZLayerId
	{
		Graphic3d_ZLayerId_UNKNOWN = -1,
		Graphic3d_ZLayerId_Default = 0,
		Graphic3d_ZLayerId_Top = -2,
		Graphic3d_ZLayerId_Topmost = -3,
		Graphic3d_ZLayerId_TopOSD = -4,
		Graphic3d_ZLayerId_BotOSD = -5
	}
}
