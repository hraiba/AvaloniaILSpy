// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

namespace ICSharpCode.ILSpy.TextView;

	/// <summary>
	/// Allows language specific search for matching brackets.
	/// </summary>
	public interface IBracketSearcher
	{
		/// <summary>
		/// Searches for a matching bracket from the given offset to the start of the document.
		/// </summary>
		/// <returns>A BracketSearchResult that contains the positions and lengths of the brackets. Return null if there is nothing to highlight.</returns>
		BracketSearchResult SearchBracket(IDocument document, int offset);
	}

	public class DefaultBracketSearcher : IBracketSearcher
	{
		public static readonly DefaultBracketSearcher DefaultInstance = new();

    public BracketSearchResult SearchBracket(IDocument document, int offset) => null;
}

	/// <summary>
	/// Describes a pair of matching brackets found by <see cref="IBracketSearcher"/>.
	/// </summary>
	public class BracketSearchResult(int openingBracketOffset, int openingBracketLength,
                                   int closingBracketOffset, int closingBracketLength)
{
    public int OpeningBracketOffset { get; } = openingBracketOffset;

    public int OpeningBracketLength { get; private set; } = openingBracketLength;

    public int ClosingBracketOffset { get; } = closingBracketOffset;

    public int ClosingBracketLength { get; private set; } = closingBracketLength;
}

	public class BracketHighlightRenderer : IBackgroundRenderer
	{
		BracketSearchResult result;
		IPen borderPen;
		IBrush backgroundBrush;
    global::AvaloniaEdit.Rendering.TextView textView;

		public void SetHighlight(BracketSearchResult result)
		{
			if (this.result != result) {
				this.result = result;
				textView.InvalidateLayer(Layer);
			}
		}

		public BracketHighlightRenderer(global::AvaloniaEdit.Rendering.TextView textView)
		{
        ArgumentNullException.ThrowIfNull(textView);

        borderPen = new Pen(new SolidColorBrush(Color.FromArgb(52, 0, 0, 255)), 1).ToImmutable();

			backgroundBrush = new SolidColorBrush(Color.FromArgb(22, 0, 0, 255)).ToImmutable();

			this.textView = textView;

			this.textView.BackgroundRenderers.Add(this);
		}

    public KnownLayer Layer => KnownLayer.Selection;

    public void Draw(global::AvaloniaEdit.Rendering.TextView textView, DrawingContext drawingContext)
		{
			if (result == null)
        {
            return;
        }

        BackgroundGeometryBuilder builder = new();

			builder.CornerRadius = 1;

			builder.AddSegment(textView, new TextSegment() { StartOffset = result.OpeningBracketOffset, Length = result.OpeningBracketLength });
			builder.CloseFigure(); // prevent connecting the two segments
			builder.AddSegment(textView, new TextSegment() { StartOffset = result.ClosingBracketOffset, Length = result.ClosingBracketLength });

			Geometry geometry = builder.CreateGeometry();
			if (geometry != null) {
				drawingContext.DrawGeometry(backgroundBrush, borderPen, geometry);
			}
		}
	}
