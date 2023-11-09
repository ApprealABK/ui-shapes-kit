using UnityEngine;

namespace ThisOtherThing.Appreal.UI_ShapesKit.Utils
{
	public class MinAttribute : PropertyAttribute
	{
		public readonly float minFloat;
		public readonly int minInt;

		public MinAttribute(float min)
		{
			this.minFloat = min;
		}

		public MinAttribute(int min)
		{
			this.minInt = min;
		}
	}
}