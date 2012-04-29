using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using System.ComponentModel.Composition;
using VVVV.Core.Logging;

namespace VVVV.Nodes.TableBuffer
{
	#region PluginInfo
	[PluginInfo(Name = "AsValue", Category = "Table", Version = "Value", Help = "Convert SpreadTable to Values", Tags = "", AutoEvaluate = true)]
	#endregion PluginInfo
	public class AsValueNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Table", IsSingle = true)]
		ISpread<SpreadTable> FPinInTable;

		[Output("Output")]
		ISpread<ISpread<double>> FOutput;

		[Output("Count")]
		ISpread<int> FCount;

		[Import()]
		ILogger FLogger;

		bool FFreshData = false;

		SpreadTable FData;
		#endregion

		public void Evaluate(int SpreadMax)
		{
			if (FPinInTable[0] != FData)
			{
				FData = FPinInTable[0];
				if (FData != null)
				{
					FData.DataChanged += new SpreadTable.DataChangedHandler(FData_DataChanged);
				}
				FFreshData = true;
			}

			if (FData == null)
			{
				FOutput.SliceCount = 0;
				FCount[0] = 0;
				return;
			}

			if (FFreshData)
			{
				FData.GetSpread(FOutput);
				FCount[0] = FOutput.SliceCount;
			}
		}

		void FData_DataChanged(object sender, EventArgs e)
		{
			FFreshData = true; 
		}
	}
}
