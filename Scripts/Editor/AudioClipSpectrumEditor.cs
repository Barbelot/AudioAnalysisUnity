using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioClipSpectrum))]
[CanEditMultipleObjects]
public class AudioClipSpectrumEditor : Editor
{
    private Material material;

    void OnEnable() {
        // Find the "Hidden/Internal-Colored" shader, and cache it for use.
        material = new Material(Shader.Find("Hidden/Internal-Colored"));
    }

    public override void OnInspectorGUI() {

        var audioClipSpectrum = target as AudioClipSpectrum;

        DrawDefaultInspector();

        // Begin to draw a horizontal layout, using the helpBox EditorStyle
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        // Reserve GUI space with a width from 10 to 10000, and a fixed height of 200, and 
        // cache it as a rectangle.
        Rect layoutRectangle = GUILayoutUtility.GetRect(10, 10000, 200, 200);

        if (Event.current.type == EventType.Repaint) {
            // If we are currently in the Repaint event, begin to draw a clip of the size of 
            // previously reserved rectangle, and push the current matrix for drawing.
            GUI.BeginClip(layoutRectangle);
            GL.PushMatrix();

            // Clear the current render buffer, setting a new background colour, and set our
            // material for rendering.
            GL.Clear(true, false, Color.black);
            material.SetPass(0);

            // Start drawing in OpenGL Quads, to draw the background canvas. Set the
            // colour black as the current OpenGL drawing colour, and draw a quad covering
            // the dimensions of the layoutRectangle.
            GL.Begin(GL.QUADS);
            GL.Color(Color.black);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(layoutRectangle.width, 0, 0);
            GL.Vertex3(layoutRectangle.width, layoutRectangle.height, 0);
            GL.Vertex3(0, layoutRectangle.height, 0);
            GL.End();

            // Start drawing in OpenGL Lines, to draw the lines of the grid.
            GL.Begin(GL.LINES);

            GL.Color(Color.white);
            for (int i = 0; i < audioClipSpectrum.spectrum.Length * audioClipSpectrum.spectrumDisplayRange; i++) {

                GL.Vertex3((float)i / audioClipSpectrum.spectrum.Length * layoutRectangle.width / audioClipSpectrum.spectrumDisplayRange, Mathf.Max(0, layoutRectangle.height * 0.5f - audioClipSpectrum.spectrum[i] * 0.5f * audioClipSpectrum.spectrumScale), 0);
                GL.Vertex3((float)i / audioClipSpectrum.spectrum.Length * layoutRectangle.width / audioClipSpectrum.spectrumDisplayRange, Mathf.Min(layoutRectangle.height, layoutRectangle.height * 0.5f + audioClipSpectrum.spectrum[i] * 0.5f * audioClipSpectrum.spectrumScale), 0);
            }

            // End lines drawing.
            GL.End();

            // Pop the current matrix for rendering, and end the drawing clip.
            GL.PopMatrix();
            GUI.EndClip();
        }

        // End our horizontal 
        GUILayout.EndHorizontal();
    }
}
