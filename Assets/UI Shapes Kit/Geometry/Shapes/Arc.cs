using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Co.Appreal.UI_ShapesKit
{
	[AddComponentMenu("UI/Shapes/Arc", 50)]
    [RequireComponent(typeof(CanvasRenderer))]
    public class Arc : MaskableGraphic, IShape
	{
		public GeoUtils.ShapeProperties ShapeProperties =
			new GeoUtils.ShapeProperties();

		public Ellipses.EllipseProperties EllipseProperties =
			new Ellipses.EllipseProperties();

		public Arcs.ArcProperties ArcProperties = 
			new Arcs.ArcProperties();

		public Lines.LineProperties LineProperties =
			new Lines.LineProperties();
		public PointsList.PointListProperties PointListProperties =
				new PointsList.PointListProperties();
		PointsList.PointsData PointsData = new PointsList.PointsData();

		public GeoUtils.OutlineProperties OutlineProperties = 
			new GeoUtils.OutlineProperties();

		public GeoUtils.ShadowsProperties ShadowProperties = new GeoUtils.ShadowsProperties();

		public GeoUtils.AntiAliasingProperties AntiAliasingProperties = 
			new GeoUtils.AntiAliasingProperties();


		GeoUtils.UnitPositionData unitPositionData;
		GeoUtils.EdgeGradientData edgeGradientData;
		Vector2 radius = Vector2.one;

		protected override void OnEnable() {
			PointListProperties.GeneratorData.Generator =
				PointsList.PointListGeneratorData.Generators.Round;

			PointListProperties.GeneratorData.Center.x = 0.0f;
			PointListProperties.GeneratorData.Center.y = 0.0f;

			base.OnEnable();
		}

		public void ForceMeshUpdate()
		{
			PointListProperties.GeneratorData.NeedsUpdate = true;
			PointsData.NeedsUpdate = true;

			SetVerticesDirty();
			SetMaterialDirty();
		}

		#if UNITY_EDITOR
		protected override void OnValidate()
		{
			EllipseProperties.OnCheck();
			OutlineProperties.OnCheck();
			AntiAliasingProperties.OnCheck();

			ForceMeshUpdate();
		}
		#endif

		protected override void OnPopulateMesh(VertexHelper vh)	{
			vh.Clear();

			Rect pixelRect = RectTransformUtility.PixelAdjustRect(rectTransform, canvas);

			OutlineProperties.UpdateAdjusted();
			ShadowProperties.UpdateAdjusted();

			Ellipses.SetRadius(
				ref radius,
				pixelRect.width,
				pixelRect.height,
				EllipseProperties
			);

			PointListProperties.GeneratorData.Width = radius.x * 2.0f;
			PointListProperties.GeneratorData.Height = radius.y * 2.0f;

			EllipseProperties.UpdateAdjusted(radius, OutlineProperties.GetOuterDistace());
			ArcProperties.UpdateAdjusted(EllipseProperties.AdjustedResolution, EllipseProperties.BaseAngle);
			AntiAliasingProperties.UpdateAdjusted(canvas);

			PointListProperties.GeneratorData.Resolution = EllipseProperties.AdjustedResolution * 2;
			PointListProperties.GeneratorData.Length = ArcProperties.Length;

			switch (ArcProperties.Direction) {
				case Arcs.ArcProperties.ArcDirection.Forward:
					PointListProperties.GeneratorData.Direction = 1.0f;
					PointListProperties.GeneratorData.FloatStartOffset = EllipseProperties.BaseAngle * 0.5f;
					break;

				case Arcs.ArcProperties.ArcDirection.Centered:
					PointListProperties.GeneratorData.Direction = -1.0f;
					PointListProperties.GeneratorData.FloatStartOffset = EllipseProperties.BaseAngle * 0.5f + (ArcProperties.Length * 0.5f);
					break;

				case Arcs.ArcProperties.ArcDirection.Backward:
					PointListProperties.GeneratorData.Direction = -1.0f;
					PointListProperties.GeneratorData.FloatStartOffset = EllipseProperties.BaseAngle * 0.5f;
					break;
			}

			// shadows
			if (ShadowProperties.ShadowsEnabled)
			{
				if (AntiAliasingProperties.Adjusted > 0.0f)
				{
					edgeGradientData.SetActiveData(
						1.0f,
						0.0f,
						AntiAliasingProperties.Adjusted
					);
				}
				else
				{
					edgeGradientData.Reset();
				}

				// use segment if LineWeight is overshooting the center
				if (
					(
						OutlineProperties.Type == GeoUtils.OutlineProperties.LineType.Center ||
						OutlineProperties.Type == GeoUtils.OutlineProperties.LineType.Inner
					) &&
					(
						radius.x + OutlineProperties.GetInnerDistace() < 0.0f ||
						radius.y + OutlineProperties.GetInnerDistace() < 0.0f
					)
				) {
					if (OutlineProperties.Type == GeoUtils.OutlineProperties.LineType.Center)
					{
						radius *= 2.0f;
					}

					for (int i = 0; i < ShadowProperties.Shadows.Length; i++)
					{
						edgeGradientData.SetActiveData(
							1.0f - ShadowProperties.Shadows[i].Softness,
							ShadowProperties.Shadows[i].Size,
							AntiAliasingProperties.Adjusted
						);

						Arcs.AddSegment(
							ref vh,
							ShadowProperties.GetCenterOffset(pixelRect.center, i),
							radius,
							EllipseProperties,
							ArcProperties,
							ShadowProperties.Shadows[i].Color,
							GeoUtils.ZeroV2,
							ref unitPositionData,
							edgeGradientData
						);
					}

				}
				else
				{
					for (int i = 0; i < ShadowProperties.Shadows.Length; i++)
					{
						edgeGradientData.SetActiveData(
							1.0f - ShadowProperties.Shadows[i].Softness,
							ShadowProperties.Shadows[i].Size,
							AntiAliasingProperties.Adjusted
						);

						if (LineProperties.LineCap == Lines.LineProperties.LineCapTypes.Close)
						{
							Arcs.AddArcRing(
								ref vh,
								ShadowProperties.GetCenterOffset(pixelRect.center, i),
								radius,
								EllipseProperties,
								ArcProperties,
								OutlineProperties,
								ShadowProperties.Shadows[i].Color,
								GeoUtils.ZeroV2,
								ref unitPositionData,
								edgeGradientData
							);
						}
						else
						{
							Lines.AddLine(
								ref vh,
								LineProperties,
								PointListProperties,
								ShadowProperties.GetCenterOffset(pixelRect.center, i),
								OutlineProperties,
								ShadowProperties.Shadows[i].Color,
								GeoUtils.ZeroV2,
								ref PointsData,
								edgeGradientData
							);
						}
					}

				}
			}

			// fill
			if (ShadowProperties.ShowShape)
			{
				if (AntiAliasingProperties.Adjusted > 0.0f)
				{
					edgeGradientData.SetActiveData(
						1.0f,
						0.0f,
						AntiAliasingProperties.Adjusted
					);
				}
				else
				{
					edgeGradientData.Reset();
				}

				// use segment if LineWeight is overshooting the center
				if (
					(
						OutlineProperties.Type == GeoUtils.OutlineProperties.LineType.Center ||
						OutlineProperties.Type == GeoUtils.OutlineProperties.LineType.Inner
					) &&
					(
						radius.x + OutlineProperties.GetInnerDistace() < 0.0f ||
						radius.y + OutlineProperties.GetInnerDistace() < 0.0f
					)

				) {
					if (OutlineProperties.Type == GeoUtils.OutlineProperties.LineType.Center)
					{
						radius.x *= 2.0f;
						radius.y *= 2.0f;
					}

					Arcs.AddSegment(
						ref vh,
						pixelRect.center,
						radius,
						EllipseProperties,
						ArcProperties,
						ShapeProperties.FillColor,
						GeoUtils.ZeroV2,
						ref unitPositionData,
						edgeGradientData
					);
				}
				else
				{
					if (LineProperties.LineCap == Lines.LineProperties.LineCapTypes.Close)
					{
						Arcs.AddArcRing(
							ref vh,
							pixelRect.center,
							radius,
							EllipseProperties,
							ArcProperties,
							OutlineProperties,
							ShapeProperties.FillColor,
							GeoUtils.ZeroV2,
							ref unitPositionData,
							edgeGradientData
						);
					}
					else
					{
						Lines.AddLine(
							ref vh,
							LineProperties,
							PointListProperties,
							pixelRect.center,
							OutlineProperties,
							ShapeProperties.FillColor,
							GeoUtils.ZeroV2,
							ref PointsData,
							edgeGradientData
						);
					}
				}
			}


		}
	}

}