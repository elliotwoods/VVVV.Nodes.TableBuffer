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
	[PluginInfo(Name = "Insert", Help = "Insert values into a SpreadTable (akin to Queue)", Tags = "", AutoEvaluate = true)]
	#endregion PluginInfo
	public class InsertNode : IHasTable
	{
		#region fields & pins
		[Input("Input")]
		ISpread<ISpread<double>> FInput;

		[Input("Insert", IsBang = true, DefaultValue=-1)]
		ISpread<bool> FInsert;

		[Input("Remove", IsBang = true)]
		ISpread<bool> FRemove;

		[Input("Index")]
		ISpread<int> FIndex;

		#endregion

		protected override void Evaluate2(int SpreadMax)
		{
			if (FIndex.SliceCount > 0)
			{
				for (int i = 0; i < FInsert.SliceCount; i++)
				{
					if (FInsert[i])
					{
						FData.Insert(FInput[i], FIndex[i]);
					}
				}

				for (int i = 0; i < FRemove.SliceCount; i++)
				{
					if (FRemove[i])
					{
						if (FData.Rows.Count > 0)
						{
							int index = VVVV.Utils.VMath.VMath.Zmod(FIndex[i], FData.Rows.Count);
							FData.Rows.RemoveAt(index);
						}
					}
				}
			}
		}
	}
}
