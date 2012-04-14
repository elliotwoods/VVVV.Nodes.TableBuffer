#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

using System.Collections.Generic;
using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes.OpenNI
{

	#region PluginInfo
	[PluginInfo(Name = "Table", Category = "SpreadTable", Help = "Create an instance of a SpreadTable to be used elsewhere", Tags = "", AutoEvaluate = true)]
	#endregion PluginInfo
	public class SpreadTableNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Column names", DefaultString="x,y,z", IsSingle=true)]
		IDiffSpread<string> FPinInColumnNames;

		[Input("Auto save", IsSingle = true)]
		IDiffSpread<bool> FPinInAutoSave;

		[Input("Load", IsBang = true, IsSingle = true)]
		ISpread<bool> FPinInLoad;

		[Input("Save", IsBang = true, IsSingle = true)]
		ISpread<bool> FPinInSave;

		[Input("Filename", IsSingle = true, DefaultString="spreadtable.xml", StringType=StringType.Filename)]
		IDiffSpread<string> FPinInFilename;

		[Output("SpreadTable")]
		ISpread<SpreadTable> FPinOutTable;

		[Import()]
		ILogger FLogger;

		SpreadTable FTable = new SpreadTable();
		#endregion fields & pins

		[ImportingConstructor]
		public SpreadTableNode(IPluginHost host)
		{
			FTable.DataChanged += new SpreadTable.DataChangedHandler(FTable_DataChanged);
		}

		void FTable_DataChanged(Object sender, EventArgs e)
		{
			if (FAutosave)
				Save();
			FTable.SetupColumns(FColumnNames);			
		}

		bool FFirstRun = true;
		bool FAutosave = false;
		string FFilename = "spreadtable.xml";
		string FColumnNames = "x,y,z";
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FPinInColumnNames.IsChanged)
			{
				FColumnNames = FPinInColumnNames[0];
				FTable.SetupColumns(FColumnNames);
			}

			if (FPinInAutoSave.IsChanged)
			{
				FAutosave = FPinInAutoSave[0];
				if (FAutosave)
					Save();
			}

			if (FPinInFilename.IsChanged)
				FFilename = FPinInFilename[0];

			if (FPinInLoad[0])
				Load();

			if (FPinInSave[0])
				Save();

			if (FFirstRun)
			{
				if (FAutosave)
					Load();
				FPinOutTable[0] = FTable;
				FFirstRun = false;
			}
		}

		void Load()
		{
			if (FFilename != "")
			{
				try
				{
					FTable.Clear();
					FTable.ReadXmlSchema(FFilename);
					FTable.ReadXml(FFilename);
				}
				catch(Exception e)
				{
					FLogger.Log(e);
				}
			}
		}

		void Save()
		{
			if (FFilename != "")
				try
				{
					FTable.WriteXmlSchema(FFilename);
					FTable.WriteXml(FFilename);
				}
				catch (Exception e)
				{
					FLogger.Log(e);
				}
		}
	}
}
