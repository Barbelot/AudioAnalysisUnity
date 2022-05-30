using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;
using UnityEngine.Playables;

[ExecuteAlways]
public class AudioLineDrawer : ImmediateModeShapeDrawer
{
	public AudioClipAmplitude audioClipAmplitude;

	[Header("Timeline")]
	public bool useTimeline = false;
	public PlayableDirector director;
	public float timeLength;

	[Header("Drawing")]
	public Vector3 lineStart;
	public Vector3 lineEnd;
	public float lineThickness = 0.1f;
	[ColorUsage(true, true)] public Color lineColor = Color.white;
	public float lineHeight = 1;
	public float beforeAndAfterLength = 0;
	public Vector2 timeRange = new Vector2(-1, 1);

	[Space]
	public bool drawMask = false;
	public float maskHeight = 0;

	[Header("Debug")]
	public bool debugLineSegments = false;
	public bool updateMask = false;

	PolygonPath _maskPath;

	private void Awake() {

		if (drawMask)
			UpdateMask();
	}

	private void Update() {
		
		if(updateMask) { UpdateMask(); updateMask = false; }
	}

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

			if (drawMask) {
				if (_maskPath == null)
					UpdateMask();
				Draw.Polygon(_maskPath, lineColor);
			} else {
				if (useTimeline && director && timeLength > 0) {

					float time = (float)director.time;
					float waveformTimeStep = timeLength / audioClipAmplitude.waveform.Length;

					int startIndex = Mathf.Max(0, Mathf.FloorToInt((time + timeRange.x) / waveformTimeStep));
					int endIndex = Mathf.Min(audioClipAmplitude.waveform.Length, Mathf.CeilToInt((time + timeRange.y) / waveformTimeStep));
					float percent = Mathf.InverseLerp((endIndex - 1) * waveformTimeStep, endIndex * waveformTimeStep, time);

					Vector3 start = lineStart - Vector3.right * beforeAndAfterLength;
					Vector3 end = lineStart;

					if(startIndex <= 0) {
						end.y = start.y = audioClipAmplitude.waveform[0] * lineHeight;
						DrawLine(start, end, lineColor);
					}

					for(int i=startIndex; i<endIndex-1; i++) {
						start.x = Mathf.Lerp(lineStart.x, lineEnd.x, (float)(i) / audioClipAmplitude.waveform.Length);
						start.y = audioClipAmplitude.waveform[i] * lineHeight;

						end.x = Mathf.Lerp(lineStart.x, lineEnd.x, (float)(i + 1) / audioClipAmplitude.waveform.Length);
						end.y = audioClipAmplitude.waveform[i + 1] * lineHeight;

						DrawLine(start, end, lineColor);
					}

					start = end;
					end.x = endIndex >= audioClipAmplitude.waveform.Length ? lineEnd.x + beforeAndAfterLength : 
						Mathf.Lerp(Mathf.Lerp(lineStart.x, lineEnd.x, (float)(endIndex-1) / audioClipAmplitude.waveform.Length), 
								   Mathf.Lerp(lineStart.x, lineEnd.x, (float)(endIndex) / audioClipAmplitude.waveform.Length), 
								   percent);
					end.y = Mathf.Lerp(audioClipAmplitude.waveform[endIndex-1] * lineHeight, audioClipAmplitude.waveform[endIndex] * lineHeight, percent);
					DrawLine(start, end, lineColor);

				} else {

					Vector3 start = lineStart - Vector3.right * beforeAndAfterLength;
					Vector3 end = lineStart;

					end.y = start.y = audioClipAmplitude.waveform[0] * lineHeight;

					DrawLine(start, end, lineColor);

					// draw lines
					for (int i = 0; i < audioClipAmplitude.waveform.Length - 1; i++) {
						start = end;
						end.x = Mathf.Lerp(lineStart.x, lineEnd.x, (float)(i + 1) / audioClipAmplitude.waveform.Length);
						end.y = audioClipAmplitude.waveform[i + 1] * lineHeight;

						DrawLine(start, end, lineColor);
					}

					start = end;
					end.x += beforeAndAfterLength;

					DrawLine(start, end, lineColor);
				}
			}
		}
	}

	void DrawLine(Vector3 start, Vector3 end, Color color) {

		if (debugLineSegments) {
			Draw.Line(start, end, Color.blue, Color.red);
		} else { 
			Draw.Line(start, end, color);
		}
	}

	void UpdateMask() {

		_maskPath = new PolygonPath();

		Vector3 start = lineStart - Vector3.right * beforeAndAfterLength;
		Vector3 end = lineStart;

		end.y = start.y = audioClipAmplitude.waveform[0] * lineHeight;

		_maskPath.AddPoint(start.x, maskHeight);
		_maskPath.AddPoint(start.x, start.y);
		_maskPath.AddPoint(end.x, end.y);

		// draw lines
		for (int i = 0; i < audioClipAmplitude.waveform.Length - 1; i++) {
			start = end;
			end.x = Mathf.Lerp(lineStart.x, lineEnd.x, (float)(i + 1) / audioClipAmplitude.waveform.Length);
			end.y = audioClipAmplitude.waveform[i + 1] * lineHeight;

			_maskPath.AddPoint(end.x, end.y);
		}

		start = end;
		end.x += beforeAndAfterLength;

		_maskPath.AddPoint(end.x, end.y);
		_maskPath.AddPoint(end.x, maskHeight);
	}
}
