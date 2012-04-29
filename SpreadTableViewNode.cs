#region usings
using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
using System.ComponentModel;
#endregion usings

namespace VVVV.Nodes.TableBuffer
{
	#region PluginInfo
	[PluginInfo(Name = "TableView", Category = "SpreadTable", Help = "Use a .NET DataGridView to view a SpreadTable", Tags = "", AutoEvaluate = true)]
	#endregion PluginInfo
	public class SpreadTableViewNode : UserControl, IPluginEvaluate
	{
		#region fields & pins
		[Input("Table", IsSingle=true)]
		IDiffSpread<SpreadTable> FPinInTable;

		[Input("Up", IsSingle = true, IsBang = true)]
		ISpread<bool> FUp;

		[Input("Down", IsSingle = true, IsBang = true)]
		ISpread<bool> FDown;

		[Output("Index")]
		ISpread<int> FCurrentIndex;

		[Import()]
		ILogger FLogger;

		DataGridView FDataGridView;
		SpreadTable FData;
		bool FNeedsUpdate = false;
		#endregion fields & pins

		#region constructor and init

		public SpreadTableViewNode()
		{
			//setup the gui
			InitializeComponent();
		}

		void InitializeComponent()
		{
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			this.FDataGridView = new System.Windows.Forms.DataGridView();
			((System.ComponentModel.ISupportInitialize)(this.FDataGridView)).BeginInit();
			this.SuspendLayout();
			// 
			// FDataGridView
			// 
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			dataGridViewCellStyle1.Format = "N4";
			dataGridViewCellStyle1.NullValue = "0.0000";
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.Gray;
			this.FDataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
			this.FDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.FDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.FDataGridView.Cursor = System.Windows.Forms.Cursors.Default;
			this.FDataGridView.Location = new System.Drawing.Point(0, 0);
			this.FDataGridView.Name = "FDataGridView";
			dataGridViewCellStyle2.Format = "N4";
			dataGridViewCellStyle2.NullValue = "0.0000";
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.Gray;
			this.FDataGridView.RowsDefaultCellStyle = dataGridViewCellStyle2;
			this.FDataGridView.Size = this.Size;
			this.FDataGridView.TabIndex = 0;
			this.FDataGridView.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dataGridView1_CellValidating);
			this.FDataGridView.MouseMove += new System.Windows.Forms.MouseEventHandler(FDataGridView_MouseMove);
			// 
			// ValueTableBufferNode
			// 
			this.Controls.Add(this.FDataGridView);
			this.Name = "ValueTableBufferNode";
			this.Size = new System.Drawing.Size(344, 368);
			this.Resize += new System.EventHandler(this.ValueTableBufferNode_Resize);
			((System.ComponentModel.ISupportInitialize)(this.FDataGridView)).EndInit();
			this.ResumeLayout(false);

		}

		Point FMouseLast;
		bool FMouseDragging = false;
		void FDataGridView_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button.HasFlag(System.Windows.Forms.MouseButtons.Right))
			{
				if (FMouseDragging)
				{
					double delta = - 0.01 * (double)(e.Y - FMouseLast.Y);
					foreach (DataGridViewCell cell in FDataGridView.SelectedCells)
						if (cell.Value.GetType() == typeof(System.Double) && cell.RowIndex < FData.Rows.Count) //avoids selection of the 'new row' at bottom or invalid cells
							cell.Value = (double)cell.Value + delta;
					FData.OnDataChange(this);
				}
				else
				{
					FMouseDragging = true;
				}
				FMouseLast = e.Location;
			}
			else
				FMouseDragging = false;
		}

		#endregion constructor and init

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			bool updateOutput = false;

			if (FPinInTable[0] != FData)
			{
				FData = FPinInTable[0];
				if (FData != null)
				{
					FData.DataChanged += new SpreadTable.DataChangedHandler(FData_DataChanged);
				}
				FDataGridView.DataSource = FData;

				updateOutput = true;
			}

			if (FData == null)
				return;

			if (FData.Rows.Count > 0)
			{
				bool moveRow = false;
				int selectedRow = 0;

				if (FDataGridView.SelectedCells.Count > 0)
					selectedRow = FDataGridView.SelectedCells[0].RowIndex;
				else
					selectedRow = 0;

				if (FUp[0])
				{
					selectedRow++;
					selectedRow %= FData.Rows.Count;
					moveRow = true;
				}

				if (FDown[0])
				{
					selectedRow--;
					if (selectedRow < 0)
						selectedRow += FData.Rows.Count;
					moveRow = true;
				}

				if (moveRow)
				{
					FDataGridView.ClearSelection();
					FDataGridView.Rows[selectedRow].Selected = true;
				}
			}

			if (FNeedsUpdate)
			{
				FDataGridView.Refresh();

				updateOutput = true;
				FNeedsUpdate = false;
			}

			if (FData.Rows.Count == 0)
			{
				FCurrentIndex.SliceCount = 1;
				FCurrentIndex[0] = 0;
			}
			else
			{
				var rows = FDataGridView.SelectedRows;
				if (rows.Count > 0)
				{
					FCurrentIndex.SliceCount = 0;
					foreach (DataGridViewRow row in rows)
					{
						FCurrentIndex.Add(row.Index);
					}
				}
				else
				{
					int row = FDataGridView.CurrentCellAddress.Y;
					FCurrentIndex.SliceCount = 1;
					FCurrentIndex[0] = row;
				}
			}
		}

		void FData_DataChanged(Object sender, EventArgs e)
		{
			//pretty hacky. this clears the 'udpate' flag if the last instruction is from itself
			FNeedsUpdate = true;// (sender != this);
		}

		private void ValueTableBufferNode_Resize(object sender, EventArgs e)
		{
			this.FDataGridView.Size = this.Size;
		}

		private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			try
			{
				System.Convert.ToDouble(e.FormattedValue);
			}
			catch
			{
				e.Cancel = true;
			}			
			ReformatTable();
		}

		private void ReformatTable()
		{
		}

	}
}
