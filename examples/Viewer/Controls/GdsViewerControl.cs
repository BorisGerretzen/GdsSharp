using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using GdsSharp.Lib;
using GdsSharp.Lib.Library;
using GdsSharp.Lib.Library.Views;
using GdsSharp.Lib.Library.Views.Elements;

namespace Viewer.Controls;

public class GdsViewerControl : Control
{
    private GdsLibrary? _library;
    private StructureView? _currentStructure;

    // Viewport transform
    private double _scale = 1.0;
    private double _offsetX;
    private double _offsetY;
    private Point _lastPanPoint;
    private bool _isPanning;

    // Layer colors
    private static readonly Dictionary<short, IBrush> LayerColors = new();
    private static readonly IBrush[] ColorPalette =
    [
        Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.Yellow,
        Brushes.Cyan, Brushes.Magenta, Brushes.Orange, Brushes.Purple,
        Brushes.LimeGreen, Brushes.DeepPink, Brushes.DodgerBlue, Brushes.Gold,
        Brushes.Coral, Brushes.Teal, Brushes.Crimson, Brushes.SlateBlue
    ];

    public GdsViewerControl()
    {
        ClipToBounds = true;
    }

    public void LoadLibrary(GdsLibrary library, string? structureName = null)
    {
        _library = library;
        LayerColors.Clear();

        var view = library.AsView();

        if (structureName != null && view.TryGetStructure(structureName, out var structure))
        {
            _currentStructure = structure;
        }
        else
        {
            // Pick the first structure or the last one (often the top-level cell)
            StructureView? lastStructure = null;
            foreach (var s in view.Structures)
            {
                lastStructure = s;
            }
            _currentStructure = lastStructure;
        }

        FitToView();
        InvalidateVisual();
    }

    public IEnumerable<string> GetStructureNames()
    {
        if (_library == null) yield break;
        foreach (var s in _library.AsView().Structures)
        {
            yield return s.Name;
        }
    }

    public void SetStructure(string name)
    {
        if (_library == null) return;
        if (_library.AsView().TryGetStructure(name, out var structure))
        {
            _currentStructure = structure;
            FitToView();
            InvalidateVisual();
        }
    }

    public void FitToView()
    {
        if (_currentStructure == null) return;

        var bbox = _currentStructure.Value.BoundingBox;
        if (bbox == null || bbox.Value.IsEmpty) return;

        var box = bbox.Value;
        var width = box.Max.X - box.Min.X;
        var height = box.Max.Y - box.Min.Y;

        if (width <= 0 || height <= 0) return;

        var viewWidth = Bounds.Width > 0 ? Bounds.Width : 800;
        var viewHeight = Bounds.Height > 0 ? Bounds.Height : 600;

        // Calculate scale to fit with some margin
        var scaleX = (viewWidth * 0.9) / width;
        var scaleY = (viewHeight * 0.9) / height;
        _scale = Math.Min(scaleX, scaleY);

        // Center the view
        var centerX = (box.Min.X + box.Max.X) / 2.0;
        var centerY = (box.Min.Y + box.Max.Y) / 2.0;

        _offsetX = viewWidth / 2 - centerX * _scale;
        _offsetY = viewHeight / 2 + centerY * _scale; // Y is flipped
    }

    private Point GdsToScreen(GdsPoint pt)
    {
        return new Point(
            pt.X * _scale + _offsetX,
            -pt.Y * _scale + _offsetY // Flip Y axis
        );
    }

    private static IBrush GetLayerBrush(short layer)
    {
        if (!LayerColors.TryGetValue(layer, out var brush))
        {
            brush = ColorPalette[layer % ColorPalette.Length];
            LayerColors[layer] = brush;
        }
        return brush;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // Draw background
        context.FillRectangle(Brushes.Black, new Rect(Bounds.Size));

        if (_library == null || _currentStructure == null) return;

        // Draw the current structure
        DrawStructure(context, _currentStructure.Value, new GdsPoint(0, 0), 0);
    }

    private void DrawStructure(DrawingContext context, StructureView structure, GdsPoint offset, int depth)
    {
        if (depth > 50) return; // Prevent infinite recursion

        // Draw boundaries (polygons)
        foreach (var boundary in structure.Boundaries)
        {
            DrawBoundary(context, boundary, offset);
        }

        // Draw paths
        foreach (var path in structure.Paths)
        {
            DrawPath(context, path, offset);
        }

        // Draw structure references recursively
        foreach (var sref in structure.StructureReferences)
        {
            if (_library!.AsView().TryGetStructure(sref.TargetName, out var refStructure))
            {
                var newOffset = new GdsPoint(offset.X + sref.Origin.X, offset.Y + sref.Origin.Y);
                DrawStructure(context, refStructure, newOffset, depth + 1);
            }
        }

        // Draw array references
        foreach (var aref in structure.ArrayReferences)
        {
            DrawArrayReference(context, aref, offset, depth);
        }
    }

    private void DrawBoundary(DrawingContext context, BoundaryView boundary, GdsPoint offset)
    {
        if (boundary.PointCount < 3) return;

        using var points = boundary.GetPoints();
        var span = points.Span;
        var brush = GetLayerBrush(boundary.Layer);

        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.SetFillRule(FillRule.NonZero);
            var first = GdsToScreen(new GdsPoint(span[0].X + offset.X, span[0].Y + offset.Y));
            ctx.BeginFigure(first, true);

            for (var i = 1; i < points.Length-1; i++)
            {
                var pt = GdsToScreen(new GdsPoint(span[i].X + offset.X, span[i].Y + offset.Y));
                ctx.LineTo(pt);
            }

            ctx.EndFigure(false);
        }

        context.DrawGeometry(brush, null, geometry);
    }

    private void DrawPath(DrawingContext context, PathView path, GdsPoint offset)
    {
        if (path.PointCount < 2) return;

        using var points = path.GetPoints();
        var span = points.Span;
        var brush = GetLayerBrush(path.Layer);
        var pen = new Pen(brush, Math.Max(1, (path.Width ?? 0) * _scale));

        for (var i = 0; i < points.Length - 1; i++)
        {
            var p1 = GdsToScreen(new GdsPoint(span[i].X + offset.X, span[i].Y + offset.Y));
            var p2 = GdsToScreen(new GdsPoint(span[i + 1].X + offset.X, span[i + 1].Y + offset.Y));
            context.DrawLine(pen, p1, p2);
        }
    }

    private void DrawArrayReference(DrawingContext context, ArrayReferenceView aref, GdsPoint offset, int depth)
    {
        if (!_library!.AsView().TryGetStructure(aref.TargetName, out var refStructure)) return;

        var cols = aref.Columns;
        var rows = aref.Rows;

        // Calculate spacing from the column and row vectors
        var colSpacingX = cols > 1 ? (aref.ColumnVector.X - aref.Origin.X) / cols : 0;
        var colSpacingY = cols > 1 ? (aref.ColumnVector.Y - aref.Origin.Y) / cols : 0;
        var rowSpacingX = rows > 1 ? (aref.RowVector.X - aref.Origin.X) / rows : 0;
        var rowSpacingY = rows > 1 ? (aref.RowVector.Y - aref.Origin.Y) / rows : 0;

        for (var col = 0; col < cols; col++)
        {
            for (var row = 0; row < rows; row++)
            {
                var instanceOffset = new GdsPoint(
                    offset.X + aref.Origin.X + col * colSpacingX + row * rowSpacingX,
                    offset.Y + aref.Origin.Y + col * colSpacingY + row * rowSpacingY
                );
                DrawStructure(context, refStructure, instanceOffset, depth + 1);
            }
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isPanning = true;
            _lastPanPoint = e.GetPosition(this);
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_isPanning)
        {
            var currentPoint = e.GetPosition(this);
            _offsetX += currentPoint.X - _lastPanPoint.X;
            _offsetY += currentPoint.Y - _lastPanPoint.Y;
            _lastPanPoint = currentPoint;
            InvalidateVisual();
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _isPanning = false;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        var mousePos = e.GetPosition(this);
        var zoomFactor = e.Delta.Y > 0 ? 1.2 : 1 / 1.2;

        // Zoom centered on mouse position
        var oldScale = _scale;
        _scale *= zoomFactor;

        // Clamp scale
        _scale = Math.Max(1e-10, Math.Min(1e10, _scale));

        // Adjust offset to keep mouse position fixed
        _offsetX = mousePos.X - (mousePos.X - _offsetX) * (_scale / oldScale);
        _offsetY = mousePos.Y - (mousePos.Y - _offsetY) * (_scale / oldScale);

        InvalidateVisual();
        e.Handled = true;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (_currentStructure != null && e.PreviousSize == default)
        {
            FitToView();
            InvalidateVisual();
        }
    }
}
