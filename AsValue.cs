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
	public class AsValueNode : IHasTable
	{
		#region fields & pins
		[Output("Output")]
		ISpread<ISpread<double>> FOutput;

		[Output("Count")]
		ISpread<int> FCount;

		bool FFreshData = false;
		#endregion

		protected override void Evaluate2(int SpreadMax)
		{
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
	}
}
