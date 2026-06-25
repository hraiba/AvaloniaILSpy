// Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using Avalonia.Media.Imaging;
using Avalonia;
using System.Collections.Generic;
using Avalonia.Controls.Platform.Surfaces;
using Avalonia.Platform;

namespace ICSharpCode.ILSpy;

static class Images
	{
		static IBitmap LoadBitmap(string name)
		{
			Bitmap image = new("Images/" + name + ".png");
			//image.Freeze();
			return image;
		}
		
		public static readonly IBitmap Breakpoint = LoadBitmap("Breakpoint");
		public static readonly IBitmap CurrentLine = LoadBitmap("CurrentLine");

		public static readonly IBitmap ViewCode = LoadBitmap("ViewCode");
		public static readonly IBitmap Save = LoadBitmap("SaveFile");
		public static readonly IBitmap OK = LoadBitmap("OK");

		public static readonly IBitmap Delete = LoadBitmap("Delete");
		public static readonly IBitmap Search = LoadBitmap("Search");

		public static readonly IBitmap Assembly = LoadBitmap("Assembly");
		public static readonly IBitmap AssemblyWarning = LoadBitmap("AssemblyWarning");
		public static readonly IBitmap AssemblyLoading = LoadBitmap("FindAssembly");

		public static readonly IBitmap Library = LoadBitmap("Library");
		public static readonly IBitmap Namespace = LoadBitmap("NameSpace");

		public static readonly IBitmap ReferenceFolderOpen = LoadBitmap("ReferenceFolder.Open");
		public static readonly IBitmap ReferenceFolderClosed = LoadBitmap("ReferenceFolder.Closed");

		public static readonly IBitmap SubTypes = LoadBitmap("SubTypes");
		public static readonly IBitmap SuperTypes = LoadBitmap("SuperTypes");

		public static readonly IBitmap FolderOpen = LoadBitmap("Folder.Open");
		public static readonly IBitmap FolderClosed = LoadBitmap("Folder.Closed");

		public static readonly IBitmap Resource = LoadBitmap("Resource");
		public static readonly IBitmap ResourceImage = LoadBitmap("ResourceImage");
		public static readonly IBitmap ResourceResourcesFile = LoadBitmap("ResourceResourcesFile");
		public static readonly IBitmap ResourceXml = LoadBitmap("ResourceXml");
		public static readonly IBitmap ResourceXsd = LoadBitmap("ResourceXsd");
		public static readonly IBitmap ResourceXslt = LoadBitmap("ResourceXslt");

		public static readonly IBitmap Class = LoadBitmap("Class");
		public static readonly IBitmap Struct = LoadBitmap("Struct");
		public static readonly IBitmap Interface = LoadBitmap("Interface");
		public static readonly IBitmap Delegate = LoadBitmap("Delegate");
		public static readonly IBitmap Enum = LoadBitmap("Enum");
		public static readonly IBitmap StaticClass = LoadBitmap("StaticClass");


		public static readonly IBitmap Field = LoadBitmap("Field");
		public static readonly IBitmap FieldReadOnly = LoadBitmap("FieldReadOnly");
		public static readonly IBitmap Literal = LoadBitmap("Literal");
		public static readonly IBitmap EnumValue = LoadBitmap("EnumValue");

		public static readonly IBitmap Method = LoadBitmap("Method");
		public static readonly IBitmap Constructor = LoadBitmap("Constructor");
		public static readonly IBitmap VirtualMethod = LoadBitmap("VirtualMethod");
		public static readonly IBitmap Operator = LoadBitmap("Operator");
		public static readonly IBitmap ExtensionMethod = LoadBitmap("ExtensionMethod");
		public static readonly IBitmap PInvokeMethod = LoadBitmap("PInvokeMethod");

		public static readonly IBitmap Property = LoadBitmap("Property");
		public static readonly IBitmap Indexer = LoadBitmap("Indexer");

		public static readonly IBitmap Event = LoadBitmap("Event");

		private static readonly IBitmap OverlayProtected = LoadBitmap("OverlayProtected");
		private static readonly IBitmap OverlayInternal = LoadBitmap("OverlayInternal");
		private static readonly IBitmap OverlayProtectedInternal = LoadBitmap("OverlayProtectedInternal");
		private static readonly IBitmap OverlayPrivate = LoadBitmap("OverlayPrivate");
		private static readonly IBitmap OverlayPrivateProtected = LoadBitmap("OverlayPrivateProtected");
		private static readonly IBitmap OverlayCompilerControlled = LoadBitmap("OverlayCompilerControlled");

		private static readonly IBitmap OverlayStatic = LoadBitmap("OverlayStatic");

		public static IBitmap LoadImage(object part, string icon)
		{
        IBitmap image;
        var assembly = part.GetType().Assembly;
			if (assembly == typeof(Images).Assembly) {
				image = new Bitmap(icon);
			} else {
				var name = assembly.GetName();
            var embededResourceStream = assembly.GetManifestResourceStream(icon);
            image = new Bitmap(embededResourceStream);
        }
			return image;
		}


		private static readonly TypeIconCache typeIconCache = new();
		private static readonly MemberIconCache memberIconCache = new();

		public static IBitmap GetIcon(TypeIcon icon, AccessOverlayIcon overlay, bool isStatic = false)
		{
			lock (typeIconCache)
        {
            return typeIconCache.GetIcon(icon, overlay, isStatic);
        }
    }

		public static IBitmap GetIcon(MemberIcon icon, AccessOverlayIcon overlay, bool isStatic)
		{
			lock (memberIconCache)
        {
            return memberIconCache.GetIcon(icon, overlay, isStatic);
        }
    }

		#region icon caches & overlay management

		private class TypeIconCache : IconCache<TypeIcon>
		{
			public TypeIconCache()
			{
				PreloadPublicIconToCache(TypeIcon.Class, Class);
				PreloadPublicIconToCache(TypeIcon.Enum, Enum);
				PreloadPublicIconToCache(TypeIcon.Struct, Struct);
				PreloadPublicIconToCache(TypeIcon.Interface, Interface);
				PreloadPublicIconToCache(TypeIcon.Delegate, Delegate);
				PreloadPublicIconToCache(TypeIcon.StaticClass, StaticClass);
			}

			protected override IBitmap GetBaseImage(TypeIcon icon)
			{
				IBitmap baseImage;
				switch (icon) {
					case TypeIcon.Class:
						baseImage = Class;
						break;
					case TypeIcon.Enum:
						baseImage = Enum;
						break;
					case TypeIcon.Struct:
						baseImage = Struct;
						break;
					case TypeIcon.Interface:
						baseImage = Interface;
						break;
					case TypeIcon.Delegate:
						baseImage = Delegate;
						break;
					case TypeIcon.StaticClass:
						baseImage = StaticClass;
						break;
					default:
                    throw new ArgumentOutOfRangeException(nameof(icon), $"TypeIcon.{icon} is not supported!");
            }

				return baseImage;
			}
		}

		private class MemberIconCache : IconCache<MemberIcon>
		{
			public MemberIconCache()
			{
				PreloadPublicIconToCache(MemberIcon.Field, Field);
				PreloadPublicIconToCache(MemberIcon.FieldReadOnly, FieldReadOnly);
				PreloadPublicIconToCache(MemberIcon.Literal, Literal);
				PreloadPublicIconToCache(MemberIcon.EnumValue, EnumValue);
				PreloadPublicIconToCache(MemberIcon.Property, Property);
				PreloadPublicIconToCache(MemberIcon.Indexer, Indexer);
				PreloadPublicIconToCache(MemberIcon.Method, Method);
				PreloadPublicIconToCache(MemberIcon.Constructor, Constructor);
				PreloadPublicIconToCache(MemberIcon.VirtualMethod, VirtualMethod);
				PreloadPublicIconToCache(MemberIcon.Operator, Operator);
				PreloadPublicIconToCache(MemberIcon.ExtensionMethod, ExtensionMethod);
				PreloadPublicIconToCache(MemberIcon.PInvokeMethod, PInvokeMethod);
				PreloadPublicIconToCache(MemberIcon.Event, Event);
			}

			protected override IBitmap GetBaseImage(MemberIcon icon)
			{
				IBitmap baseImage;
				switch (icon) {
					case MemberIcon.Field:
						baseImage = Field;
						break;
					case MemberIcon.FieldReadOnly:
						baseImage = FieldReadOnly;
						break;
					case MemberIcon.Literal:
						baseImage = Literal;
						break;
					case MemberIcon.EnumValue:
						baseImage = Literal;
						break;
					case MemberIcon.Property:
						baseImage = Property;
						break;
					case MemberIcon.Indexer:
						baseImage = Indexer;
						break;
					case MemberIcon.Method:
						baseImage = Method;
						break;
					case MemberIcon.Constructor:
						baseImage = Constructor;
						break;
					case MemberIcon.VirtualMethod:
						baseImage = VirtualMethod;
						break;
					case MemberIcon.Operator:
						baseImage = Operator;
						break;
					case MemberIcon.ExtensionMethod:
						baseImage = ExtensionMethod;
						break;
					case MemberIcon.PInvokeMethod:
						baseImage = PInvokeMethod;
						break;
					case MemberIcon.Event:
						baseImage = Event;
						break;
					default:
                    throw new ArgumentOutOfRangeException(nameof(icon), $"MemberIcon.{icon} is not supported!");
            }

				return baseImage;
			}
		}

    private class WbFb(WriteableBitmap bitmap) : IFramebufferPlatformSurface
    {
        WriteableBitmap _bitmap = bitmap;
        public ILockedFramebuffer Lock() => _bitmap.Lock();
    }

    private abstract class IconCache<T>
		{
			private readonly Dictionary<Tuple<T, AccessOverlayIcon, bool>, IBitmap> cache = [];

			protected void PreloadPublicIconToCache(T icon, IBitmap image)
			{
				var iconKey = new Tuple<T, AccessOverlayIcon, bool>(icon, AccessOverlayIcon.Public, false);
				cache.Add(iconKey, image);
			}

			public IBitmap GetIcon(T icon, AccessOverlayIcon overlay, bool isStatic)
			{
				var iconKey = new Tuple<T, AccessOverlayIcon, bool>(icon, overlay, isStatic);
				if (cache.ContainsKey(iconKey)) {
					return cache[iconKey];
				} else {
					IBitmap result = BuildMemberIcon(icon, overlay, isStatic);
					cache.Add(iconKey, result);
					return result;
				}
			}

			private IBitmap BuildMemberIcon(T icon, AccessOverlayIcon overlay, bool isStatic)
			{
				IBitmap baseImage = GetBaseImage(icon);
				IBitmap overlayImage = GetOverlayImage(overlay);

				return CreateOverlayImage(baseImage, overlayImage, isStatic);
			}

			protected abstract IBitmap GetBaseImage(T icon);

			private static IBitmap GetOverlayImage(AccessOverlayIcon overlay)
			{
				IBitmap overlayImage;
				switch (overlay) {
					case AccessOverlayIcon.Public:
						overlayImage = null;
						break;
					case AccessOverlayIcon.Protected:
						overlayImage = OverlayProtected;
						break;
					case AccessOverlayIcon.Internal:
						overlayImage = OverlayInternal;
						break;
					case AccessOverlayIcon.ProtectedInternal:
						overlayImage = OverlayProtectedInternal;
						break;
					case AccessOverlayIcon.Private:
						overlayImage = OverlayPrivate;
						break;
					case AccessOverlayIcon.PrivateProtected:
						overlayImage = OverlayPrivateProtected;
						break;
					case AccessOverlayIcon.CompilerControlled:
						overlayImage = OverlayCompilerControlled;
						break;
					default:
                    throw new ArgumentOutOfRangeException(nameof(overlay), $"AccessOverlayIcon.{overlay} is not supported!");
            }
				return overlayImage;
			}

			private static readonly Rect iconRect = new(0, 0, 16, 16);

        private static IBitmap CreateOverlayImage(IBitmap baseImage, IBitmap overlay, bool isStatic)
			{
            var image = new WriteableBitmap(new PixelSize(16, 16), new Vector(96, 96), PixelFormat.Rgba8888, AlphaFormat.Unpremul);

            using (var rt = AvaloniaLocator.Current.GetService<IPlatformRenderInterface>().CreateRenderTarget(new[] { new WbFb(image)})) {

                using var ctx = rt.CreateDrawingContext(null);
                ctx.DrawBitmap(baseImage.PlatformImpl, 1.0, iconRect, iconRect);

                if (overlay != null)
                {
                    ctx.DrawBitmap(overlay.PlatformImpl, 1.0, iconRect, iconRect);
                }

                if (isStatic)
                {
                    ctx.DrawBitmap(OverlayStatic.PlatformImpl, 1.0, iconRect, iconRect);
                }

            }

            // TODO: image.Freeze()
            return image;
        }
    }

		#endregion
	}
