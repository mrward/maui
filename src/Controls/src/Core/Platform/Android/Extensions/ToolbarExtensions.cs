﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Microsoft.Maui.Graphics;
using ATextView = global::Android.Widget.TextView;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;
using AView = global::Android.Views.View;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class ToolbarExtensions
	{
		const int DefaultDisabledToolbarAlpha = 127;
		public static void DisposeMenuItems(this AToolbar toolbar, IEnumerable<ToolbarItem> toolbarItems, PropertyChangedEventHandler toolbarItemChanged)
		{
			if (toolbarItems == null)
				return;

			foreach (var item in toolbarItems)
				item.PropertyChanged -= toolbarItemChanged;
		}

		public static void UpdateMenuItems(this AToolbar toolbar,
			IEnumerable<ToolbarItem> sortedToolbarItems,
			IMauiContext mauiContext,
			Color tintColor,
			PropertyChangedEventHandler toolbarItemChanged,
			List<IMenuItem> menuItemsCreated,
			List<ToolbarItem> toolbarItemsCreated,
			Action<Context, IMenuItem, ToolbarItem> updateMenuItemIcon = null)
		{
			if (sortedToolbarItems == null || menuItemsCreated == null)
				return;

			var context = mauiContext.Context;
			var menu = toolbar.Menu;
			menu.Clear();

			foreach (var menuItem in menuItemsCreated)
				menuItem.Dispose();

			foreach (var toolbarItem in toolbarItemsCreated)
				toolbarItem.PropertyChanged -= toolbarItemChanged;

			menuItemsCreated.Clear();
			toolbarItemsCreated.Clear();

			foreach (var item in sortedToolbarItems)
			{
				UpdateMenuItem(toolbar, item, null, mauiContext, tintColor, toolbarItemChanged, menuItemsCreated, toolbarItemsCreated, updateMenuItemIcon);
			}
		}

		internal static void UpdateMenuItem(AToolbar toolbar,
			ToolbarItem item,
			int? menuItemIndex,
			IMauiContext mauiContext,
			Color tintColor,
			PropertyChangedEventHandler toolbarItemChanged,
			List<IMenuItem> menuItemsCreated,
			List<ToolbarItem> toolbarItemsCreated,
			Action<Context, IMenuItem, ToolbarItem> updateMenuItemIcon = null)
		{
			var context = mauiContext.Context;
			IMenu menu = toolbar.Menu;
			item.PropertyChanged -= toolbarItemChanged;
			item.PropertyChanged += toolbarItemChanged;

			IMenuItem menuitem;

			Java.Lang.ICharSequence newTitle = null;

			if (!String.IsNullOrWhiteSpace(item.Text))
			{
				if (item.Order != ToolbarItemOrder.Secondary && tintColor != null && tintColor != null)
				{
					var color = item.IsEnabled ? tintColor.ToNative() : tintColor.MultiplyAlpha(0.302f).ToNative();
					SpannableString titleTinted = new SpannableString(item.Text);
					titleTinted.SetSpan(new ForegroundColorSpan(color), 0, titleTinted.Length(), 0);
					newTitle = titleTinted;
				}
				else
				{
					newTitle = new Java.Lang.String(item.Text);
				}
			}
			else
			{
				newTitle = new Java.Lang.String();
			}

			if (menuItemIndex == null)
			{
				menuitem = menu.Add(0, AView.GenerateViewId(), 0, newTitle);
				menuItemsCreated?.Add(menuitem);
				toolbarItemsCreated?.Add(item);
			}
			else
			{
				if (menuItemsCreated == null || menuItemsCreated.Count < menuItemIndex.Value)
					return;

				menuitem = menuItemsCreated[menuItemIndex.Value];

				if (!menuitem.IsAlive())
					return;

				menuitem.SetTitle(newTitle);
			}

			menuitem.SetEnabled(item.IsEnabled);
			menuitem.SetTitleOrContentDescription(item);

			if (updateMenuItemIcon != null)
				updateMenuItemIcon(context, menuitem, item);
			else
				UpdateMenuItemIcon(mauiContext, menuitem, item, tintColor);

			if (item.Order != ToolbarItemOrder.Secondary)
				menuitem.SetShowAsAction(ShowAsAction.Always);

			menuitem.SetOnMenuItemClickListener(new GenericMenuClickListener(((IMenuItemController)item).Activate));

			if (item.Order != ToolbarItemOrder.Secondary && !NativeVersion.IsAtLeast(26) && (tintColor != null && tintColor != null))
			{
				var view = toolbar.FindViewById(menuitem.ItemId);
				if (view is ATextView textView)
				{
					if (item.IsEnabled)
						textView.SetTextColor(tintColor.ToNative());
					else
						textView.SetTextColor(tintColor.MultiplyAlpha(0.302f).ToNative());
				}
			}
		}

		internal static void UpdateMenuItemIcon(IMauiContext mauiContext, IMenuItem menuItem, ToolbarItem toolBarItem, Color tintColor)
		{
			ImageSourceLoader.LoadImage(toolBarItem, mauiContext, result =>
			{
				var baseDrawable = result.Value;
				if (menuItem == null || !menuItem.IsAlive())
				{
					return;
				}

				if (baseDrawable != null)
				{
					using (var constant = baseDrawable.GetConstantState())
					using (var newDrawable = constant.NewDrawable())
					using (var iconDrawable = newDrawable.Mutate())
					{
						if (tintColor != null)
							iconDrawable.SetColorFilter(tintColor.ToNative(Colors.White), FilterMode.SrcAtop);

						if (!menuItem.IsEnabled)
						{
							iconDrawable.Mutate().SetAlpha(DefaultDisabledToolbarAlpha);
						}

						menuItem.SetIcon(iconDrawable);
					}
				}
			});
		}

		public static void OnToolbarItemPropertyChanged(
			this AToolbar toolbar,
			PropertyChangedEventArgs e,
			ToolbarItem toolbarItem,
			ICollection<ToolbarItem> toolbarItems,
			IMauiContext mauiContext,
			Color tintColor,
			PropertyChangedEventHandler toolbarItemChanged,
			List<IMenuItem> currentMenuItems,
			List<ToolbarItem> currentToolbarItems,
			Action<Context, IMenuItem, ToolbarItem> updateMenuItemIcon = null)
		{
			if (toolbarItems == null)
				return;

			if (!e.IsOneOf(MenuItem.TextProperty, MenuItem.IconImageSourceProperty, MenuItem.IsEnabledProperty))
				return;
			var context = mauiContext.Context;
			int index = 0;

			foreach (var item in toolbarItems)
			{
				if (item == toolbarItem)
				{
					break;
				}

				index++;
			}

			if (index >= currentMenuItems.Count)
				return;

			if (currentMenuItems[index].IsAlive())
				UpdateMenuItem(toolbar, toolbarItem, index, mauiContext, tintColor, toolbarItemChanged, currentMenuItems, currentToolbarItems, updateMenuItemIcon);
			else
				UpdateMenuItems(toolbar, toolbarItems, mauiContext, tintColor, toolbarItemChanged, currentMenuItems, currentToolbarItems, updateMenuItemIcon);
		}
	}
}
