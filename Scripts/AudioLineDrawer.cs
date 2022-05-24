using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;

[ExecuteAlways]
public class AudioLineDrawer : ImmediateModeShapeDrawer
{
	public AudioClipAmplitude audioClipAmplitude;

	[Header("Drawing")]
	public Vector3 lineStart;
	public Vector3 lineEnd;
	public float lineThickness = 0.1f;
	public Color lineColor = Color.white;
	public float lineHeight = 1;

	[Header("Debug")]
	public bool debugLineSegments = false;

	public override void DrawShapes(Camera cam) {

		if (!audioClipAmplitude)
			return;

		using (Draw.Command(cam)) {

			// set up static parameters. these are used for all following Draw.Line calls
			Draw.LineGeometry = LineGeometry.Billboard;
			Draw.ThicknessSpace = ThicknessSpace.Meters;
			Draw.Thickness = lineThickness;

			// set static parameter to draw in the local space of this object
			Draw.Matrix = transform.localToWorldMatrix;

			Vector3 start = lineStart;
			Vector3 end = lineStart;

			end.y = audioClipAmplitude.waveform[0] * lineHeight;

			// draw lines
			for (int i=0; i<audioClipAmplitude.waveform.Length-1; i++) {
				start = end;
				end.x = Mathf.Lerp(lineStart.x, lineEnd.x, (float)(i + 1) / audioClipAmplitude.waveform.Length);
				end.y = audioClipAmplitude.waveform[i + 1] * lineHeight;

				if (debugLineSegments) {
					Draw.Line(start, end, Color.blue, Color.red);
				} else {
					Draw.Line(start, end, lineColor);
				}
			}

		}
	}
}
