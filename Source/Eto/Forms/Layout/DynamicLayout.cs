using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using System.Windows.Markup;

namespace Eto.Forms
{
	[ContentProperty("Rows")]
	public class DynamicLayout : Layout
	{
		DynamicTable topTable;
		DynamicTable currentItem;
		bool? yscale;

		public List<DynamicRow> Rows
		{
			get { return topTable.Rows; }
		}

		public bool Generated { get; private set; }

		public Padding? Padding
		{
			get { return topTable.Padding; }
			set { topTable.Padding = value; }
		}

		public Size? Spacing
		{
			get { return topTable.Spacing; }
			set { topTable.Spacing = value; }
		}
		
		public Padding? DefaultPadding { get; set; }
		
		public Size? DefaultSpacing { get; set; }
		
		public override Container Container
		{
			get { return base.Container; }
			protected internal set {

				if (base.Container != null) {
					base.Container.PreLoad -= HandleContainerLoad;
					base.Container.Load -= HandleContainerLoad;
				}

				base.Container = value;
				if (topTable != null)
					topTable.Container = value;
				if (base.Container != null) {
					base.Container.PreLoad += HandleContainerLoad;
					base.Container.Load += HandleContainerLoad;
				}
			}
		}

		public override Layout InnerLayout
		{
			get { return topTable.Layout; }
		}

		public override IEnumerable<Control> Controls
		{
			get {
				if (topTable.Layout != null) return topTable.Layout.Controls;
				return Enumerable.Empty<Control> ();
			}
		}

		#region Exceptions
		
		[Serializable]
		public class AlreadyGeneratedException : Exception
		{
			public AlreadyGeneratedException ()
			{
			}
			
			public AlreadyGeneratedException (string message) : base (message)
			{
			}
			
			public AlreadyGeneratedException (string message, Exception inner) : base (message, inner)
			{
			}
			
			protected AlreadyGeneratedException (System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base (info, context)
			{
			}
		}
		
		#endregion
		
		class InternalHandler : WidgetHandler<object, Layout>, ILayout 
		{
			public void OnPreLoad ()
			{
			}

			public void OnLoad ()
			{
			}

			public void OnLoadComplete ()
			{
			}

			public void Update ()
			{
			}
		}

		public override void OnPreLoad (EventArgs e)
		{
			base.OnPreLoad (e);
			if (InnerLayout != null)
				InnerLayout.OnPreLoad (e);
		}

		public override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);
			if (InnerLayout != null)
				InnerLayout.OnLoad (e);
		}

		public override void OnLoadComplete (EventArgs e)
		{
			base.OnLoadComplete (e);
			if (InnerLayout != null)
				InnerLayout.OnLoadComplete (e);
		}

		public override void Update ()
		{
			base.Update ();
			if (InnerLayout != null)
				InnerLayout.Update ();
		}

		public DynamicLayout ()
			: this(null, null, null)
		{
		}

		public DynamicLayout (Padding? padding, Size? spacing = null)
			: this(null, padding, spacing)
		{
		}
		
		public DynamicLayout (Container container, Padding? padding = null, Size? spacing = null)
			: base (container != null ? container.Generator : Generator.Current, container, new InternalHandler (), false)
		{
			topTable = new DynamicTable { 
				Padding = padding, 
				Spacing = spacing
			};
			this.Container = container;
			currentItem = topTable;
			Initialize ();
			if (this.Container != null)
				this.Container.Layout = this;
		}

		void HandleContainerLoad (object sender, EventArgs e)
		{
			if (!Generated)
				this.Generate ();
		}

		public void BeginVertical (bool xscale, bool? yscale = null)
		{
			BeginVertical (null, null, xscale, yscale);
		}
		
		public void BeginVertical (Size spacing, bool? xscale = null, bool? yscale = null)
		{
			BeginVertical (null, spacing, xscale, yscale);
		}
		
		public void BeginVertical (Padding? padding = null, Size? spacing = null, bool? xscale = null, bool? yscale = null)
		{
			if (Generated)
				throw new AlreadyGeneratedException ();
			var newItem = new DynamicTable { 
				Parent = currentItem ?? topTable, 
				Padding = padding, 
				Spacing = spacing,
				XScale = xscale,
				YScale = yscale
			};
			currentItem.Add (newItem);
			currentItem = newItem;
		}
		
		public void EndVertical ()
		{
			if (Generated)
				throw new AlreadyGeneratedException ();
			currentItem = currentItem.Parent ?? topTable;
		}
		
		public void EndBeginHorizontal (bool? yscale = null)
		{
			EndHorizontal ();
			BeginHorizontal (yscale);
		}
		
		public void BeginHorizontal (bool? yscale = null)
		{
			if (Generated)
				throw new AlreadyGeneratedException ();
			currentItem.AddRow (currentItem.CurrentRow = new DynamicRow ());
			this.yscale = yscale;
		}
		
		public void EndHorizontal ()
		{
			if (Generated)
				throw new AlreadyGeneratedException ();
			if (currentItem.CurrentRow == null)
				EndVertical ();
			else 
				currentItem.CurrentRow = null;
		}

		public void Add (Control control, bool? xscale = null, bool? yscale = null)
		{
			if (Generated)
				throw new AlreadyGeneratedException ();
			currentItem.Add (new DynamicControl{ Control = control, XScale = xscale, YScale = yscale ?? this.yscale});
		}
		
		public void AddRow (params Control[] controls)
		{
			if (Generated)
				throw new AlreadyGeneratedException ();
			if (controls == null) controls = new Control[] { null };
			var items = controls.Select (r => new DynamicControl { Control = r, YScale = yscale, XScale = r != null ? null : (bool?)true });
			var row = new DynamicRow (items.Cast<DynamicItem>());
			currentItem.AddRow (row);
			currentItem.CurrentRow = null;
		}

		[Obsolete("Please use one of the other overloads to AddCentered, the parameters of this method conflict with the other overrides")]
		public void AddCentered (Control control, bool xscale, bool? yscale = null)
		{
			AddCentered (control, true, true, null, null, xscale, yscale);
		}

		public void AddCentered (Control control, Size spacing, bool? xscale = null, bool? yscale = null)
		{
			AddCentered (control, true, true, null, spacing, xscale, yscale);
		}
		
		public void AddCentered (Control control, Padding padding, Size? spacing = null, bool? xscale = null, bool? yscale = null)
		{
			AddCentered (control, true, true, padding, spacing, xscale, yscale);
		}

		public void AddCentered (Control control, bool horizontalCenter = true, bool verticalCenter = true, Padding? padding = null, Size? spacing = null, bool? xscale = null, bool? yscale = null)
		{
			this.BeginVertical (padding, spacing, xscale, yscale);
			if (verticalCenter)
				this.Add (null, null, true);
			
			this.BeginHorizontal ();
			if (horizontalCenter)
				this.Add (null, true, null);
			
			this.Add (control);
			
			if (horizontalCenter)
				this.Add (null, true, null);
			
			this.EndHorizontal ();
			
			if (verticalCenter)
				this.Add (null, null, true);
			this.EndVertical ();
			
		}
		
		public void AddColumn (params Control[] controls)
		{
			BeginVertical ();
			foreach (var control in controls)
				Add (control);
			EndVertical ();
		}
		
		/// <summary>
		/// Generates the layout for the container
		/// </summary>
		/// <remarks>
		/// This is called automatically on the Container's LoadCompleted event, but can be called manually if needed.
		/// </remarks>
		/// <exception cref="AlreadyGeneratedException">specifies that the control was already generated</exception>
		public void Generate ()
		{
			if (Generated)
				throw new AlreadyGeneratedException ();
			topTable.Generate (this);
			Generated = true;
		}
	}
}