using System;
using System.Globalization;

namespace CncManager.Definition
{
	public sealed class CncAlarm
	{
		public CncAlarm( int classID, string category, int objectID, int alarmID, int subAlarmID, DateTime time, string text )
		{
			ClassID = classID;
			Category = category;
			ObjectID = objectID;
			AlarmID = alarmID;
			SubAlarmID = subAlarmID;
			Time = time;
			Text = text ?? string.Empty;
		}

		// Raw SDK alarm class ID.
		public int ClassID { get; }
		// SDK class name without the ALARMCLASS_ prefix.
		public string Category { get; }
		// SDK object ID; axis number for MOTOR alarms.
		public int ObjectID { get; }
		// Alarm number within the class and object.
		public int AlarmID { get; }
		// Additional sub-alarm identifier.
		public int SubAlarmID { get; }
		// Timestamp reported by the controller.
		public DateTime Time { get; }
		// Controller hint text; never null.
		public string Text { get; }

		// Display code: category-object-alarm-subalarm.
		public string Code => string.Format( CultureInfo.InvariantCulture, "{0}-{1}-{2}-{3}", Category, ObjectID, AlarmID, SubAlarmID );

		// De-duplication key including the controller timestamp.
		public string Key => string.Format( CultureInfo.InvariantCulture, "{0}:{1}:{2}:{3}:{4}", ClassID, ObjectID, AlarmID, SubAlarmID, Time.Ticks );
	}
}
