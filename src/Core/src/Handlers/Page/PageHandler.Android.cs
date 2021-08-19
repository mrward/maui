﻿using System;
using Android.Views;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : ViewHandler<IView, PageViewGroup>
	{
		//Graphics.Color? DefaultBackgroundColor;

		protected override PageViewGroup CreateNativeView()
		{
			if (VirtualView == null)
			{
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a PageViewGroup");
			}

			var viewGroup = new PageViewGroup(Context)
			{
				CrossPlatformMeasure = VirtualView.Measure,
				CrossPlatformArrange = VirtualView.Arrange
			};

			return viewGroup;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");

			NativeView.CrossPlatformMeasure = VirtualView.Measure;
			NativeView.CrossPlatformArrange = VirtualView.Arrange;
		}

		void UpdateContent()
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			NativeView.RemoveAllViews();

			if (VirtualView is IContentView cv && cv.Content is IView view)
				NativeView.AddView(view.ToNative(MauiContext));
		}

		public static void MapTitle(PageHandler handler, IView page)
		{
		}

		public static void MapContent(PageHandler handler, IView page)
		{
			handler.UpdateContent();
		}

		protected override void DisconnectHandler(PageViewGroup nativeView)
		{
			// If we're being disconnected from the xplat element, then we should no longer be managing its chidren
			nativeView.RemoveAllViews();
			base.DisconnectHandler(nativeView);
		}
	}
}
