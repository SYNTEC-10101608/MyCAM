namespace MyCAM.Data
{
	public enum FiveAxisType
	{
		None = 0,
		Spindle = 1,
		Table = 2,
		Mix = 3,
	}

	public enum ToolDirection
	{
		None = 0,
		X = 1,
		Y = 2,
		Z = 3,
	}

	public enum RotaryAxis
	{
		None = 0,
		X = 1,
		Y = 2,
		Z = 3,
	}

	public enum RotaryDirection
	{
		None = 0,
		RightHand = 1,
		LeftHand = 2,
	}

	public enum MachineComponentType
	{
		Base = 0,
		XAxis = 1,
		YAxis = 2,
		ZAxis = 3,
		Master = 4,
		Slave = 5,
		WorkPiece = 6,
		Tool = 7,
	}
}
