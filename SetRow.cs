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
	[PluginInfo(Name = "SetRow", Category = "Table", Version = "Value", Help = "Set values in a SpreadTable (akin to S+H)", Tags = "", AutoEvaluate = true)]
	#endregion PluginInfo
	public class SetNode : IHasTable
	{
		#region fields & pins
		[Input("Input")]
		ISpread<ISpread<double>> FInput;

		[Input("Index")]
		ISpread<int> FIndex;

		[Input("Set", IsBang=true)]
		ISpread<ISpread<bool>> FSet;
		#endregion

		protected override void Evaluate2(int SpreadMax)
		{
			for (int i = 0; i < FSet.SliceCount; i++)
			{
				//check whether any are high
				if (SpectralOr(FSet[i]))
				{
					if (FIndex.SliceCount > 0)
						FData.Set(FInput[i], FIndex[i], FSet[i]);
					else
						FData.Set(FInput[i], 0, FSet[i]);
				}
			}
		}

		bool SpectralOr(ISpread<bool> spread)
		{
			bool value = false;
			foreach (bool slice in spread)
				value |= slice;
			return value;
		}
	}
}
