using System.Buffers;
using GdsSharp.Lib.Library.Builder.Payload;
using GdsSharp.Lib.Library.VertexStore;
using GdsSharp.Lib.Reading;
using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Library.Editing;

internal static class GdsReconciler
{
    /// <summary>
    /// Rebuilds a library by applying the edits in the given session to the base library.
    /// </summary>
    /// <param name="baseLib">Base library to apply edits to.</param>
    /// <param name="session">Edit session containing the edits.</param>
    /// <param name="vertexStoreFactory">Factory to create the vertex store for the new library. If null, a ChunkedVertexStore will be used.</param>
    /// <returns>New GdsLibrary with the edits applied.</returns>
    public static GdsLibrary Rebuild(GdsLibrary baseLib, GdsEditSession session, Func<IGdsVertexStore>? vertexStoreFactory)
    {
        var newStore = vertexStoreFactory?.Invoke() ?? new ChunkedVertexStore();
        var builder = new GdsLibraryBuilder(newStore) { Info = baseLib.Info };

        for (var si = 0; si < baseLib.Structures.Length; si++)
        {
            var structure = baseLib.Structures[si];
            builder.AddStructure(structure.Info);

            if (session.StructureDeltas.TryGetValue(si, out var sd) && sd.Elements is { } seq)
            {
                foreach (var ek in seq)
                    EmitElement(builder, baseLib, session, ek);
            }
            else
            {
                var start = structure.ElementStartIndex;
                var end = start + structure.ElementCount;
                for (var ei = start; ei < end; ei++)
                    EmitElement(builder, baseLib, session, ElementKey.Base(ei));
            }
        }

        return builder.Build();
    }

    private static void EmitElement(
        GdsLibraryBuilder builder,
        GdsLibrary baseLib,
        GdsEditSession session,
        ElementKey element)
    {
        if (element.IsBase)
        {
            var er = baseLib.Elements[element.Id];
            var kind = er.Kind;
            var common = er.Common;

            session.ElementDeltas.TryGetValue(element.Id, out var ed);

            var newElementId = kind switch
            {
                GdsElementKind.Boundary => EmitBoundary(builder, baseLib, session, common, er.Index, ed),
                GdsElementKind.Path => EmitPath(builder, baseLib, session, common, er.Index, ed),
                GdsElementKind.Box => EmitBox(builder, baseLib, session, common, er.Index, ed),
                GdsElementKind.Node => EmitNode(builder, baseLib, session, common, er.Index, ed),
                GdsElementKind.Text => EmitText(builder, baseLib, common, er.Index, ed),
                GdsElementKind.StructureReference => EmitStructureReference(builder, baseLib, common, er.Index, ed),
                GdsElementKind.ArrayReference => EmitArrayReference(builder, baseLib, common, er.Index, ed),
                _ => throw new NotSupportedException($"Unknown element kind {kind}")
            };

            EmitProperties(builder, baseLib, element.Id, newElementId, ed?.PropertiesOverride);
        }
        else
        {
            var ne = session.NewElements[element.Id];
            var kind = ne.Kind;
            var common = ne.Common;

            var newElementId = kind switch
            {
                GdsElementKind.Boundary => EmitBoundary(builder, session, common, (BoundaryPayload)ne.Payload),
                GdsElementKind.Path => EmitPath(builder, session, common, (PathPayload)ne.Payload),
                GdsElementKind.Box => EmitBox(builder, session, common, (BoxPayload)ne.Payload),
                GdsElementKind.Node => EmitNode(builder, session, common, (NodePayload)ne.Payload),
                GdsElementKind.Text => EmitText(builder, common, (TextPayload)ne.Payload),
                GdsElementKind.StructureReference => EmitStructureReference(builder, common, (SRefPayload)ne.Payload),
                GdsElementKind.ArrayReference => EmitArrayReference(builder, common, (ARefPayload)ne.Payload),
                _ => throw new NotSupportedException($"Unknown element kind {kind}")
            };

            EmitProperties(builder, ne.Properties, newElementId);
        }
    }

    private static int EmitBoundary(GdsLibraryBuilder builder, GdsLibrary baseLib, GdsEditSession session, GdsElementCommon? common, int payloadIndex, ElementDelta? ed)
    {
        var basePayload = baseLib.Boundaries[payloadIndex];
        var ov = ed?.PayloadOverride as BoundaryOverride?;

        var layer = ov?.Layer ?? basePayload.Layer;
        var dt = ov?.DataType ?? basePayload.DataType;
        var vref = ov?.Vertices ?? new VertexRef(VertexStoreKind.Base, basePayload.VertexOffset, basePayload.VertexCount);

        return AddWithVertices(baseLib, session, vref, pts => builder.AddBoundary(common, layer, dt, pts));
    }

    private static int EmitPath(GdsLibraryBuilder builder, GdsLibrary baseLib, GdsEditSession session, GdsElementCommon? common, int payloadIndex, ElementDelta? ed)
    {
        var basePayload = baseLib.Paths[payloadIndex];
        var ov = ed?.PayloadOverride as PathOverride?;

        var layer = ov?.Layer ?? basePayload.Layer;
        var dt = ov?.DataType ?? basePayload.DataType;
        var width = ov?.Width ?? basePayload.Width;
        var pt = ov?.PathType ?? basePayload.PathType;
        var vref = ov?.Vertices ?? new VertexRef(VertexStoreKind.Base, basePayload.VertexOffset, basePayload.VertexCount);

        return AddWithVertices(baseLib, session, vref, pts => builder.AddPath(common, layer, dt, pts, width, pt));
    }

    private static int EmitBox(GdsLibraryBuilder builder, GdsLibrary baseLib, GdsEditSession session, GdsElementCommon? common, int payloadIndex, ElementDelta? ed)
    {
        var basePayload = baseLib.Boxes[payloadIndex];
        var ov = ed?.PayloadOverride as BoxOverride?;

        var layer = ov?.Layer ?? basePayload.Layer;
        var bt = ov?.BoxType ?? basePayload.BoxType;
        var vref = ov?.Vertices ?? new VertexRef(VertexStoreKind.Base, basePayload.VertexOffset, GdsGlobals.BoxPointCount);

        return AddWithVertices(baseLib, session, vref, pts => builder.AddBox(common, layer, bt, pts));
    }

    private static int EmitNode(GdsLibraryBuilder builder, GdsLibrary baseLib, GdsEditSession session, GdsElementCommon? common, int payloadIndex, ElementDelta? ed)
    {
        var basePayload = baseLib.Nodes[payloadIndex];
        var ov = ed?.PayloadOverride as NodeOverride?;

        var layer = ov?.Layer ?? basePayload.Layer;
        var nt = ov?.NodeType ?? basePayload.NodeType;
        var vref = ov?.Vertices ?? new VertexRef(VertexStoreKind.Base, basePayload.VertexOffset, basePayload.VertexCount);

        return AddWithVertices(baseLib, session, vref, pts => builder.AddNode(common, layer, nt, pts));
    }

    private static int EmitText(GdsLibraryBuilder builder, GdsLibrary baseLib, GdsElementCommon? common, int payloadIndex, ElementDelta? ed)
    {
        var basePayload = baseLib.Texts[payloadIndex];
        var ov = ed?.PayloadOverride as TextOverride?;

        var eff = basePayload;
        if (ov.HasValue)
        {
            var o = ov.Value;
            eff = new TextPayload(
                o.Layer ?? basePayload.Layer,
                o.TextType ?? basePayload.TextType,
                o.Presentation ?? basePayload.Presentation,
                o.PathType ?? basePayload.PathType,
                o.Width ?? basePayload.Width,
                o.Strans ?? basePayload.Strans,
                o.Origin ?? basePayload.Origin,
                o.Text ?? basePayload.Text
            );
        }

        return builder.AddText(common, eff.Layer, eff.TextType, eff.Presentation, eff.PathType, eff.Width, eff.Strans, eff.Origin, eff.Text);
    }

    private static int EmitStructureReference(GdsLibraryBuilder builder, GdsLibrary baseLib, GdsElementCommon? common, int payloadIndex, ElementDelta? ed)
    {
        var basePayload = baseLib.StructureReferences[payloadIndex];
        var ov = ed?.PayloadOverride as SRefOverride?;
        var eff = basePayload;
        if (ov.HasValue)
        {
            var o = ov.Value;
            eff = basePayload with
            {
                TargetName = o.TargetName ?? basePayload.TargetName,
                Strans = o.Strans ?? basePayload.Strans,
                Origin = o.Origin ?? basePayload.Origin
            };
        }

        return builder.AddStructureReference(common, eff.TargetName, eff.Strans, eff.Origin);
    }

    private static int EmitArrayReference(GdsLibraryBuilder builder, GdsLibrary baseLib, GdsElementCommon? common, int payloadIndex, ElementDelta? ed)
    {
        var basePayload = baseLib.ArrayReferences[payloadIndex];
        var ov = ed?.PayloadOverride as ARefOverride?;
        var eff = basePayload;
        if (ov.HasValue)
        {
            var o = ov.Value;
            eff = basePayload with
            {
                TargetName = o.TargetName ?? basePayload.TargetName,
                Strans = o.Strans ?? basePayload.Strans,
                Rows = o.Rows ?? basePayload.Rows,
                Columns = o.Columns ?? basePayload.Columns,
                RowVector = o.RowVector ?? basePayload.RowVector,
                ColumnVector = o.ColumnVector ?? basePayload.ColumnVector,
                Origin = o.Origin ?? basePayload.Origin
            };
        }

        return builder.AddArrayReference(common, eff.TargetName, eff.Strans, eff.Rows, eff.Columns, eff.RowVector, eff.ColumnVector, eff.Origin);
    }

    private static int EmitBoundary(GdsLibraryBuilder builder, GdsEditSession session, GdsElementCommon? common, BoundaryPayload payload)
    {
        var vref = new VertexRef(VertexStoreKind.Delta, payload.VertexOffset, payload.VertexCount);
        return AddWithVertices(null, session, vref, pts => builder.AddBoundary(common, payload.Layer, payload.DataType, pts));
    }

    private static int EmitPath(GdsLibraryBuilder builder, GdsEditSession session, GdsElementCommon? common, PathPayload payload)
    {
        var vref = new VertexRef(VertexStoreKind.Delta, payload.VertexOffset, payload.VertexCount);
        return AddWithVertices(null, session, vref, pts => builder.AddPath(common, payload.Layer, payload.DataType, pts, payload.Width, payload.PathType));
    }

    private static int EmitBox(GdsLibraryBuilder builder, GdsEditSession session, GdsElementCommon? common, BoxPayload payload)
    {
        var vref = new VertexRef(VertexStoreKind.Delta, payload.VertexOffset, GdsGlobals.BoxPointCount);
        return AddWithVertices(null, session, vref, pts => builder.AddBox(common, payload.Layer, payload.BoxType, pts));
    }

    private static int EmitNode(GdsLibraryBuilder builder, GdsEditSession session, GdsElementCommon? common, NodePayload payload)
    {
        var vref = new VertexRef(VertexStoreKind.Delta, payload.VertexOffset, payload.VertexCount);
        return AddWithVertices(null, session, vref, pts => builder.AddNode(common, payload.Layer, payload.NodeType, pts));
    }

    private static int EmitText(GdsLibraryBuilder builder, GdsElementCommon? common, TextPayload payload)
    {
        return builder.AddText(common, payload.Layer, payload.TextType, payload.Presentation, payload.PathType, payload.Width, payload.Strans, payload.Origin, payload.Text);
    }

    private static int EmitStructureReference(GdsLibraryBuilder builder, GdsElementCommon? common, SRefPayload payload)
    {
        return builder.AddStructureReference(common, payload.TargetName, payload.Strans, payload.Origin);
    }

    private static int EmitArrayReference(GdsLibraryBuilder builder, GdsElementCommon? common, ARefPayload payload)
    {
        return builder.AddArrayReference(common, payload.TargetName, payload.Strans, payload.Rows, payload.Columns, payload.RowVector, payload.ColumnVector, payload.Origin);
    }

    private static void EmitProperties(GdsLibraryBuilder builder, GdsLibrary original, int oldBaseElementId, int newElementId,
        List<(short Attr, string Value)>? overrideProps)
    {
        if (overrideProps is { Count: > 0 })
        {
            foreach (var (a, v) in overrideProps)
                builder.AddElementProperty(newElementId, a, v);
            return;
        }

        if (!original.TryGetElementProperties(oldBaseElementId, out var props)) return;
        foreach (var p in props)
            builder.AddElementProperty(newElementId, p.Attribute, p.Value);
    }

    private static void EmitProperties(GdsLibraryBuilder builder, List<(short Attr, string Value)>? props, int newElementId)
    {
        if (props is null) return;
        foreach (var (a, v) in props)
            builder.AddElementProperty(newElementId, a, v);
    }

    private static int AddWithVertices(GdsLibrary? baseLib,
        GdsEditSession session,
        VertexRef vref,
        AddWithPoints add)
    {
        var count = vref.Count;
        if (count <= 0) return add(ReadOnlySpan<GdsPoint>.Empty);

        GdsPoint[]? rented = null;
        var pts = count <= 256
            ? stackalloc GdsPoint[count]
            : (rented = ArrayPool<GdsPoint>.Shared.Rent(count)).AsSpan(0, count);

        ReadVertices(baseLib, session, vref, pts);
        var id = add(pts);

        if (rented != null) ArrayPool<GdsPoint>.Shared.Return(rented);
        return id;
    }

    private static void ReadVertices(GdsLibrary? baseLib, GdsEditSession session, VertexRef vref, Span<GdsPoint> dst)
    {
        var store = vref.Store == VertexStoreKind.Base
            ? baseLib?.VertexStore ?? throw new InvalidOperationException("Base vertex store missing")
            : session.DeltaVertices;
        store.Read(vref.Offset, dst);
    }

    private delegate int AddWithPoints(ReadOnlySpan<GdsPoint> points);
}