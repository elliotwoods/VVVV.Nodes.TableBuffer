using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using VVVV.PluginInterfaces.V2;
using System.Diagnostics;

namespace VVVV.Nodes
{
	class SpreadTable : DataTable
	{
		public delegate void DataChangedHandler(Object sender, EventArgs e);
		public event DataChangedHandler DataChanged;

		public SpreadTable()
		{
			ClearAll();
			SetupEvents();
			this.TableName = "SpreadTable";
		}

		private void SetupEvents()
		{
			this.ColumnChanged += new DataColumnChangeEventHandler(OnDataChange);
			this.RowChanged += new DataRowChangeEventHandler(OnDataChange);
			this.RowDeleted += new DataRowChangeEventHandler(OnDataChange);
			this.TableCleared += new DataTableClearEventHandler(OnDataChange);
			this.TableNewRow += new DataTableNewRowEventHandler(OnDataChange);
		}

		void OnDataChange(object sender, EventArgs e)
		{
			if (DataChanged != null)
				DataChanged(sender, e);
		}

		public void OnDataChange(Object sender)
		{
			OnDataChange(sender, new EventArgs());
		}

		public void SetupColumns(string ColumnNames)
		{
			if (ColumnNames == "")
				return;

			var Columns = this.Columns;
			string[] ColumnNamesSplit = ColumnNames.Split(',');

			int index = 0;
			foreach (var name in ColumnNamesSplit)
			{

				if (index >= Columns.Count)
					this.AddColumn(name);
				else
					Columns[index].ColumnName = name;

				index++;
			}
		}

		public void ClearAll()
		{
			this.Rows.Clear();
			this.Columns.Clear();
			OnDataChange(this);
		}

		public Spread<Spread<double> > Spread
		{
			get
			{
				Spread<Spread<double>> data = new Spread<Spread<double>>(this.Rows.Count);

				int index = 0;
				foreach (DataRow row in this.Rows)
				{
					data[index] = new Spread<double>(this.Columns.Count);

					object cell;
					for (int j = 0; j < data[index].SliceCount; j++)
					{
						cell = row[j];
						if (cell.GetType() == typeof(System.Double))
							data[index][j] = (double)cell;
						else
							data[index][j] = 0.0;
					}
					index++;
				}
				return data;
			}
			set
			{
				//we preserve our own column count, but take new row count
				this.Rows.Clear();
				this.Insert( (ISpread<ISpread<double>>) value);
			}
		}

		public void Insert(ISpread<ISpread<double>> insertSpread)
		{
			foreach (var row in insertSpread)
			{
				Insert(row);
			}
		}

		public void Insert(ISpread<double> insertSpread)
		{
			//check whether slice count is too big. if soo add columns
			//should generally happen on first row only in a spread of spreads
			while (insertSpread.SliceCount > this.Columns.Count)
			{
				this.AddColumn(this.Columns.Count.ToString());
			}

			//insert the row
			var dataRow = this.Rows.Add();

			//set values
			for (int i = 0; i < insertSpread.SliceCount; i++)
			{
				dataRow[i] = insertSpread[i];
			}

			OnDataChange(null);
		}

		private DataColumn AddColumn(string name)
		{
			DataColumn newCol = Columns.Add(name, typeof(double));
			
			newCol.DefaultValue = 0.0;
			foreach (DataRow testRow in Rows)
			{
				if (testRow[newCol].GetType() == typeof(DBNull))
					testRow[newCol] = newCol.DefaultValue;
			}
			newCol.AllowDBNull = false;

			OnDataChange(null);
			return newCol;
		}
	}
}
