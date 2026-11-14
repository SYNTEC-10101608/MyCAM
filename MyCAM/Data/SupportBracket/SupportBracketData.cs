namespace MyCAM.Data
{
	// support bracket design parameters
	public class SupportBracketData
	{
		public string Version
		{
			get; set;
		} = "1";

		// Bottom section 
		public double XExtension
		{
			get; set;
		} = 50.0;

		public double YExtension
		{
			get; set;
		} = 50.0;

		public double BaseThick
		{
			get; set;
		} = 5.0;

		// Bracket positions 
		public double[] XPositions
		{
			get; set;
		} = new double[ 0 ];

		public double[] YPositions
		{
			get; set;
		} = new double[ 0 ];

		// Bracket section 
		public double CrossGap
		{
			get; set;
		} = 5.0;

		public int XPinCount
		{
			get; set;
		} = 8;

		public int YPinCount
		{
			get; set;
		} = 2;

		public double WallThick
		{
			get; set;
		} = 3.0;

		public double RibHeight
		{
			get; set;
		} = 50.0;

		public double ZExtension
		{
			get; set;
		} = 100.0;

		public double PinLength
		{
			get; set;
		} = 20.0;

		public double FilletRadius
		{
			get; set;
		} = 1.0;
	}
}
