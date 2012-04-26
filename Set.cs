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
	[PluginInfo(Name = "Set", Category = "SpreadTable", Help = "Set values in a SpreadTable (akin to S+H)", Tags = "", AutoEvaluate = true)]
	#endregion PluginInfo
	public class SetNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		ISpread<ISpread<double>> FInput;

		[Input("Set")]
		ISpread<bool> FSet;
		
		[Input("Table", IsSingle = true)]
		IDiffSpread<SpreadTable> FPinInTable;

		[Import()]
		ILogger FLogger;

		SpreadTable FData;
		#endregion

		public void Evaluate(int SpreadMax)
		{
			if (FPinInTable.IsChanged)
			{
				FData = FPinInTable[0];
			}

			if (FData == null)
				return;

			for (int i = 0; i < FSet.SliceCount; i++)
			{
				if (FSet[i])
				{
					FData.Set(i, FInput[i]);
				}
			}
		}
	}
}
