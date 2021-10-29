﻿using Mapsui.UI.Objects;
using Mapsui.Styles;
using Mapsui.Providers;
using System.Runtime.CompilerServices;
using System.Linq;
using System;
#if __MAUI__
using Mapsui.UI.Maui.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Mapsui.UI.Maui;

using Color = Microsoft.Maui.Graphics.Color;
using KnownColor = Mapsui.UI.Maui.KnownColor;
#else
using Xamarin.Forms;
using Mapsui.UI.Forms.Extensions;

using Color = Xamarin.Forms.Color;
using KnownColor = Xamarin.Forms.Color;
#endif

#if __MAUI__
namespace Mapsui.UI.Maui
#else
namespace Mapsui.UI.Forms
#endif
{
    public class Circle : Drawable
    {
        public static readonly BindableProperty CenterProperty = BindableProperty.Create(nameof(Center), typeof(Position), typeof(Circle), default(Position));
        public static readonly BindableProperty RadiusProperty = BindableProperty.Create(nameof(Radius), typeof(Distance), typeof(Circle), Distance.FromMeters(1));
        public static readonly BindableProperty QualityProperty = BindableProperty.Create(nameof(Quality), typeof(double), typeof(Circle), 3.0);
        public static readonly BindableProperty FillColorProperty = BindableProperty.Create(nameof(FillColor), typeof(Color), typeof(Circle), KnownColor.DarkGray);

        public Circle()
        {
            CreateFeature();
        }

        /// <summary>
        /// Center of circle
        /// </summary>
        public Position Center
        {
            get => (Position)GetValue(CenterProperty);
            set => SetValue(CenterProperty, value);
        }

        /// <summary>
        /// Radius of circle in meters
        /// </summary>
        public Distance Radius
        {
            get => (Distance)GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
        }

        /// <summary>
        /// Color to fill circle with
        /// </summary>
        public Color FillColor
        {
            get => (Color)GetValue(FillColorProperty);
            set => SetValue(FillColorProperty, value);
        }

        /// <summary>
        /// Quality for circle. Determines, how many points used to draw circle. 3 is poorest, 360 best quality.
        /// </summary>
        public double Quality
        {
            get => (double)GetValue(QualityProperty);
            set => SetValue(QualityProperty, value);
        }

        protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(Center):
                    UpdateFeature();
                    break;
                case nameof(Radius):
                    UpdateFeature();
                    break;
                case nameof(Quality):
                    UpdateFeature();
                    break;
                case nameof(FillColor):
                    ((VectorStyle)Feature.Styles.First()).Fill = new Styles.Brush(FillColor.ToMapsui());
                    break;
                case nameof(StrokeWidth):
                    ((VectorStyle)Feature.Styles.First()).Outline.Width = StrokeWidth;
                    break;
                case nameof(StrokeColor):
                    ((VectorStyle)Feature.Styles.First()).Outline.Color = StrokeColor.ToMapsui();
                    break;
            }
        }

        private readonly object _sync = new object();

        private void CreateFeature()
        {
            lock (_sync)
            {
                if (Feature == null)
                {
                    // Create a new one
                    Feature = new Feature
                    {
                        Geometry = new Geometries.Polygon(),
                        ["Label"] = Label,
                    };
                    Feature.Styles.Clear();
                    Feature.Styles.Add(new VectorStyle
                    {
                        Outline = new Pen { Width = StrokeWidth, Color = StrokeColor.ToMapsui() },
                        Fill = new Styles.Brush { Color = FillColor.ToMapsui() }
                    });
                }
            }
        }

        private void UpdateFeature()
        {
            if (Feature == null)
            {
                // Create a new one
                CreateFeature();
            }

            // Create new circle
            var centerX = Center.ToMapsui().X;
            var centerY = Center.ToMapsui().Y;
            var radius = Radius.Meters / Math.Cos(Center.Latitude / 180.0 * Math.PI);
            var increment = 360.0 / (Quality < 3.0 ? 3.0 : (Quality > 360.0 ? 360.0 : Quality));
            var exteriorRing = new Geometries.LinearRing();

            for (double angle = 0; angle < 360; angle += increment)
            {
                var angleRad = angle / 180.0 * Math.PI;
                exteriorRing.Vertices.Add(new Geometries.Point(radius * Math.Sin(angleRad) + centerX, radius * Math.Cos(angleRad) + centerY));
            }

            Feature.Geometry = new Geometries.Polygon(exteriorRing);
        }
    }
}