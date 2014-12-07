﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SmartStage
{
	public class AscentPlot
	{
		public Texture2D texture;

		private List<Sample> samples;
		private List<StageDescription> stages;
		private List<PlotElement> plots = new List<PlotElement>();

		private class PlotElement
		{
			Color colour;
			public readonly string name;
			List<double> time;
			List<double> values;
			public bool active;
			public float pulse;
			public readonly GUIStyle buttonStyle;

			public PlotElement(string name, List<double> time, List<double> values, Color colour)
			{
				this.name = name;
				this.time = time;
				this.values = values;
				this.colour = colour;
				active = true;
				buttonStyle = new GUISkin().button;
				buttonStyle.normal.textColor = colour;
				buttonStyle.hover.textColor = colour;
				buttonStyle.active.textColor = colour;
			}

			public void draw(Texture2D texture)
			{
				if (!active)
					return;

				Color pulsed = Color.Lerp(Color.white, colour, (float)Math.Pow(Math.Cos(pulse)/2 + 1, 3));

				double timeToX = texture.width / time.Last();
				double valToY = texture.height / values.Max();
				for (int i = 1 ; i < time.Count() ; i++)
				{
					texture.SetPixel((int)(time[i] * timeToX), (int)(values[i] * valToY), pulsed);
				}
			}
		}

		public AscentPlot (List<Sample> samples, List<StageDescription> stages, int xdim, int ydim)
		{
			this.samples = samples;
			this.stages = stages;
			List<double> times = samples.ConvertAll(s => s.time);
			plots.Add(new PlotElement("acceleration", times, samples.ConvertAll(s => s.acceleration), new Color(0.3f, 0.3f, 1)));
			plots.Add(new PlotElement("velocity", times, samples.ConvertAll(s => s.velocity), new Color(1, 0.3f, 0.3f)));
			texture = new Texture2D(xdim, ydim, TextureFormat.RGB24, false);
			drawTexture();
		}

		private void drawTexture()
		{
			fillBackground();
			foreach(var e in plots)
				e.draw(texture);
			texture.Apply();
		}

		private void fillBackground()
		{
			Color even = new Color(0,0,0);
			Color odd = new Color(0.2f, 0.2f, 0.2f);
			double xToTime = samples.Last().time / texture.width;
			int x = 0;
			Color colour = even;
			foreach (var stage in stages)
			{
				for (;x * xToTime < stage.activationTime; x++)
				{
					for (int y = 0 ; y < texture.height ; y++)
					{
						texture.SetPixel(x, y, colour);
					}
				}
				colour = (colour == even) ? odd : even;
			}
			for (;x < texture.width ; x++)
			{
				for (int y = 0 ; y < texture.height ; y++)
				{
					texture.SetPixel(x, y, colour);
				}
			}
		}

		public void draw()
		{
			GUILayout.BeginHorizontal();
			GUILayout.Box(texture, GUIStyle.none, new GUILayoutOption[] { GUILayout.Width(texture.width), GUILayout.Height(texture.height)});
			GUILayout.BeginVertical();
			foreach (var e in plots)
			{
				if (GUILayout.Button(e.name, e.buttonStyle))
				{
					e.active = ! e.active;
				}
				if (Event.current.type == EventType.Repaint)
				{
					if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
					{
						e.pulse = (float)(Time.time * 2 * Math.PI);
						GUI.changed = true;
					}
					else
					{
						if (e.pulse != 0)
							GUI.changed = true;
						e.pulse = 0;
					}
				}
			}
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			if (GUI.changed)
				drawTexture();
		}
	}
}

