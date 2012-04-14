using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using System.ComponentModel.Composition;
using VVVV.Core.Logging;

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Insert", Category = "SpreadTable", Help = "Insert values into a SpreadTable", Tags = "", AutoEvaluate = true)]
	#endregion PluginInfo
	public class InsertNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		ISpread<ISpread<double>> FInput;

		[Input("Clear", IsBang = true, IsSingle = true)]
		ISpread<bool> FClear;

		[Input("Insert", IsBang = true)]
		ISpread<bool> FInsert;

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

			if (FClear[0])
			{
				FData.ClearAll();
			}

			for (int i = 0; i < FInsert.SliceCount; i++)
			{
				if (FInsert[i])
				{
					FData.Insert(FInput[i]);
				}
			}
		}
	}
}
